using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using RobotArmUR2.Util.Calibration.Paper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace RobotArmUR2.VisionProcessing {

	/// <summary> Contains all steps required to detect shapes. </summary>
	public static class ImageProcessing {

		/// <summary> Converts the input image into a grayscale image. </summary>
		/// <param name="input">Image to convert.</param>
		/// <returns>Converted image.</returns>
		public static Image<Gray, TDepth> GetGrayImage<TColor, TDepth>(Image<TColor, TDepth> input) where TColor : struct, IColor where TDepth : new() {
			if (input == null) return null;
			return input.Convert<Gray, TDepth>();
		}

		/// <summary>Thresholds the input image to a black/white version.</summary>
		/// <param name="input">Image to threshold</param>
		/// <param name="threshold">The threshold value that should be used.</param>
		/// <param name="MaxValue">The max color value that should be used.</param>
		/// <returns>The thresholded image.</returns>
		public static Image<TColor, TDepth> GetThresholdImage<TColor, TDepth>(Image<TColor, TDepth> input, TColor threshold, TColor MaxValue) where TColor : struct, IColor where TDepth : new() {
			if (input == null) return null;
			return input.ThresholdBinary(threshold, MaxValue);
		}

		/// <summary>Warps the input image so the calibrated corners take up the entire image in about the ratio of an 8.5 x 11 paper.</summary>
		/// <param name="image">Image to warp</param>
		/// <param name="paperCalibration">The calibration that should be used.</param>
		/// <returns>Warped image.</returns>
		public static Image<TColor, TDepth> GetWarpedImage<TColor, TDepth>(Image<TColor, TDepth> image, PaperCalibration paperCalibration) where TColor : struct, IColor where TDepth : new() {
			if (image == null) return null;
			if (paperCalibration == null) paperCalibration = new PaperCalibration(); //Defaults to whole image.
			PointF[] paperPoints = paperCalibration.ToArray(image.Size.Width, image.Size.Height); //BottomLeft, TopLeft, TopRight, BottomRight
			Image<TColor, TDepth> warpedImage = new Image<TColor, TDepth>(550, 425); //Should be close to aspect ratio of a piece of 8.5 x 11 paper.
			PointF[] targetPoints = new PointF[] { new PointF(0, warpedImage.Height - 1), new PointF(0, 0), new PointF(warpedImage.Width - 1, 0), new PointF(warpedImage.Width - 1, warpedImage.Height - 1) };

			using (var matrix = CvInvoke.GetPerspectiveTransform(paperPoints, targetPoints)) {
				CvInvoke.WarpPerspective(image, warpedImage, matrix, warpedImage.Size, Emgu.CV.CvEnum.Inter.Cubic);
			}

			return warpedImage;
		}

		/// <summary>Detects the edges in an image using canny edge detection.</summary>
		/// <param name="input">Image to detect iamges in.</param>
		/// <returns>UMat with detected edges.</returns>
		public static UMat EdgeDetection<TColor, TDepth>(Image<TColor, TDepth> input) where TColor : struct, IColor where TDepth : new() {
			if (input == null) return null;
			UMat cannyEdges = new UMat();
			CvInvoke.Canny(input, cannyEdges, 180.0, 120.0);
			return cannyEdges;
		}

		/// <summary>Converts detected edges into a viewable image.</summary>
		/// <typeparam name="TColor">Desired output color.</typeparam>
		/// <typeparam name="TDepth">Desired output depth</typeparam>
		/// <param name="Edges">The detected edges to convert.</param>
		/// <returns>Image with detected edges.</returns>
		public static Image<TColor, TDepth> GetEdgeImage<TColor, TDepth>(UMat Edges) where TColor:struct, IColor where TDepth:new(){
			if (Edges == null) return null;
			return Edges.ToImage<TColor, TDepth>();
		}

		/// <summary>Takes the detected edges and find contours.</summary>
		/// <param name="Edges">Detected edges.</param>
		/// <returns>List of detected contours.</returns>
		public static VectorOfVectorOfPoint FindContours(UMat Edges){
			if (Edges == null) return null;
			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			CvInvoke.FindContours(Edges, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
			return contours;
		}

		/// <summary>Creates an enumerator capable of enumerating through a list of contours.</summary>
		/// <param name="contours">Contours to enumerate through.</param>
		/// <returns>The enumerator.</returns>
		public static IEnumerable GetContourEnumerable(VectorOfVectorOfPoint contours) {
			if (contours == null) return null;
			return new ContourList(contours);
		}

		/// <summary>Given detected edges, detect shapes on the iamge. If contours were already found, use DetectShapes(Contours, ImageSize) instead.</summary>
		/// <param name="Edges">Detected edges</param>
		/// <returns>Detected shapes.</returns>
		public static DetectedShapes DetectShapes(UMat Edges) {
			if (Edges == null) return null;
			return DetectShapes(FindContours(Edges), Edges.Size);
		}

		/// <summary>Given a list of contours and the size of the image they were detected on, detects shapes on the image.</summary>
		/// <param name="contours">List of detected contours.</param>
		/// <param name="imageSize">Size of the image the contours were found on.</param>
		/// <returns>The detected shapes.</returns>
		public static DetectedShapes DetectShapes(VectorOfVectorOfPoint contours, Size imageSize) {
			if (contours == null || imageSize == null) return null;
			List<Triangle2DF> triangles = new List<Triangle2DF>();
			List<RotatedRect> squares = new List<RotatedRect>();

			foreach (VectorOfPoint contour in GetContourEnumerable(contours)) {
				VectorOfPoint approxContour = new VectorOfPoint();
				CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
				if (CvInvoke.ContourArea(approxContour, false) > 250) { //only consider areas that are large enough.
					if (approxContour.Size == 3) { //Three vertices, must be a triangle!
						Point[] pts = approxContour.ToArray();
						triangles.Add(new Triangle2DF(pts[0], pts[1], pts[2]));
					} else if (approxContour.Size == 4) { //Four vertices, must be a square!
						squares.Add(CvInvoke.MinAreaRect(approxContour));
					}
				}
			}

			return new DetectedShapes(triangles, squares, imageSize);
		}

		/// <summary>Given edges, attempts to detect the paper. If contours were already found, use DetectPaper(Contours) instead. </summary>
		/// <param name="Edges">The detected edges.</param>
		/// <returns>Attempted detection of the paper.</returns>
		public static RotatedRect? DetectPaper(UMat Edges) {
			if (Edges == null) return null;
			return DetectPaper(FindContours(Edges));
		}

		/// <summary>Given a list of contours, attempts to detect the paper.</summary>
		/// <param name="contours">The list of contours detected.</param>
		/// <returns>Attempted detection of the paper.</returns>
		public static RotatedRect? DetectPaper(VectorOfVectorOfPoint contours) {
			if (contours == null) return null;
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

		/// <summary>Draws detected shapes onto an image.</summary>
		/// <param name="image">Image to draw on.</param>
		/// <param name="shapes">Detected shapes to draw.</param>
		/// <param name="TriangleColor">Color of triangles.</param>
		/// <param name="SquareColor">Color of squares</param>
		/// <param name="thickness">Thickness of drawn shapes.</param>
		public static void DrawShapes<TColor, TDepth>(Image<TColor, TDepth> image, DetectedShapes shapes, TColor TriangleColor, TColor SquareColor, int thickness = 2) where TColor : struct, IColor where TDepth : new() {
			if(image == null || shapes == null) return;
			foreach (Triangle2DF triangle in shapes.Triangles) {
				image.Draw(triangle, TriangleColor, thickness);
				image.Draw(new CircleF(triangle.Centeroid, 2), TriangleColor, thickness);
			}

			foreach (RotatedRect square in shapes.Squares) {
				image.Draw(square, SquareColor, thickness);
				image.Draw(new CircleF(square.Center, 2), SquareColor, thickness);
			}
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

}
