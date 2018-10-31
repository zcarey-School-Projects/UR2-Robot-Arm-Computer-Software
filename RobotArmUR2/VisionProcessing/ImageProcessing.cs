using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {
	public class ImageProcessing {

		private static readonly object visionLock = new object();

		private Image<Gray, byte> paperMask;
		//public PointF[] paperMaskPoints { get; set; } = new PointF[] { new PointF(0, 0), new PointF(1, 0), new Point(1, 1), new Point(0, 1) };
		public PaperCalibration paperMaskPoints { get; set; } = new PaperCalibration();
		public bool paperMaskDirty = true;

		public Image<Gray, byte> grayImage { get; private set; }
		public Image<Gray, byte> cannyImage { get; private set; }
		private UMat cannyEdges = new UMat();

		public List<Triangle2DF> triangleList { get; private set; } = new List<Triangle2DF>();
		public List<RotatedRect> boxList { get; private set; } = new List<RotatedRect>();

		private Bgr TrigColor = new Bgr(Color.Yellow);
		private Bgr RectColor = new Bgr(Color.Red);

		public Bgr TriangleColor { get { return new Bgr(TrigColor.Blue, TrigColor.Green, TrigColor.Red); } set { TrigColor = value; } }
		public Bgr RectangleColor { get { return new Bgr(RectColor.Blue, RectColor.Green, RectColor.Red); } set { RectColor = value; } }
		public int TriangleThickness { get; set; } = 5;
		public int RectangleThickness { get; set; } = 5;

		public int RectangleMinimumAngle { get; set; } = 80;
		public int RectangleMaximumAngle { get; set; } = 100;

		public double CannyThreLinking { get; set; } = 120.0;
		public double CannyThres { get; set; } = 180.0;

		public ImageProcessing() {
				
		}

		~ImageProcessing() {
			Dispose();
		}

		public void Dispose() {
			if (paperMask != null) paperMask.Dispose();
			//paperMaskPoints = new PointF[] { };
			paperMaskPoints = null;
			if (grayImage != null) grayImage.Dispose();
			if (cannyImage != null) cannyImage.Dispose();
			cannyEdges = null;
			triangleList.Clear();
			boxList.Clear();
		}

		public void MaskImage<TColor>(Image<TColor, byte> image) where TColor : struct, IColor {
			MaskImage(image, 0);
		}

		public void MaskImage<TColor>(Image<TColor, byte> image, byte MaskColor) where TColor : struct, IColor {
			lock (visionLock) {
				if ((paperMask == null) || (paperMaskDirty) || (image.Width != paperMask.Width) || (image.Height != paperMask.Height)) {
					paperMask = GeneratePaperMask(image.Width, image.Height);
					paperMaskDirty = false;
				}

				//Mask the gray image.
				for (int y = 0; y < image.Height; y++) {
					for (int x = 0; x < image.Width; x++) {
						if (paperMask.Data[y, x, 0] < (255 / 2)) {
							image.Data[y, x, 0] = MaskColor;
						}
					}
				}
			}
		}

		private Image<Gray, byte> GeneratePaperMask(int width, int height) {
			Image<Gray, byte> newMask = new Image<Gray, byte>(width, height);

			/*if (paperMaskPoints == null) {
				paperMask.Draw(new Rectangle(0, 0, paperMask.Width, paperMask.Height), new Gray(255));
			} else {*/
				Point[] points = new Point[4];
			/*for (int i = 0; i < paperMaskPoints.Length; i++) {
				points[i] = new Point((int)(width * paperMaskPoints[i].X), (int)(height * paperMaskPoints[i].Y));
			}*/
			points[0] = new Point((int)(width * paperMaskPoints.BL.X), (int)(height * paperMaskPoints.BL.Y));
			points[1] = new Point((int)(width * paperMaskPoints.TL.X), (int)(height * paperMaskPoints.TL.Y));
			points[2] = new Point((int)(width * paperMaskPoints.TR.X), (int)(height * paperMaskPoints.TR.Y));
			points[3] = new Point((int)(width * paperMaskPoints.BR.X), (int)(height * paperMaskPoints.BR.Y));
			//paperMask.DrawPolyline(points, true, new Gray(255), -1);
			newMask.FillConvexPoly(points, new Gray(255));
			//}

			return newMask;
		}

		public void MaskInputImage(Image<Bgr, byte> input) {
			grayImage = input.Convert<Gray, byte>();
			MaskImage(grayImage, 255);
			grayImage = grayImage.ThresholdBinary(new Gray(255 / 2), new Gray(255));
		}

		public void FindShapesInMaskedImage() {
			CannyEdgeDetection();
			DetectShapes();
		}

		private void CannyEdgeDetection() {
			cannyEdges = new UMat();
			LineSegment2D[] lines;
			CvInvoke.Canny(grayImage, cannyEdges, CannyThres, CannyThreLinking);
			lines = CvInvoke.HoughLinesP(cannyEdges,
				1, //Distance resolution in pixel-related units
				Math.PI / 45.0, //Angle resolution measured in radians
				20, //threshold
				30, //min line width
				10); //gap between lines
			cannyImage = grayImage.CopyBlank();
			foreach (LineSegment2D line in lines) {
				cannyImage.Draw(line, new Gray(255), 2);
			}
		}

		private void DetectShapes() {
			triangleList = new List<Triangle2DF>();
			boxList = new List<RotatedRect>();
			//Image<Bgr, byte> triangleRectImage = origImage.CopyBlank();
			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			CvInvoke.FindContours(cannyEdges, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
			int count = contours.Size;
			for (int i = 0; i < count; i++) {
				VectorOfPoint contour = contours[i];
				VectorOfPoint approxContour = new VectorOfPoint();
				CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
				if (CvInvoke.ContourArea(approxContour, false) > 250) { //only consider areas that are large enough.
					if (approxContour.Size == 3) { //Three vertices, must be a triangle!
						Point[] pts = approxContour.ToArray();
						triangleList.Add(new Triangle2DF(pts[0], pts[1], pts[2]));
					} else if (approxContour.Size == 4) { //Four vertices, must be a square!
						#region Determine if all angles are within 80-100 degrees
						bool isRectangle = true;
						Point[] pts = approxContour.ToArray();
						LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
						for (int j = 0; j < edges.Length; j++) {
							double dAngle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
							if (dAngle < RectangleMinimumAngle || dAngle > RectangleMaximumAngle) {
								isRectangle = false;
								break;
							}
						}
						#endregion

						if (isRectangle) {
							boxList.Add(CvInvoke.MinAreaRect(approxContour));
						}
					}
				}
			}
		}

		public bool AutoDetectPaper(Image<Bgr, byte> image) {
			grayImage = image.Convert<Gray, byte>();
			CannyEdgeDetection();
			DetectShapes();

			if (boxList.Count == 0) {
				return false;
			}

			//Find the largest rectangle
			float largestSize = 0;
			RotatedRect largest = boxList[0]; //Assign initial values to avoid compile errors.
			foreach (RotatedRect box in boxList) {
				float area = box.Size.Width * box.Size.Height;
				if (area > largestSize) {
					largest = box;
					largestSize = area;
				}
			}

			//Organize Points
			List<PointF> points = new List<PointF>();
			points.AddRange(largest.GetVertices());

			PaperCalibration paper = new PaperCalibration();
			paper.BL = new PointF(points[0].X / image.Width, points[0].Y / image.Height);
			paper.TL = new PointF(points[1].X / image.Width, points[1].Y / image.Height);
			paper.TR = new PointF(points[2].X / image.Width, points[2].Y / image.Height);
			paper.BR = new PointF(points[3].X / image.Width, points[3].Y / image.Height);

			paper.SortPointOrder();

			/*
			List<PointF> points = new List<PointF>();
			points.AddRange(largest.GetVertices());
			PointF[] leftPoints = new PointF[2];

			for (int i = 0; i < 2; i++) {
				float leftVal = float.MaxValue;
				foreach (PointF point in points) {
					if (point.X < leftVal) {
						leftVal = point.X;
						leftPoints[i] = point;
					}
				}
				points.Remove(leftPoints[i]);
			}
			
			if (leftPoints[0].Y < leftPoints[1].Y) {
				paperMaskPoints[0] = leftPoints[0];
				paperMaskPoints[3] = leftPoints[1];
			} else {
				paperMaskPoints[0] = leftPoints[1];
				paperMaskPoints[3] = leftPoints[0];
			}

			if (points[0].Y < points[1].Y) {
				paperMaskPoints[1] = points[0];
				paperMaskPoints[2] = points[1];
			} else {
				paperMaskPoints[1] = points[1];
				paperMaskPoints[2] = points[0];
			}

			//Calculate correct point position
			for (int i = 0; i < paperMaskPoints.Length; i++) {
				paperMaskPoints[i].X /= image.Width;
				paperMaskPoints[i].Y /= image.Height;
			}
			*/
			return true;
		}

		public void DrawFoundShapes(Image<Bgr, byte> image) {
			foreach (Triangle2DF triangle in triangleList) {
				image.Draw(triangle, TrigColor, TriangleThickness);
			}

			foreach (RotatedRect box in boxList) {
				image.Draw(box, RectColor, RectangleThickness);
			}
		}


	}
}
