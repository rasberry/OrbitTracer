using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitTracer
{
	public interface ICanvas
	{
		int Width { get; }
		int Height { get; }
		void SetPixel(int x,int y,Color c);
		Color GetPixel(int x,int y);
		Bitmap Source { get; }
		void SavePng(string fileName);
	}

	public class BitmapCanvas : ICanvas, IDisposable
	{
		public BitmapCanvas(int width,int height)
		{
			Width = width;
			Height = height;
			bitmap = new Bitmap(width,height,PixelFormat.Format32bppArgb);
			fast = new FastBitmap.LockBitmap(bitmap);
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public void SetPixel(int x, int y, Color c)
		{
			if (!isLocked) { fast.LockBits(); isLocked = true;}
			fast.SetPixel(x,y,c);
		}

		public Color GetPixel(int x, int y)
		{
			if (!isLocked) { fast.LockBits(); isLocked = true;}
			return fast.GetPixel(x,y);
		}

		public Bitmap Source { get {
			if (isLocked) { fast.UnlockBits(); isLocked = false; }
			return bitmap;
		} }

		public void SavePng(string filename)
		{
			if (isLocked) { fast.UnlockBits(); isLocked = false; }
			bitmap.Save(filename,ImageFormat.Png);
		}

		public void Dispose()
		{
			fast.Dispose();
		}

		Bitmap bitmap;
		FastBitmap.LockBitmap fast;
		bool isLocked;
	}
}
