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

namespace RobotArmUR2.VisionProcessing{

	public class Vision{

		private static readonly object visionLock = new object();
		private bool keepThreadRunning = true;

		private Form1 UI;
		private PaperCalibrater paperCalibrater;
		private Thread captureThread;
		private InputHandler input;

		private Image<Bgr, byte> origImage;


		private ImageProcessing imageProc = new ImageProcessing();

		private DateTime lastTime = DateTime.UtcNow;
		private const int FPS_NumFramesToAvg = 10;
		private float[] FPS_times = new float[FPS_NumFramesToAvg];
		private int FPS_index = 0;
		private float totalFPS = 0;
		private Stopwatch timer = new Stopwatch();
		private bool nullFPS = true;

		private VisionMode mode = VisionMode.Default;

		public Vision(Form1 UI, PaperCalibrater paperCalibrater) {
			this.UI = UI;
			this.paperCalibrater = paperCalibrater;
			captureThread = new Thread(VisionThread);
			captureThread.Name = "Vision Processing Thread";
			captureThread.IsBackground = true;
		}

		public Vision(Form1 UI, PaperCalibrater paperCalibrater, int cameraIndex) : this(UI, paperCalibrater){
			input = new CameraInput(0);
		}

		~Vision() {
			Dispose();
		}

		public void Dispose() {
			if(input != null) input.Dispose();
			if(origImage != null) origImage.Dispose();
			if (imageProc != null) imageProc.Dispose();
		}

		private void VisionThread() {
			while (true) {
				calculateFPS(); //Calculate FPS and write to text

				//Check if the thread should continue running.
				lock (visionLock) {
					if (!keepThreadRunning) {
						break;
					}
				}

				//If a new input image is grabbed successfully, continue with processing it.
				if (inputImages()) {
					switch (mode) {
						case VisionMode.Default: processVision(); displayImages(); break;
						case VisionMode.CalibratePaper: calibratePaper(); break;
						default: processVision(); break;
					}
				} else {
					Thread.Sleep(1);
				}
				
			}

			Dispose();
		}

		public bool AutoDetectPaper() {
			return imageProc.AutoDetectPaper(origImage);
		}

		private bool inputImages() {
			if (input == null) {
				origImage = null;
			} else {
				origImage = input.getFrame(); //Attempt to read a new frame.
			}

			//If no image is read, just return a blank image and wait 1 ms to try again.
			if (origImage == null) {
				origImage = new Image<Bgr, byte>(1, 1);
				//grayImage = new Image<Gray, byte>(1, 1);
				displayImages(); //Update the images to be blank.
				nullFPS = true; //So we can reset the FPS counter.
				return false;
			}

			nullFPS = false; //So we know we don't have to reset the FPS counter
			UI.setNativeResolutionText(origImage.Width, origImage.Height); //TODO: Not update resolution text every frame.

			//Resize the image to allow for faster / more efficient computing (If needed).
			origImage = origImage.Resize(640, 480, Emgu.CV.CvEnum.Inter.Cubic);

			return true;
		}

		private void processVision() {

			#region Image Conditioning
			//Grab gray image for shape detection
			//grayImage = origImage.Convert<Gray, byte>();
			
			#endregion

		}

		private void displayImages() {
			UI.displayImage(origImage, Form1.PictureId.Original);
			UI.displayImage(imageProc.grayImage, Form1.PictureId.Gray);
			UI.displayImage(imageProc.cannyImage, Form1.PictureId.Canny);
		}

		public void getShapeLists(out List<Triangle2DF> triangles, out List<RotatedRect> boxes) {
			lock (visionLock) {
				triangles = new List<Triangle2DF>(imageProc.triangleList);
				boxes = new List<RotatedRect>(imageProc.boxList);
			}
		}

		public void setPaperMaskPoints(PointF p1, PointF p2, PointF p3, PointF p4) {
			lock (visionLock) {
				imageProc.paperMaskPoints = new PointF[] { p1, p2, p3, p4 };
				imageProc.paperMaskDirty = true;
			}
		}

		public PointF[] getPaperMaskPoints() {
			PointF[] copyArray = new PointF[imageProc.paperMaskPoints.Length];
			for(int i = 0; i < imageProc.paperMaskPoints.Length; i++) {
				copyArray[i] = new PointF(imageProc.paperMaskPoints[i].X, imageProc.paperMaskPoints[i].Y);
			}

			return copyArray;
		}

