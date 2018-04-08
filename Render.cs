using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrbitTracer
{
	public class Render
	{
		double[,] data;

		public async Task RenderToCanvas(ICanvas canvas, FracConfig config)
		{
			int width = canvas.Width;
			int height = canvas.Height;

			if (data == null || data.GetLength(0) != width || data.GetLength(1) != height) {
				data = new double[width,height];
			} else {
				for(int y=0; y<height; y++) {
					for(int x=0; x<width; x++) {
						data[x,y] = 0;
					}
				}
			}

			var taskList = new List<Task>(height*width);

			Console.WriteLine("spooling tasks "+height);
			for(int y=0; y<height; y++) {
				var task = RenderRowAsync(config,y,width,height,data);
				taskList.Add(task);
			}

			Console.WriteLine("before wait");
			await Task.WhenAll(taskList);
			Console.WriteLine("after wait");

			double min = double.MaxValue,max = double.MinValue;
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = data[x,y];
					if (d > 0) { d = Math.Log10(d); }
					if (d < min) { min = d; }
					if (d > max) { max = d; }
				}
			}
			double range = Math.Abs(max - min);
			double mult = 255.0/range;

			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = data[x,y];
					if (d > 0) { d = Math.Log10(d); }
					Color c;
					if (d <= 0) {
						c = Color.Black;
					} else {
						double q = d*mult - min;
						//int w = (int)Math.Min(255.0,Math.Max(0,q));
						int w = (int)q;
						c = Color.FromArgb(w,w,w);
					}
					try {
						canvas.SetPixel(x,y,c);
					} catch {
						Console.WriteLine("!! Trying to set x="+x+" y="+y+" c="+c+" w="+width+" h="+height);
						throw;
					}
				}
			}
		}

		public Task RenderOrbitAsync(ICanvas canvas, FracConfig conf, int x, int y, Color highlight)
		{
			return Task.Run(() => {
				RenderOrbitToBitmap(canvas,conf,x,y,highlight);
			});
		}

		static void RenderOrbitToBitmap(ICanvas canvas, FracConfig conf, int x, int y, Color highlight)
		{
			int wth = canvas.Width;
			int hth = canvas.Height;

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);

			Complex[] points = new Complex[conf.IterMax];
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape);

			for (int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real, conf.Resolution, wth, conf.OffsetX);
				int by = WorldToWin(f.Imaginary, conf.Resolution, hth, conf.OffsetY);
				if (bx > 0 && bx < wth && by > 0 && by < hth) {
					canvas.SetPixel(bx,by,highlight);
				}
			}

			canvas.SetPixel(x,y,Color.Blue);
		}

		static Task RenderRowAsync(FracConfig conf, int y, int wth, int hth, double[,] data)
		{
			return Task.Run(() => {
				RenderRow(conf,y,wth,hth,data);
			});
		}

		static void RenderRow(FracConfig conf, int y, int wth, int hth, double[,] data)
		{
			for(int x = 0; x<wth; x++)
			{
				RenderPart(conf,x,y,wth,hth,data);
			}
		}

		static void InitZC(FracConfig conf, int x, int y, int wth, int hth, out Complex z, out Complex c)
		{
			double cx = WinToWorld(x, conf.Resolution, wth, conf.OffsetX);
			double cy = WinToWorld(y, conf.Resolution, hth, conf.OffsetY);

			switch(conf.Plane)
			{
			case Planes.XY: default:
				c = new Complex(cx,cy);
				z = new Complex(conf.X,conf.Y); break;
			case Planes.XW:
				c = new Complex(conf.W,cy);
				z = new Complex(conf.X,cx); break;
			case Planes.XZ:
				c = new Complex(cx,conf.Z);
				z = new Complex(conf.X,cy); break;
			case Planes.YW:
				c = new Complex(conf.W,cx);
				z = new Complex(cy,conf.Y); break;
			case Planes.YZ:
				c = new Complex(cx,conf.Z);
				z = new Complex(cy,conf.Y); break;
			case Planes.WZ:
				c = new Complex(conf.W,conf.Z);
				z = new Complex(cx,cy); break;
			}
		}

		static void RenderPart(FracConfig conf, int x, int y, int wth, int hth, double[,] data)
		{
			//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
			//int hxres = data.GetLength(0);
			//int hyres = data.GetLength(1);

			//double xoff = -0.8, yoff = -0.5;
			//Complex res = new Complex((double)wth,(double)hth);

			Complex z,c;
			InitZC(conf,x,y,wth,hth,out z,out c);

			Complex[] points = new Complex[conf.IterMax];
			int escapeiter = FillOrbit(points,conf.IterMax,z,c,conf.Escape);

			for(int iter = 0; iter < escapeiter; iter++)
			{
				Complex f = points[iter];
				int bx = WorldToWin(f.Real,conf.Resolution,wth,conf.OffsetX);
				int by = WorldToWin(f.Imaginary,conf.Resolution,hth,conf.OffsetY);
				if (bx > 0 && bx < wth && by > 0 && by < hth) {
					InterlockedAdd(ref data[bx,by],1);
				}
			}

			//for(iter = 0; iter < conf.IterMax; iter++) {
			//
			//	//z = Complex.Pow(z*z,c*4)+c;
			//	z = z*z + c;
			//	int bx = WorldToWin(z.Real,conf.Scale,hxres,conf.OffsetX);
			//	int by = WorldToWin(z.Imaginary,conf.Scale,hyres,conf.OffsetY);
			//	if (bx > 0 && bx < wth && by > 0 && by < hth) {
			//		InterlockedAdd(ref data[bx,by],1);
			//	}
			//
			//	var dist = z.Magnitude;
			//	if (dist > conf.Escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
			//}
			////smooth coloring
			//double index = iter;
			//if (iter < itermax)
			//{
			//	//double zn = Math.Sqrt(z.Real*z.Real+z.Imaginary*z.Imaginary);
			//	double zn = z.Magnitude;
			//	double nu = Math.Log(Math.Log(zn,2),2);
			//	index = iter + 1.0 - nu;
			//}
			//data[hx,hy] = index;
		}

		static int FillOrbit(Complex[] points, int itermax, Complex z, Complex c, double escape)
		{
			int iter;
			for(iter = 0; iter < itermax; iter++) {
				z = z*z + c;

				points[iter] = z;

				var dist = z.Magnitude;
				if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
			}

			return iter;
		}

		static double WinToWorld(int v, double magnify, int res, int offset)
		{
			//return (v + offset)/(double)res / magnify;
			//return (((double)v) / ((double)res) + offset) / magnify;
			return (v - offset) / magnify;

		}
		static int WorldToWin(double v, double magnify, int res, int offset)
		{
			//return (int)Math.Round(res * magnify * v) - offset;
			//return (int)Math.Round((double)res * (magnify * v - offset));
			return (int)Math.Round(v * magnify) + offset;
		}

		static double InterlockedAdd(ref double location1, double value)
		{
			double newCurrentValue = location1; // non-volatile read, so may be stale
			while (true)
			{
				double currentValue = newCurrentValue;
				double newValue = currentValue + value;
				newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
				if (newCurrentValue == currentValue)
					return newValue;
			}
		}
	}
}
