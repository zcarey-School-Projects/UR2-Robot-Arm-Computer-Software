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

		public VisionImages Images { get; private set; }

		public bool RotateImage180 { get; set; } = false;
		public byte GrayscaleThreshold { get; set; } = (byte)(255 / 2);

		#region Events and Handlers
		public delegate void NewFrameFinishedHandler(Vision sender, VisionImages outputs);
		public event NewFrameFinishedHandler OnNewFrameProcessed;
		#endregion

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
			VisionImages output = null;

			if (image != null) {
				Image<Bgr, byte> inputImage = image.Resize(ApplicationSettings.WorkingImageScaledHeight / image.Height, Emgu.CV.CvEnum.Inter.Cubic); //Scale image so Height = 480, but still keeps aspect ratio.
				if (RotateImage180) {
					inputImage._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
					inputImage._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
				}

				Image<Gray, byte> grayImage = ImageProcessing.GetGrayImage(inputImage);
				Image<Gray, byte> threshImage = ImageProcessing.GetThresholdImage(grayImage, new Gray(GrayscaleThreshold), new Gray(255));
				Image<Gray, byte> warpedImage = ImageProcessing.GetWarpedImage(threshImage, ApplicationSettings.PaperCalibration);
				UMat edges = ImageProcessing.EdgeDetection(warpedImage);
				Image<Gray, byte> cannyImage = ImageProcessing.GetEdgeImage<Gray, byte>(edges);
				DetectedShapes shapes = ImageProcessing.DetectShapes(edges);
				Image<Bgr, byte> warpedShapes = warpedImage.Convert<Bgr, byte>();
				ImageProcessing.DrawShapes(warpedShapes, shapes, ApplicationSettings.TriangleHighlightColor, ApplicationSettings.SquareHighlightColor, ApplicationSettings.ShapeHighlightThickness);

				output = new VisionImages(image, inputImage, grayImage, threshImage, warpedImage, cannyImage, warpedShapes, shapes);
			}

			Images = output;
			OnNewFrameProcessed?.Invoke(this, output);
		}

		public RotatedRect? AutoDetectPaper(VisionImages images) {
			if (images == null || images.Input == null) return null;
			Image<Gray, byte> workingImage = ImageProcessing.GetGrayImage(images.Input);
			workingImage = ImageProcessing.GetThresholdImage(workingImage, new Gray(GrayscaleThreshold), new Gray(255));
			UMat edges = ImageProcessing.EdgeDetection(workingImage);

			return ImageProcessing.DetectPaper(edges);
		}

	}

}
