using Emgu.CV.Structure;
using RobotArmUR2.Util;
using System.Collections.Generic;
using System.Drawing;

namespace RobotArmUR2.VisionProcessing {
	public class DetectedShapes {

		public List<Triangle2DF> Triangles { get; }
		public List<RotatedRect> Squares { get; }
		public List<PaperPoint> RelativeTrianglePoints { get; }
		public List<PaperPoint> RelativeSquarePoints { get; }

		public DetectedShapes() {
			this.Triangles = new List<Triangle2DF>();
			this.Squares = new List<RotatedRect>();
			this.RelativeTrianglePoints = new List<PaperPoint>();
			this.RelativeSquarePoints = new List<PaperPoint>();
		}

		public DetectedShapes(List<Triangle2DF> Triangles, List<RotatedRect> Squares, Size ImageSize) {
			this.Triangles = Triangles;
			this.Squares = this.Squares = Squares;
			this.RelativeTrianglePoints = new List<PaperPoint>(Triangles.Count);
			this.RelativeSquarePoints = new List<PaperPoint>(Squares.Count);

			foreach(Triangle2DF triangle in Triangles) {
				this.RelativeTrianglePoints.Add(convertCoord(triangle.Centeroid, ImageSize));
			}

			foreach (RotatedRect square in Squares) {
				this.RelativeSquarePoints.Add(convertCoord(square.Center, ImageSize));
			}
		}

		private static PaperPoint convertCoord(PointF center, Size imageSize) {
			return new PaperPoint(center.X / imageSize.Width, center.Y / imageSize.Height);
		}

	}
}
