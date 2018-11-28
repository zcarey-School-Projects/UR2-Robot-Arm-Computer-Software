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
			/*set {
				lock (calibrationLock) {
					if (value != null) {
						paperCalibration = value;
					}
				}
			}*/ //TODO check is lock is needed. If not, remove need for private variable
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
					//lock (visionLock) { //Prevents image grabbing while images are being processed.
						processVision();
					//}
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

						//Scale image so Height = 480, but still keeps aspect ratio.
						inputImage = input.Resize(480d / input.Height, Emgu.CV.CvEnum.Inter.Cubic);

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
			lock (visionLock) {
				GrayscaleImage = ImageProcessing.GetGrayImage(InputImage);
				ThresholdImage = ImageProcessing.GetThresholdImage(grayscaleImage, new Gray(GrayscaleThreshold), new Gray(255));
				lock (calibrationLock) {//TODO what? prevent calibration changes
					WarpedImage = ImageProcessing.GetWarpedImage(thresholdImage, paperCalibration);
				}
				cannyEdges = ImageProcessing.EdgeDetection(warpedImage);
				CannyImage = DrawEdges(warpedImage, new Gray(255));
				DetectedShapes shapes = ImageProcessing.DetectShapes(cannyEdges);
				Triangles = shapes.Triangles;
				Squares = shapes.Squares;
			}
		}

		private Image<TColor, TDepth> DrawEdges<TColor, TDepth>(Image<TColor, TDepth> copyImage, TColor drawColor, int lineThickness = 2) where TColor:struct, IColor where TDepth:new() {
			Image<TColor, TDepth> cannyImage = copyImage.CopyBlank();
			LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges,
				1, //Distance resolution in pixel-related units
				Math.PI / 45.0, //Angle resolution measured in radians
				20, //threshold
				30, //min line width
				10); //gap between lines
			foreach (LineSegment2D line in lines) {
				cannyImage.Draw(line, drawColor, lineThickness);
			}
			return cannyImage;
		}

		public RotatedRect? AutoDetectPaper() {//TODO threadsafe??????
			Image<Gray, byte> workingImage = ImageProcessing.GetGrayImage(InputImage);
			workingImage = ImageProcessing.GetThresholdImage(workingImage, new Gray(GrayscaleThreshold), new Gray(255));
			UMat edges = ImageProcessing.EdgeDetection(workingImage);
			//DetectShapes();

			return ImageProcessing.DetectPaper(edges);
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
