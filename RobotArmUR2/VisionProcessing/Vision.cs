using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Drawing;
using System;
using RobotArmUR2.Util;
using System.Timers;

namespace RobotArmUR2.VisionProcessing {

	public class Vision {

		private static readonly object inputLock = new object(); //Protects changing the input stream while trying to input a new image.
		private static readonly object visionLock = new object(); //Protects accessing images while new ones are being processed.

		public ImageStream InputStream { get; } = new ImageStream();
		private System.Timers.Timer inputTimer = new System.Timers.Timer();
		private Mat rawInputBuffer;

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

		//private FPSCounter fpsCounter = new FPSCounter();

		public Vision() {
			InputStream.OnNewImage += InputStream_OnNewImage;
			inputTimer.Elapsed += Timer_OnTimeElapsed;
			inputTimer.AutoReset = true;

			Console.WriteLine("Have OpenCL: " + CvInvoke.HaveOpenCL);
			Console.WriteLine("Compatible GPU: " + CvInvoke.HaveOpenCLCompatibleGpuDevice);
			Console.WriteLine("Are we using OpenCL: " + CvInvoke.UseOpenCL);
		}

		private void InputStream_OnNewImage(ImageStream stream, Mat image) {
			lock (inputLock) {
				inputTimer.Interval = Math.Max(16, stream.TargetFPS * 3f); //TODO dont let mbe 0
				inputTimer.Start();

				rawInputBuffer = image;
				OnNewInputImage(rawInputBuffer.ToImage<Bgr, byte>());
			}
		}

		private void Timer_OnTimeElapsed(object sender, ElapsedEventArgs e) {
			if (Monitor.TryEnter(inputLock)) {
				try {
					if (!InputStream.IsOpened) InputStream_OnStreamEnded(InputStream);
					else OnNewInputImage(rawInputBuffer.ToImage<Bgr, byte>());
				} finally {
					// Ensure that the lock is released.
					Monitor.Exit(inputLock);
				}
			}
		}

		private void InputStream_OnStreamEnded(ImageStream sender) {
			lock (inputLock) {
				inputTimer.Stop();
				rawImage = null;
				OnNewInputImage(null);
				//TODO special event for empty frames
			}
		}

		private void OnNewInputImage(Image<Bgr, byte> image) {
			if (image != null) {
				lock (visionLock) {
					//FPS tick
					SetFPSCounter(InputStream.FPS); //Display target FPS?
					SetNativeResolutionText(image.Size);

					rawImage = image;

					//Scale image so Height = 480, but still keeps aspect ratio.
					inputImage = rawImage.Resize(480d / rawImage.Height, Emgu.CV.CvEnum.Inter.Cubic); //TODO put size in ApplicationSetttings

					if (RotateImage180) {
						inputImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
						inputImage._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
					}

					processVision();
				}
			} else {
				//Reset FPS
				SetNativeResolutionText?.Invoke(new Size(0, 0));
				lock (visionLock) {
					RawImage = null;
					InputImage = null;
					GrayscaleImage = null;
					ThresholdImage = null;
					WarpedImage = null;
					CannyImage = null;
				}
			}

			NewFrameFinished?.Invoke(this); //TODO lol
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

	}

}
