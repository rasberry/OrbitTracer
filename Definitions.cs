using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitTracer
{
	public enum Planes { XY, XW, XZ, YW, YZ, WZ }
	public struct FracConfig
	{
		public double X;
		public double Y;
		public double W;
		public double Z;
		public Planes Plane;
		public double Scale;
		public double Escape;
		public int IterMax;
		public double OffsetX;
		public double OffsetY;

		public static FracConfig Default { get {
			return new FracConfig {
				X = 0.0,
				Y = 0.0,
				W = 0.0,
				Z = 0.0,
				Plane = Planes.XY,
				Scale = 1.0,
				Escape = 4.0,
				IterMax = 100
			};
		}}
	}
}
