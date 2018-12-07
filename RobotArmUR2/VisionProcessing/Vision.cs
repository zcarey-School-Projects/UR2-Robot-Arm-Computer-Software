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
		//TODO make locks NON STATIC for ability to use multiple classes.
		public ImageStream InputStream { get; } = new ImageStream();

		public VisionImages Images { get; private set; }
		public byte GrayscaleThreshold { get; set; } = (byte)(255 / 2);

		#region Events and Handlers
		public delegate void NewFrameFinishedHandler(Vision sender, VisionImages outputs);
		public event NewFrameFinishedHandler OnNewFrameProcessed;
		#endregion

		public Vision() {
			InputStream.OnNewImage += InputStream_OnNewImage;
			InputStream.OnStreamEnded += InputStream_OnStreamEnded;
		}

		private void InputStream_OnNewImage(ImageStream stream, Mat image) {
			lock (inputLock) {
				if (image == null) {
					Console.WriteLine("Ended!");
					//OnNewInputImage(null); //TODO fix
					return;
				}

				OnNewInputImage(image.ToImage<Bgr, byte>());
			}
		}

		private void InputStream_OnStreamEnded(ImageStream sender) {
			Images = null;
			OnNewFrameProcessed?.Invoke(this, null);
		}

		private void OnNewInputImage(Image<Bgr, byte> image) {
			VisionImages output = null;

			if (image != null) {
				Image<Bgr, byte> inputImage = image.Resize(ApplicationSettings.WorkingImageScaledHeight / image.Height, Emgu.CV.CvEnum.Inter.Cubic); //Scale image so Height = 480, but still keeps aspect ratio.

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
