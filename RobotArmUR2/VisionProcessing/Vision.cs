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

		public ImageStream InputStream { get; } = new ImageStream();
		private System.Timers.Timer inputTimer = new System.Timers.Timer();
		private Mat rawInputBuffer;

		private VisionImages images = new VisionImages();
		public VisionImages Images { get => images; private set { if (value == null) images = new VisionImages(); else images = value; } }

		private DetectedShapes detecetdShapes = new DetectedShapes();
		public DetectedShapes DetectedShapes { get => detecetdShapes; private set { if (value == null) detecetdShapes = new DetectedShapes(); else detecetdShapes = value; } }

		public bool RotateImage180 { get; set; } = false;
		public byte GrayscaleThreshold { get; set; } = (byte)(255 / 2);

		#region Events and Handlers
		public delegate void SetNativeResolutionTextHandler(Size size); 
		public event SetNativeResolutionTextHandler SetNativeResolutionText;

		public delegate void SetFPSCounterHandler(float CurrentFPS, float TargetFPS);
		public event SetFPSCounterHandler SetFPSCounter;

		public delegate void NewFrameFinishedHandler(Vision vision);
		public event NewFrameFinishedHandler NewFrameFinished;
		#endregion

		//private FPSCounter fpsCounter = new FPSCounter();
		//TODO class that stores computed images!
		public Vision() {
			InputStream.OnNewImage += InputStream_OnNewImage;
			inputTimer.Elapsed += Timer_OnTimeElapsed;
			inputTimer.AutoReset = true;
		}

		private void InputStream_OnNewImage(ImageStream stream, Mat image) {
			lock (inputLock) { 
				inputTimer.Interval = Math.Max(1000/120, stream.TargetFPS * 3f);
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
				OnNewInputImage(null);
			}
		}

		private void OnNewInputImage(Image<Bgr, byte> image) {
			Image<Bgr, byte> rawImage = null;
			Image<Bgr, byte> inputImage = null;
			Image<Gray, byte> grayImage = null;
			Image<Gray, byte> thresholdImage = null;
			Image<Gray, byte> warpedImage = null;
			Image<Gray, byte> cannyImage = null;
			if (image != null) {
				SetFPSCounter?.Invoke(InputStream.FPS, InputStream.TargetFPS); //Display target FPS?
				SetNativeResolutionText?.Invoke(image.Size);

				rawImage = image;

				//Scale image so Height = 480, but still keeps aspect ratio.
				inputImage = rawImage.Resize(480d / rawImage.Height, Emgu.CV.CvEnum.Inter.Cubic); //TODO put size in ApplicationSetttings

				if (RotateImage180) {
					inputImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
					inputImage._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
				}

				grayImage = ImageProcessing.GetGrayImage(inputImage);
				thresholdImage = ImageProcessing.GetThresholdImage(grayImage, new Gray(GrayscaleThreshold), new Gray(255));
				warpedImage = ImageProcessing.GetWarpedImage(thresholdImage, ApplicationSettings.PaperCalibration);
				UMat edges = ImageProcessing.EdgeDetection(warpedImage);
				cannyImage = ImageProcessing.GetEdgeImage<Gray, byte>(edges);
				DetectedShapes = ImageProcessing.DetectShapes(edges);
			} else {
				SetNativeResolutionText?.Invoke(new Size(0, 0));
				SetFPSCounter?.Invoke(0, InputStream.TargetFPS);
			}

			Images = new VisionImages(rawImage, inputImage, grayImage, thresholdImage, warpedImage, cannyImage);
			NewFrameFinished?.Invoke(this); //TODO lol
		}

		public RotatedRect? AutoDetectPaper() {
			Image<Gray, byte> workingImage = ImageProcessing.GetGrayImage(Images.Input);
			workingImage = ImageProcessing.GetThresholdImage(workingImage, new Gray(GrayscaleThreshold), new Gray(255));
			UMat edges = ImageProcessing.EdgeDetection(workingImage);

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
