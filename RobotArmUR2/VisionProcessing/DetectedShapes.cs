using Emgu.CV.Structure;
using RobotArmUR2.Util;
using System.Collections.Generic;
using System.Drawing;


namespace RobotArmUR2.VisionProcessing {

	/// <summary> Retains the detected shapes from image processing and their location on the image. </summary>
	public class DetectedShapes {

		/// <summary> Triangles detected at their pixel location on the image. </summary>
		public List<Triangle2DF> Triangles { get; }

		/// <summary> Squares detected at their pixel location on the image. </summary>
		public List<RotatedRect> Squares { get; }

		/// <summary> Triangles detected at their relative location on the paper. (All coordinates between [0, 1]) </summary>
		public List<PaperPoint> RelativeTrianglePoints { get; }

		/// <summary> Squares detected at their relative location on the paper. (All coordinates between [0, 1]) </summary>
		public List<PaperPoint> RelativeSquarePoints { get; }

		/// <summary> Creates an empty object. </summary>
		public DetectedShapes() {
			this.Triangles = new List<Triangle2DF>();
			this.Squares = new List<RotatedRect>();
			this.RelativeTrianglePoints = new List<PaperPoint>();
			this.RelativeSquarePoints = new List<PaperPoint>();
		}

		/// <summary> Saves the lists and converts their coordinates. </summary>
		/// <param name="Triangles"> Detected triangles. </param>
		/// <param name="Squares"> Detected squares. </param>
		/// <param name="ImageSize"> Size of image they were detected on. </param>
		public DetectedShapes(List<Triangle2DF> Triangles, List<RotatedRect> Squares, Size ImageSize) {
			if (ImageSize == null) ImageSize = new Size(1, 1);
			this.Triangles = (Triangles == null) ? (new List<Triangle2DF>()) : Triangles;
			this.Squares = (Squares == null) ? (new List<RotatedRect>()) : Squares;
			this.RelativeTrianglePoints = new List<PaperPoint>(Triangles.Count);
			this.RelativeSquarePoints = new List<PaperPoint>(Squares.Count);

			foreach(Triangle2DF triangle in this.Triangles) {
				this.RelativeTrianglePoints.Add(convertCoord(triangle.Centeroid, ImageSize));
			}

			foreach (RotatedRect square in this.Squares) {
				this.RelativeSquarePoints.Add(convertCoord(square.Center, ImageSize));
			}
		}

		//Converts an absolute pixel coordinate into a relative paper coordinate.
		private static PaperPoint convertCoord(PointF center, Size imageSize) {
			return new PaperPoint(center.X / imageSize.Width, center.Y / imageSize.Height);
		}

	}
}
