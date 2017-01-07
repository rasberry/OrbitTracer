using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrbitTracer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			this.Text = nameof(OrbitTracer);
		}

		Render Renderer;
		FracConfig Config;
		Bitmap Canvas;
		Bitmap Highlight;
		Bitmap Fractal;

		private async void mainform_Load(object sender, EventArgs e)
		{
			this.Show();

			Renderer = new Render();
			Config = new FracConfig {
				Escape = 4.0,
				Plane = Planes.XY,
				Scale = 1.0/3.0,
				X = 0.0, Y = 0.0, W = 0.0, Z = 0.0,
				IterMax = 1000,
				OffsetX = -0.7, OffsetY = -0.5
			};

			int w = this.pictureBox1.Width;
			int h = this.pictureBox1.Height;

			Fractal = new Bitmap(w,h,PixelFormat.Format32bppArgb);
			Highlight = new Bitmap(w,h,PixelFormat.Format32bppArgb);
			Canvas = new Bitmap(w,h,PixelFormat.Format32bppArgb);

			await Renderer.RenderToBitmap(Fractal,Config);
			this.pictureBox1.Image = Fractal;
		}

		static int Min(int one,int two,int three)
		{
			int temp = one;
			if (two < temp) { temp = two; }
			if (three < temp) { temp = three; }
			return temp;
		}

		static void JoinBitmaps(Bitmap bottom, Bitmap top, Bitmap dest)
		{
			int w = Min(bottom.Width,top.Width,dest.Width);
			int h = Min(bottom.Height,top.Height,dest.Height);

			using (var b = new FastBitmap.LockBitmap(bottom))
			using (var t = new FastBitmap.LockBitmap(top))
			using (var d = new FastBitmap.LockBitmap(dest))
			{
				b.LockBits();
				t.LockBits();
				d.LockBits();

				for(int y=0; y<h; y++) {
					for(int x=0; x<w; x++) {
						Color bc = b.GetPixel(x,y);
						Color tc = t.GetPixel(x,y);
						Color fin = tc.ToArgb() == 0 ? bc : tc;
						d.SetPixel(x,y,fin);
					}
				}

				d.UnlockBits();
				t.UnlockBits();
				b.UnlockBits();
			}
		}

		static void ClearBitmap(Bitmap img)
		{
			Color c = Color.FromArgb(0);
			using (var b = new FastBitmap.LockBitmap(img))
			{
				b.LockBits();
				for(int y=0; y<img.Height; y++) {
					for(int x=0; x<img.Width; x++) {
						b.SetPixel(x,y,c);
					}
				}
				b.UnlockBits();
			}
		}

		static bool HighlightIsRendering = false;
		private async void pictureBox1_MouseClick(object sender, MouseEventArgs e)
		{

			if (HighlightIsRendering) { return; }
			HighlightIsRendering = true;
			//var loc = this.pictureBox1.PointToScreen(e.Location);
			//var loc = this.pictureBox1.PointToClient(e.Location);
			//var loc = this.pictureBox1.Location;
			//loc.X = e.X - loc.X;
			//loc.Y = e.Y - loc.Y;
			var loc = e.Location;

			ClearBitmap(Highlight);
			await Renderer.RenderOrbitAsync(Highlight,Config,loc.X,loc.Y,Color.Red);
			JoinBitmaps(Fractal,Highlight,Canvas);
			this.pictureBox1.Image = Canvas;
			this.pictureBox1.Refresh();
			HighlightIsRendering = false;
		}

		//private async void pictureBox1_Click(object sender, EventArgs e)
		//{
		//	e.
		//
		//	ClearBitmap(Highlight);
		//	await Renderer.RenderOrbitAnyc(Highlight,Config,
		//}
	}
}
