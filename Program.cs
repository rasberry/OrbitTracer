using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrbitTracer
{
	class Program
	{
		static void Main(string[] args)
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);

			try {
				MainMain(args).GetAwaiter().GetResult();
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		static async Task MainMain(string[] args)
		{
			if (args.Length < 1) {
				Application.Run(new MainForm());
			} else {
				if (!ProcessArgs(args)) { return; }
				await MainCmd();
			}
		}

		static async Task MainCmd()
		{
			if (ShouldCreateOrbits) {
				ProduceOrbits();
			} else {
				var ren = new Render();
				var bmp = new BitmapCanvas(Width,Height);
				var conf = new FracConfig {
					Escape = 4.0,
					Plane = Planes.XY,
					Resolution = Resolution,
					X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
					IterMax = 1000,
					OffsetX = Width/2,
					OffsetY = Height/2
				};
				await ren.RenderToCanvas(bmp,conf);
				bmp.SavePng(FileName+".png");
			}
		}

		static void ProduceOrbits() {

		}

		static bool ShouldCreateOrbits = false;
		static int Width = -1;
		static int Height = -1;
		//static double RealMin = double.NaN;
		//static double RealMax = double.NaN;
		//static double ImagMin = double.NaN;
		//static double ImagMax = double.NaN;
		static string FileName = null;
		static double Resolution = 200;

		static bool ProcessArgs(string[] args)
		{
			bool showHelp = false;
			bool noChecks = false;
			for(int a=0; a<args.Length; a++)
			{
				string c = args[a];
				if (c == "--help" || c == "-h")
				{
					showHelp = true;
					noChecks = true;
				}
				else if (c == "-o")
				{
					ShouldCreateOrbits = true;
				}
				else if (c == "-d" && (a += 2) < args.Length)
				{
					string sw = args[a - 1];
					string sh = args[a - 0];
					if (!int.TryParse(sw, out Width))
					{
						Console.WriteLine("Invalid width " + sw);
						showHelp = true;
					}
					if (!int.TryParse(sh, out Height))
					{
						Console.WriteLine("Invalid height " + sh);
						showHelp = true;
					}
				}
				//else if (c == "-s" && (a += 4) < args.Length)
				//{
				//	string ri = args[a - 3];
				//	string rx = args[a - 2];
				//	string ii = args[a - 1];
				//	string ix = args[a - 0];
				//	if (!double.TryParse(ri, out RealMin))
				//	{
				//		Console.WriteLine("Invalid Real Min " + ri);
				//		showHelp = true;
				//	}
				//	if (!double.TryParse(rx, out RealMax))
				//	{
				//		Console.WriteLine("Invalid Real Max " + rx);
				//		showHelp = true;
				//	}
				//	if (!double.TryParse(ii, out ImagMin))
				//	{
				//		Console.WriteLine("Invalid Imaginary Min " + ii);
				//		showHelp = true;
				//	}
				//	if (!double.TryParse(ix, out ImagMax))
				//	{
				//		Console.WriteLine("Invalid Imaginary Max " + ix);
				//		showHelp = true;
				//	}
				//}
				else if (c == "-r" && ++a < args.Length)
				{
					string res = args[a];
					if (!double.TryParse(res,out Resolution)) {
						Console.WriteLine("Invalid resolution "+res);
						showHelp = true;
					}
				}
				else
				{
					FileName = c;
				}
			}

			if (!noChecks)
			{
				//sanity checks
				if (String.IsNullOrWhiteSpace(FileName)) {
					Console.WriteLine("E: Missing filename / prefix");
					showHelp = true;
				}
				if (Width < 1 || Height < 1) {
					Console.WriteLine("E: output image size is invalid");
					showHelp = true;
				}
				//if (RealMin > RealMax) {
				//	double temp = RealMin;
				//	RealMin = RealMax;
				//	RealMax = temp;
				//}
				//if (RealMax - RealMin < double.Epsilon) {
				//	Console.WriteLine("E: Real number range must be bigger than zero");
				//	showHelp = true;
				//}
				//if (ImagMin > ImagMax) {
				//	double temp = ImagMin;
				//	ImagMin = ImagMax;
				//	ImagMax = temp;
				//	if (ImagMax - ImagMin < double.Epsilon) {
				//		Console.WriteLine("E: Imaginary number range must be bigger than zero");
				//		showHelp = true;
				//	}
				//}
				if (Resolution < double.Epsilon) {
					Console.WriteLine("E: Resolution must be greater than zero");
					showHelp = true;
				}
			}

			if (showHelp) {
				Console.WriteLine("\n"+nameof(OrbitTracer)+" [options] (filename / prefix)"
					+"\nOptions:"
					+"\n --help / -h                       Show this help"
					//+"\n -s (rmin) (rmax) (imin) (imax)    Dimensions of space to render - real and imaginary"
					+"\n -d (width) (height)               Size of image output images in pixels"
					+"\n -r (resolution)                   Scale factor (Default: 200. 400 = 2x bigger)"
					+"\n -o                                Output orbits instead of nebulabrot"
					+"\n                                    (Warning: produces one image per coordinate)"
				);
			}
			return !showHelp;
		}
	}
}
