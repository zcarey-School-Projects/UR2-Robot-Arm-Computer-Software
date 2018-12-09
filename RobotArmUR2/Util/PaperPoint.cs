using System;
using System.Drawing;

namespace RobotArmUR2.Util {

	/// <summary>Used to easily pass points around that are meants to be relative points (i.e. between [0, 1]) on the sheet of paper.
	/// Helps distinguish regular image points (PointF) and paper points.
	/// Preferrably all coordinates are between [0, 1], however they are not limited to such.</summary>
	public class PaperPoint {

		/// <summary>The X coordinate</summary>
		public float X { get; set; }

		/// <summary>The Y coordinate.</summary>
		public float Y { get; set; }

		/// <summary>Creates a new point at 0, 0</summary>
		public PaperPoint() : this(0, 0) { }

		/// <summary>Creates a new coordinate at the specified location.</summary>
		/// <param name="x">The X position, preferrably between [0, 1]</param>
		/// <param name="y">The Y position, preferrably between [0, 1]</param>
		public PaperPoint(float x, float y) {
			this.X = x;
			this.Y = y;
		}

		/// <summary> Creates a deep copy of another point, so changing one won't change the other.</summary>
		/// <param name="point">The point to copy.</param>
		public PaperPoint(PaperPoint point) {
			SetPoint(point);
		}

		/// <summary>Sets this coordinate equal to that of another. Same as doing a deep copy.</summary>
		/// <param name="point">The point to copy.</param>
		public void SetPoint(PaperPoint point) {
			X = point.X;
			Y = point.Y;
		}

		/// <summary>Given a pixel coordinate and the size of the image, sets this points to its relative coordinate.</summary>
		/// <param name="point">The pixel coordinate point.</param>
		/// <param name="imgSize">The size of the image.</param>
		public void SetPoint(PointF point, Size imgSize) {
			X = point.X / imgSize.Width;
			Y = point.Y / imgSize.Height;
		}

		/// <summary>Given a pixel coordinate and the size of the image, sets this points to its relative coordinate.</summary>
		/// <param name="point">The pixel coordinate point.</param>
		/// <param name="imgSize">The size of the image.</param>
		public void SetPoint(Point point, Size imgSize) {
			SetPoint((PointF)point, imgSize);
		}

		/// <summary> Given the size of an image, return the coordinate point that this point represents. </summary>
		/// <param name="size">Size of the image</param>
		/// <returns></returns>
		public PointF GetScreenCoord(Size size) {
			return new PointF((size.Width + 1) * X, (size.Height + 1) * Y);
		}

		/// <summary>Returns GetScreenCoord, but clipped to the size of the image.</summary>
		/// <param name="screenSize"></param>
		/// <returns></returns>
		public PointF GetClippedScreenCoord(Size screenSize) {
			PointF coord = GetScreenCoord(screenSize);
			coord.X = Math.Max(0, Math.Min(screenSize.Width - 1, coord.X));
			coord.Y = Math.Max(0, Math.Min(screenSize.Height - 1, coord.Y));

			return coord;
		}

		/// <summary>Returns the point in the form of a string.</summary>
		/// <returns></returns>
		public override string ToString() {
			return "{ " + X.ToString("N3") + " , " + Y.ToString("N3") + " }";
		}

		/// <summary>Returns the point in the form of a string with the specified digits.</summary>
		/// <param name="digits"></param>
		/// <returns></returns>
		public string ToString(uint digits) {
			return "{ " + X.ToString("N" + digits) + " , " + Y.ToString("N" + digits) + " }";
		}


		public static implicit operator PointF(PaperPoint pt) {
			return new PointF(pt.X, pt.Y);
		}

		public static implicit operator PaperPoint(PointF pt) {
			return new PaperPoint(pt.X, pt.Y);
		}

	}
}
