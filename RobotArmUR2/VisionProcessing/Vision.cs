using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Drawing;
using RobotHelpers.InputHandling;
using RobotHelpers;
using Emgu.CV.Util;

namespace RobotArmUR2.VisionProcessing{

	public class Vision : IDisposable {

		private static readonly object exitLock = new object();
		private static readonly object inputLock = new object();
		private static readonly object visionLock = new object();
		private static readonly object calibrationLock = new object();

		private VisionUIInvoker uiListener = new VisionUIInvoker();
		public IVisionUI UIListener { get => uiListener.Listener; set => uiListener.Listener = value; }

		private InputHandler inputStream;
		public InputHandler InputStream {
			get {
				lock (inputLock) {
					return inputStream;
				}
			}
			set {
				lock (inputLock) {
					if (inputStream != null) inputStream.Dispose();
					inputStream = value;
				}
			} }

		private Image<Bgr, byte> rawImage = new Image<Bgr, byte>(1, 1); //Full pixel size, non-edited
		public Image<Bgr, byte> RawImage {
			get {
				lock (visionLock) { return rawImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) rawImage = new Image<Bgr, byte>(1, 1);
					else rawImage = value;
				}
			}
		} 

		private Image<Bgr, byte> inputImage = new Image<Bgr, byte>(1, 1); //Resized raw image for better memory usage
		public Image<Bgr, byte> InputImage {
			get {
				lock (visionLock) { return inputImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) inputImage = new Image<Bgr, byte>(1, 1);
					else inputImage = value;
				}
			}
		} 

		private Image<Gray, byte> grayscaleImage = new Image<Gray, byte>(1, 1); //Grayscaled InputImage
		public Image<Gray, byte> GrayscaleImage {
			get {
				lock (visionLock) { return grayscaleImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) grayscaleImage = new Image<Gray, byte>(1, 1);
					else grayscaleImage = value;
				}
			}
		}

		private Image<Gray, byte> thresholdImage = new Image<Gray, byte>(1, 1);//Thresholded Greyscale Image to either a black/white, no gray
		public Image<Gray, byte> ThresholdImage {
			get {
				lock (visionLock) { return thresholdImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) thresholdImage = new Image<Gray, byte>(1, 1);
					else thresholdImage = value;
				}
			}
		}

		private Image<Gray, byte> warpedImage = new Image<Gray, byte>(1, 1); //Warped image so the corners of the paper are in the corner of the image.
		public Image<Gray, byte> WarpedImage {
			get {
				lock (visionLock) { return warpedImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) warpedImage = new Image<Gray, byte>(1, 1);
					else warpedImage = value;
				}
			}
		}

		private UMat cannyEdges = new UMat();
		private Image<Gray, byte> cannyImage = new Image<Gray, byte>(1, 1); //Canny edges detected from ThresholdImage
		public Image<Gray, byte> CannyImage {
			get {
				lock (visionLock) { return cannyImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null)cannyImage = new Image<Gray, byte>(1, 1);
					else cannyImage = value;
				}
			}
		}

		private List<Triangle2DF> triangleList = new List<Triangle2DF>(); //List of detected triangles
		public List<Triangle2DF> Triangles { get => new List<Triangle2DF>(triangleList); private set => triangleList = value; }

		private List<RotatedRect> squareList = new List<RotatedRect>(); //List of detected squares
		public List<RotatedRect> Squares { get => new List<RotatedRect>(squareList); private set => squareList = value; }

		private PaperCalibration paperCalibration = new PaperCalibration();
		public PaperCalibration PaperCalibration {
			get {
				lock (calibrationLock) {
					//return new PaperCalibration(paperCalibration);
					return paperCalibration;
				}
			}
			set {
				lock (calibrationLock) {
					if (value != null) {
						paperCalibration = value;
					}
				}
			}
		}

		public bool RotateImage180 { get; set; } = false;
		public byte GrayscaleThreshold { get; set; } = (byte)(255 / 2);


		private volatile bool exitThread = false;
		private Thread captureThread;

		private FPSCounter fpsCounter = new FPSCounter();

		public Vision() {
			captureThread = new Thread(visionThreadLoop);
			captureThread.Name = "Vision Processing Thread";
			captureThread.IsBackground = true;
		}

		public Vision(int cameraIndex) : this(){
			inputStream = new CameraInput(cameraIndex);
			inputStream.Play();
		}

		~Vision() {
			Dispose();
		}

		public void Dispose() {
			//TODO dispose
			//if(inputStream != null) inputStream.Dispose();
			//if(origImage != null) origImage.Dispose();
			//if (imageProc != null) imageProc.Dispose();
		}

		private void visionThreadLoop() {
			while (true) {
				lock (exitLock) {
					if (exitThread) {
						break;
					}
				}	

				if (!inputImages()) {
					fpsCounter.Reset();
					uiListener.SetNativeResolutionText(new Size(0, 0));
					lock (visionLock) {
						InputImage = null;
						GrayscaleImage = null;
						ThresholdImage = null;
						WarpedImage = null;
						CannyImage = null;
					}
					Thread.Sleep(1); //Keeps the thread from going mach 3 when there is no input, but still fast enough that it isn't "stalling" the pogram.
				} else {
					fpsCounter.Tick();
					uiListener.SetNativeResolutionText(rawImage.Size);
					lock (visionLock) { //Prevents image grabbing while images are being processed.
						processVision();
					}
				}

				uiListener.SetFPSCounter(fpsCounter.FPS);
				uiListener.NewFrameFinished(this);
			}

			exitThread = false;
		}

		private bool inputImages() {
			lock (inputLock) {
				lock (visionLock) {
					if (inputStream == null) {
						RawImage = null;
					} else {
						Image<Bgr, byte> input = inputStream.GetFrame(); //Attempt to read a new frame.
						RawImage = input;
						if (input == null) return false;

						//TODO change so height os 480
						//Scale image so Height = 640, but still keeps aspect ratio.
						inputImage = input.Resize(640d / input.Height, Emgu.CV.CvEnum.Inter.Cubic);

						if (RotateImage180) {
							inputImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
							inputImage._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
						}

						return true;
					}

					return false;
				}
			}
		}

		//Does all the necessary processing on images to properly detect shapes.
		private void processVision() {
			GrayscaleImage = InputImage.Convert<Gray, byte>(); //Convert to black/white image since it is all we care about.
			ThresholdImage = grayscaleImage.ThresholdBinary(new Gray(GrayscaleThreshold), new Gray(255));
			warpImage();
			cannyEdgeDetection(warpedImage);
			DetectShapes();
		}

		private void DetectShapes() {
			triangleList = new List<Triangle2DF>();
			squareList = new List<RotatedRect>();
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
							/*if (dAngle < RectangleMinimumAngle || dAngle > RectangleMaximumAngle) {
								isRectangle = false;
								break;
							}*/
						}
						#endregion

						if (isRectangle) {
							squareList.Add(CvInvoke.MinAreaRect(approxContour));
						}
					}
				}
			}
		}

		//Finds edges in an image.
		private void cannyEdgeDetection(Image<Gray, byte> inputImage) {
			//UMat cannyEdges = new UMat();
			//LineSegment2D[] lines;
			CvInvoke.Canny(inputImage, cannyEdges, 180.0, 120.0);
			cannyImage = inputImage.CopyBlank();
			LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges,
				1, //Distance resolution in pixel-related units
				Math.PI / 45.0, //Angle resolution measured in radians
				20, //threshold
				30, //min line width
				10); //gap between lines
			foreach (LineSegment2D line in lines) {
				cannyImage.Draw(line, new Gray(255), 2);
			}
		}

		//Warps the ThresholdImage so the calibrated corners take up the entire image.
		private void warpImage() {
			PointF[] paperPoints;
			lock (calibrationLock) {
				paperPoints = paperCalibration.ToArray(thresholdImage.Size);
			}
			warpedImage = new Image<Gray, byte>(550, 425); //Should be close to aspect ratio of the paper.
			if (paperPoints == null) return; //Rare, but possible
			Size size = warpedImage.Size;
			PointF[] targetPoints = new PointF[] { new PointF(0, size.Height - 1), new PointF(0, 0), new PointF(size.Width - 1, 0), new PointF(size.Width - 1, size.Height - 1) };

			using (var matrix = CvInvoke.GetPerspectiveTransform(paperPoints, targetPoints)) {
				warpedImage = new Image<Gray, byte>(550, 425);
				CvInvoke.WarpPerspective(thresholdImage, warpedImage, matrix, warpedImage.Size, Emgu.CV.CvEnum.Inter.Cubic);
			}
		}

		public RotatedRect? AutoDetectPaper() {
			GrayscaleImage = InputImage.Convert<Gray, byte>(); //Convert to black/white image since it is all we care about.
			ThresholdImage = grayscaleImage.ThresholdBinary(new Gray(255d / 2), new Gray(255));
			cannyEdgeDetection(warpedImage);
			DetectShapes();

			if (squareList.Count == 0) return null;

			//Find largest rectangle
			float largestSize = 0;
			RotatedRect? largest = null;
			foreach(RotatedRect square in squareList) {
				float area = square.Size.Width * square.Size.Height;
				if(area > largestSize) {
					largest = square;
					largestSize = area;
				}
			}

			return largest;
		}

		public void DrawTriangles(Image<Bgr, byte> image, List<Triangle2DF> shapes, Bgr color, int thickness) {
			foreach(Triangle2DF triangle in shapes) {
				image.Draw(triangle, color, thickness);
				image.Draw(new CircleF(triangle.Centeroid, 2), color, thickness);
			}
		}

		public void DrawSquares(Image<Bgr, byte> image, List<RotatedRect> shapes, Bgr color, int thickness) {
			foreach(RotatedRect square in shapes) {
				image.Draw(square, color, thickness);
				image.Draw(new CircleF(square.Center, 2), color, thickness);
			}
		}

		public void start() { if(!captureThread.IsAlive) captureThread.Start(); }

		public void stop() {
			if (captureThread.IsAlive) {
				lock (exitLock) {
					//Safely let the thread know to exit.
					exitThread = true;
				}
				captureThread.Join(); //Wait for the thread to finish before closing.
			}
		}

		public void CloseInputStream() {
			lock (inputLock) {
				inputStream.Dispose();
				inputStream = null;
			}
		}

	}
}
