using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Drawing;
using Emgu.CV.Util;
using System.Diagnostics;
using RobotHelpers.InputHandling;
using RobotArmUR2.VisionProcessing;
using RobotHelpers;

namespace RobotArmUR2.VisionProcessing{

	public class Vision : IDisposable {

		private static readonly object exitLock = new object();
		private static readonly object inputLock = new object();
		private static readonly object visionLock = new object();
		private static readonly object calibrationLock = new object();

		private VisionUIInvoker uiListener = new VisionUIInvoker();
		public IVisionUI UIListener { get => uiListener.Listener; set => uiListener.Listener = value; } //TODO thread safe

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

		private Image<Gray, byte> cannyImage = new Image<Gray, byte>(1, 1); //Canny edges detected from ThresholdImage
		public Image<Gray, byte> CannyImage {
			get {
				lock (visionLock) { return cannyImage.Copy(); }
			}
			private set {
				lock (visionLock) {
					if (value == null) cannyImage = new Image<Gray, byte>(1, 1);
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
					return new PaperCalibration(paperCalibration);
				}
			}
			set {
				lock (calibrationLock) {
					if (value != null) {
						paperCalibration = value;
						paperCalibration.SortPointOrder();
					}
				}
			}
		}


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
						return true;
					}

					return false;
				}
			}
		}

		//Does all the necessary processing on images to properly detect shapes.
		private void processVision() {
			GrayscaleImage = InputImage.Convert<Gray, byte>(); //Convert to black/white image since it is all we care about.
			ThresholdImage = grayscaleImage.ThresholdBinary(new Gray(255d / 2), new Gray(255));
			warpImage();

			//imageProc.MaskInputImage(origImage);
			//imageProc.FindShapesInMaskedImage();
			//imageProc.DrawFoundShapes(origImage);
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

		/*public bool AutoDetectPaper() {
			return imageProc.AutoDetectPaper(origImage);
		}
		*/


		/*
		private void processVision() {
			if (origImage == null || origImage.Width < 1 || origImage.Height < 1) return;
			imageProc.MaskInputImage(origImage);
			imageProc.FindShapesInMaskedImage();
			imageProc.DrawFoundShapes(origImage);
		}*/
		/*
		private void displayImages() {
			uiListener.Invoke_DisplayImage(origImage, PictureId.Original);
			uiListener.Invoke_DisplayImage(imageProc.grayImage, PictureId.Gray);
			uiListener.Invoke_DisplayImage(imageProc.cannyImage, PictureId.Canny);
		}
		*/
		/*
		public void getShapeLists(out List<Triangle2DF> triangles, out List<RotatedRect> boxes) {
			lock (visionLock) {
				triangles = new List<Triangle2DF>(imageProc.triangleList);
				boxes = new List<RotatedRect>(imageProc.boxList);
			}
		}
		*/
		/*
		public void setPaperMaskPoints(PaperCalibration paper) {
			lock (visionLock) {
				paper.SortPointOrder();
				imageProc.paperMaskPoints = paper;
				imageProc.paperMaskDirty = true;
			}
		}
		*/
		/*
		public PaperCalibration getPaperMaskPoints() {
			return new PaperCalibration(imageProc.paperMaskPoints);
		}
		*/
		/*
		private void calibratePaper() {
			Image<Bgr, byte> rect = origImage.CopyBlank();
			Point[] points = new Point[4];
			//for(int i = 0; i < points.Length; i++) {
			//	points[i] = new Point((int)(imageProc.paperMaskPoints[i].X * origImage.Width), (int)(imageProc.paperMaskPoints[i].Y * origImage.Height));
			//}
			points[0] = new Point((int)(imageProc.paperMaskPoints.BL.X * origImage.Width), (int)(imageProc.paperMaskPoints.BL.Y * origImage.Height));
			points[1] = new Point((int)(imageProc.paperMaskPoints.TL.X * origImage.Width), (int)(imageProc.paperMaskPoints.TL.Y * origImage.Height));
			points[2] = new Point((int)(imageProc.paperMaskPoints.TR.X * origImage.Width), (int)(imageProc.paperMaskPoints.TR.Y * origImage.Height));
			points[3] = new Point((int)(imageProc.paperMaskPoints.BR.X * origImage.Width), (int)(imageProc.paperMaskPoints.BR.Y * origImage.Height));
			//rect.DrawPolyline(points, true, new Bgr(42, 240, 247), 0);
			rect.FillConvexPoly(points, new Bgr(42, 240, 247));
			CvInvoke.AddWeighted(origImage, 0.8, rect, 0.2, 0, origImage);

			foreach(Point point in points) {
				origImage.Draw(new CircleF(point, 10), new Bgr(42, 240, 247), 3);
			}
			
			paperCalibrater.displayImage(origImage);
		}
		*/
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
