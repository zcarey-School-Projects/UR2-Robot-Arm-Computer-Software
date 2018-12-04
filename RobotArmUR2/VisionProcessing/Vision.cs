using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Drawing;
using RobotHelpers.InputHandling;
using RobotHelpers.Util;

namespace RobotArmUR2.VisionProcessing {

	public class Vision {

		private static readonly object exitLock = new object();
		private static readonly object inputLock = new object(); //Protects changing the input stream while trying to input a new image.
		private static readonly object visionLock = new object(); //Protects accessing images while new ones are being processed.

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

		#region Image Properties
		private Image<Bgr, byte> rawImage = new Image<Bgr, byte>(1, 1); //Full pixel size, non-edited
		public Image<Bgr, byte> RawImage {
			get {
				lock (visionLock) { return rawImage; }
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
				lock (visionLock) { return inputImage; }
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
				lock (visionLock) { return grayscaleImage; }
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
				lock (visionLock) { return thresholdImage; }
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
				lock (visionLock) { return warpedImage; }
			}
			private set {
				lock (visionLock) {
					if (value == null) warpedImage = new Image<Gray, byte>(1, 1);
					else warpedImage = value;
				}
			}
		}

		private Image<Gray, byte> cannyImage = new Image<Gray, byte>(1, 1); //Canny edges in image form detected from ThresholdImage
		public Image<Gray, byte> CannyImage {
			get {
				lock (visionLock) { return cannyImage; }
			}
			private set {
				lock (visionLock) {
					if (value == null)cannyImage = new Image<Gray, byte>(1, 1);
					else cannyImage = value;
				}
			}
		}

		#endregion

		private DetectedShapes detecetdShapes = new DetectedShapes();
		public DetectedShapes DetectedShapes { get => detecetdShapes; private set { if (value == null) detecetdShapes = new DetectedShapes(); else detecetdShapes = value; } }

		public bool RotateImage180 { get; set; } = false;
		public byte GrayscaleThreshold { get; set; } = (byte)(255 / 2);

		#region Events and Handlers
		public delegate void SetNativeResolutionTextHandler(Size size); 
		public event SetNativeResolutionTextHandler SetNativeResolutionText;

		public delegate void SetFPSCounterHandler(float FPS);
		public event SetFPSCounterHandler SetFPSCounter;

		public delegate void NewFrameFinishedHandler(Vision vision);
		public event NewFrameFinishedHandler NewFrameFinished;
		#endregion


		private volatile bool exitThread = false;
		private Thread captureThread;

		private FPSCounter fpsCounter = new FPSCounter();

		public Vision() {
			captureThread = new Thread(visionThreadLoop);
			captureThread.Name = "Vision Processing Thread";
			captureThread.IsBackground = true;
		}

		private void visionThreadLoop() {
			while (true) {
				lock (exitLock) {
					if (exitThread) {
						break;
					}
				}

				lock (visionLock) { //While we are changing images around, we dont want the images to be accessed.
					if (!inputImages()) {
						fpsCounter.Reset();
						SetNativeResolutionText(new Size(0, 0));
						lock (visionLock) {
							RawImage = null;
							InputImage = null;
							GrayscaleImage = null;
							ThresholdImage = null;
							WarpedImage = null;
							CannyImage = null;
						}
						Thread.Sleep(1); //Keeps the thread from going mach 3 when there is no input, but still fast enough that it isn't "stalling" the pogram.
					} else {
						fpsCounter.Tick();
						SetNativeResolutionText(RawImage.Size);
						processVision();
					}
				}

				SetFPSCounter(fpsCounter.FPS);
				NewFrameFinished(this);
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
				ThresholdImage = ImageProcessing.GetThresholdImage(GrayscaleImage, new Gray(GrayscaleThreshold), new Gray(255));
				WarpedImage = ImageProcessing.GetWarpedImage(ThresholdImage, ApplicationSettings.PaperCalibration);
				UMat edges = ImageProcessing.EdgeDetection(WarpedImage);
				CannyImage = ImageProcessing.GetEdgeImage<Gray, byte>(edges);
				DetectedShapes = ImageProcessing.DetectShapes(edges);
			}
		}

		public RotatedRect? AutoDetectPaper() {
			Image<Gray, byte> workingImage = ImageProcessing.GetGrayImage(InputImage);
			workingImage = ImageProcessing.GetThresholdImage(workingImage, new Gray(GrayscaleThreshold), new Gray(255));
			UMat edges = ImageProcessing.EdgeDetection(workingImage);
			//DetectShapes();

			return ImageProcessing.DetectPaper(edges);
		}

		public void DrawShapes<TColor, TDepth>(Image<TColor, TDepth> image, TColor TriangleColor, TColor SquareColor, int thickness = 2) where TColor : struct, IColor where TDepth : new() {
			DetectedShapes shapes = DetectedShapes;
			foreach (Triangle2DF triangle in shapes.Triangles) {
				image.Draw(triangle, TriangleColor, thickness);
				image.Draw(new CircleF(triangle.Centeroid, 2), TriangleColor, thickness);
			}

			foreach (RotatedRect square in shapes.Squares) {
				image.Draw(square, SquareColor, thickness);
				image.Draw(new CircleF(square.Center, 2), SquareColor, thickness);
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

	}

}