		private void calibratePaper() {
			Image<Bgr, byte> rect = origImage.CopyBlank();
			Point[] points = new Point[imageProc.paperMaskPoints.Length];
			for(int i = 0; i < points.Length; i++) {
				points[i] = new Point((int)(imageProc.paperMaskPoints[i].X * origImage.Width), (int)(imageProc.paperMaskPoints[i].Y * origImage.Height));
			}
			//rect.DrawPolyline(points, true, new Bgr(42, 240, 247), 0);
			rect.FillConvexPoly(points, new Bgr(42, 240, 247));
			CvInvoke.AddWeighted(origImage, 0.8, rect, 0.2, 0, origImage);

			foreach(PointF point in imageProc.paperMaskPoints) {
				origImage.Draw(new CircleF(new PointF(origImage.Width * point.X, origImage.Height * point.Y), 10), new Bgr(42, 240, 247), 3);
			}
			
			paperCalibrater.displayImage(origImage);
		}

		private void calculateFPS() {
			float FPS = 0;
			if (!nullFPS) {
				DateTime currentTime = DateTime.UtcNow;
				TimeSpan span = currentTime.Subtract(lastTime);
				float currentFPS = 1000f / (float)span.TotalMilliseconds;
				lastTime = currentTime;
				totalFPS -= FPS_times[FPS_index];
				totalFPS += currentFPS;
				FPS_times[FPS_index] = currentFPS;
				FPS_index++;
				if (FPS_index >= FPS_NumFramesToAvg) {
					FPS_index = 0;
				}
				FPS = totalFPS / FPS_NumFramesToAvg;
			} else {
				FPS_times = new float[FPS_NumFramesToAvg];
				FPS_index = 0;
				totalFPS = 0;
			}

			UI.setFPS(FPS);
		}

		public void start() { if(!captureThread.IsAlive) captureThread.Start(); }

		public void stop() {
			if (captureThread.IsAlive) {
				lock (visionLock) {
					//Safely let the thread know to exit.
					keepThreadRunning = false;
				}
				captureThread.Join(); //Wait for the thread to finish before closing.
			}
		}

		public void play() {
			lock (visionLock) {
				input.play();
			}
		}

		public void pause() {
			lock (visionLock) {
				input.pause();
			}
		}

		public void closeInputStreams() {
			lock (visionLock) {
				input.Dispose();
				input = null;
			}
		}

		public bool setCamera(int cameraId) {
			lock (visionLock) {
				input.Dispose();
				input = new CameraInput(cameraId);
				//input.play();
				return input.isFrameAvailable();
			}
		}

		public bool setInternalVideo(String filename) {
			return setVideo(input.getWorkingDirectory() + filename);
		}

		public bool setVideo(String filename) {
			lock (visionLock) {
				if (input is VideoInput) {
					return ((VideoInput)input).LoadFromFile(filename);
				} else {
					input.Dispose();
					input = new VideoInput(filename);
					return input.isFrameAvailable();
				}
			}
		}

		public bool setInternalImage(String filename) {
			return setImage(input.getWorkingDirectory() + filename);
		}

		public bool setImage(String filename) {
			lock (visionLock) {
				if (input is ImageInput) {
					return ((ImageInput)input).LoadFromFile(filename);
				} else {
					bool wasPlaying = input.isPlaying();
					input.Dispose();
					input = new ImageInput(filename);
					if (!wasPlaying) input.pause();
					return input.isFrameAvailable();
				}
			}
		}

		public bool loadImage() {
			lock (visionLock) {
				if (!(input is ImageInput)) {
					bool wasPlaying = input.isPlaying();
					input.Dispose();
					input = new ImageInput(null);
					if (!wasPlaying) input.pause();
				}

				return ((FileInput)input).PromptUserToLoadFile();
			}
		}

		public bool loadVideo() {
			lock (visionLock) {
				if (!(input is VideoInput)) {
					bool wasPlaying = input.isPlaying();
					input.Dispose();
					input = new VideoInput(null);
					if (!wasPlaying) input.pause();
				}

				return ((FileInput)input).PromptUserToLoadFile();
			}
		}

		public bool loadInput() {
			lock (visionLock) {
				if (input is FileInput) {
					return ((FileInput)input).PromptUserToLoadFile();
				}
				return false;
			}
		}

		public byte[,,] getRawData() {
			lock (visionLock) {
				return input.readRawData();
			}
		}

		public int getStreamWidth() {
			return input.getWidth();
		}

		public int getStreamHeight() {
			return input.getHeight();
		}

		public void setMode(VisionMode newMode) {
			lock (visionLock) {
				mode = newMode;
			}
		}

		public enum VisionMode {
			Default,
			CalibratePaper,
		}
	}
}
