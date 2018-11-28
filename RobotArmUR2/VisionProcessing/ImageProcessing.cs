

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace RobotArmUR2.VisionProcessing {
	public static class ImageProcessing {

		public static Image<Gray, TDepth> GetGrayImage<TColor, TDepth>(Image<TColor, TDepth> input) where TColor : struct, IColor where TDepth : new() {
			return input.Convert<Gray, TDepth>();
		}

		public static Image<TColor, TDepth> GetThresholdImage<TColor, TDepth>(Image<TColor, TDepth> input, TColor threshold, TColor MaxValue) where TColor : struct, IColor where TDepth : new() {
			return input.ThresholdBinary(threshold, MaxValue);
		}

		//Warps the ThresholdImage so the calibrated corners take up the entire image.
		public static Image<TColor, TDepth> GetWarpedImage<TColor, TDepth>(Image<TColor, TDepth> image, PaperCalibration paperCalibration) where TColor : struct, IColor where TDepth : new() { //TODO follow format of other methods
			PointF[] paperPoints = new PointF[4]; //TODO add a "Calibration.ToScreenCoordArray(Size);"
			paperPoints[0] = paperCalibration.BottomLeft.GetScreenCoord(image.Size); //Yay implicit operators :)
			paperPoints[1] = paperCalibration.TopLeft.GetScreenCoord(image.Size);
			paperPoints[2] = paperCalibration.TopRight.GetScreenCoord(image.Size);
			paperPoints[3] = paperCalibration.BottomRight.GetScreenCoord(image.Size);
			Image<TColor, TDepth> warpedImage = new Image<TColor, TDepth>(550, 425); //Should be close to aspect ratio of a piece of 8.5 x 11 paper.
			PointF[] targetPoints = new PointF[] { new PointF(0, warpedImage.Height - 1), new PointF(0, 0), new PointF(warpedImage.Width - 1, 0), new PointF(warpedImage.Width - 1, warpedImage.Height - 1) };

			using (var matrix = CvInvoke.GetPerspectiveTransform(paperPoints, targetPoints)) {
				CvInvoke.WarpPerspective(image, warpedImage, matrix, warpedImage.Size, Emgu.CV.CvEnum.Inter.Cubic);
			}

			return warpedImage;
		}

		public static UMat EdgeDetection<TColor, TDepth>(Image<TColor, TDepth> input) where TColor : struct, IColor where TDepth : new() {
			UMat cannyEdges = new UMat();
			CvInvoke.Canny(input, cannyEdges, 180.0, 120.0); //TODO crashed, may need to add checks
			return cannyEdges;
		}

		public static VectorOfVectorOfPoint FindContours(UMat Edges){
			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			CvInvoke.FindContours(Edges, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
			return contours;
		}

		public static IEnumerable GetContourEnumerable(VectorOfVectorOfPoint contours) {
			return new ContourList(contours);
		}

		public static DetectedShapes DetectShapes(UMat Edges) {
			return DetectShapes(FindContours(Edges));
		}

		public static DetectedShapes DetectShapes(VectorOfVectorOfPoint contours) {
			DetectedShapes shapes = new DetectedShapes();

			foreach (VectorOfPoint contour in GetContourEnumerable(contours)) {
				VectorOfPoint approxContour = new VectorOfPoint();
				CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
				if (CvInvoke.ContourArea(approxContour, false) > 250) { //only consider areas that are large enough.
					if (approxContour.Size == 3) { //Three vertices, must be a triangle!
						Point[] pts = approxContour.ToArray();
						shapes.Triangles.Add(new Triangle2DF(pts[0], pts[1], pts[2]));
					} else if (approxContour.Size == 4) { //Four vertices, must be a square!
						#region Determine if all angles are within 80-100 degrees
						bool isRectangle = true;
						Point[] pts = approxContour.ToArray();
						LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
						for (int j = 0; j < edges.Length; j++) {
							double dAngle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
							/*if (dAngle < RectangleMinimumAngle || dAngle > RectangleMaximumAngle) {
								isRectangle = false;
								break; //TODO cleanup
							}*/
						}
						#endregion

						if (isRectangle) {
							shapes.Squares.Add(CvInvoke.MinAreaRect(approxContour));
						}
					}
				}
			}

			return shapes;
		}

		public static RotatedRect? DetectPaper(UMat Edges) {
			return DetectPaper(FindContours(Edges));
		}

		public static RotatedRect? DetectPaper(VectorOfVectorOfPoint contours) {
			//Detect latgest contour
			double largestSize = 0;
			VectorOfPoint largestContour = null;
			foreach(VectorOfPoint contour in GetContourEnumerable(contours)) {
				double area = CvInvoke.ContourArea(contour, false);
				if(area > largestSize) {
					largestSize = area;
					largestContour = contour;
				}
			}

			if (largestContour == null) return null;
			else return CvInvoke.MinAreaRect(largestContour);
		}

		//Class allows to go through contours using foreach
		private class ContourList : IEnumerable {
			public VectorOfVectorOfPoint Contours { get; private set; }
			public ContourList(VectorOfVectorOfPoint contours) {
				Contours = contours;
			}

			// Implementation for the GetEnumerator method.
			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator)GetEnumerator();
			}

			public ContourEnumerator GetEnumerator() {
				return new ContourEnumerator(Contours);
			}

			public class ContourEnumerator : IEnumerator {
				private VectorOfVectorOfPoint contours;

				// Enumerators are positioned before the first element
				// until the first MoveNext() call.
				private int position = -1;

				public ContourEnumerator(VectorOfVectorOfPoint contours) {
					this.contours = contours;
				}

				public bool MoveNext() {
					position++;
					return (position < contours.Size);
				}

				public void Reset() {
					position = -1;
				}

				object IEnumerator.Current {
					get {
						return Current;
					}
				}

				public VectorOfPoint Current {
					get {
						try {
							return contours[position];
						} catch (IndexOutOfRangeException) {
							throw new InvalidOperationException();
						}
					}
				}
			}
		}
	}

	public class DetectedShapes {

		public List<Triangle2DF> Triangles { get; } = new List<Triangle2DF>();
		public List<RotatedRect> Squares { get; } = new List<RotatedRect>();

		public DetectedShapes() {

		}

	}
}
