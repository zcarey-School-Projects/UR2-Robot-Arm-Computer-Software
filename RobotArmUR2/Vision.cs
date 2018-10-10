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

namespace RobotArmUR2{

	public class Vision{

		private static readonly object visionLock = new object();
		private bool keepThreadRunning = true;

		private Form1 UI;
		private PaperCalibrater paperCalibrater;
		private Thread captureThread;
		private InputHandler input;

		private Image<Bgr, byte> origImage;
		private Image<Gray, byte> grayImage;
		private Image<Gray, byte> cannyImage;
		private Image<Gray, byte> paperMask;
		private PointF[] paperPoints = new PointF[] { new PointF(0, 0), new PointF(1, 0), new Point(1, 1), new Point(0, 1) };
		private bool paperMaskDirty = true;
		private List<Triangle2DF> triangleList = new List<Triangle2DF>();
		private List<RotatedRect> boxList = new List<RotatedRect>();

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
			if(grayImage != null) grayImage.Dispose();
			if(cannyImage != null) cannyImage.Dispose();
			if(paperMask != null) paperMask.Dispose();
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

		private bool inputImages() {
			if (input == null) {
				origImage = null;
			} else {
				origImage = input.getFrame(); //Attempt to read a new frame.
			}

			//If no image is read, just return a blank image and wait 1 ms to try again.
			if (origImage == null) {
				origImage = new Image<Bgr, byte>(1, 1);
				grayImage = new Image<Gray, byte>(1, 1);
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
			grayImage = origImage.Convert<Gray, byte>();
			if ((paperMask == null) || (paperMaskDirty) || (grayImage.Width != paperMask.Width) || (grayImage.Height != paperMask.Height)) {
				getPaperMask();
			}

			//Mask the gray image.
			for(int y = 0; y < grayImage.Height; y++) {
				for(int x = 0; x < grayImage.Width; x++) {
					if (paperMask.Data[y, x, 0] == 0) {
						grayImage.Data[y, x, 0] = 0;
					}
				}
			}
			#endregion

			#region Canny and edge detection
			UMat cannyEdges = new UMat();
			LineSegment2D[] lines;
			double dCannyThreLinking = 120.0;
			double dCannyThres = 180.0;
			CvInvoke.Canny(grayImage, cannyEdges, dCannyThres, dCannyThreLinking);
			lines = CvInvoke.HoughLinesP(cannyEdges,
				1, //Distance resolution in pixel-related units
				Math.PI / 45.0, //Angle resolution measured in radians
				20, //threshold
				30, //min line width
				10); //gap between lines
			cannyImage = grayImage.CopyBlank();
			foreach (LineSegment2D line in lines) {
				cannyImage.Draw(line, new Gray(255), 2);
			}
			//lineImage is output image i guess
			#endregion

			#region Find Triangle and Rectangles
			triangleList = new List<Triangle2DF>();
			boxList = new List<RotatedRect>();
			Image<Bgr, byte> triangleRectImage = origImage.CopyBlank();
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
							if (dAngle < 80 || dAngle > 100) {
								isRectangle = false;
								break;
							}
						}
						#endregion

						if (isRectangle) {
							boxList.Add(CvInvoke.MinAreaRect(approxContour));
						}
					}
				}
			}
			#endregion

			#region Draw found shapes on the original image
			foreach (Triangle2DF triangle in triangleList) {
				origImage.Draw(triangle, new Bgr(Color.Yellow), 5);
			}

			foreach (RotatedRect box in boxList) {
				origImage.Draw(box, new Bgr(Color.Red), 5);
			}
			//triangleRectImage is output img i guess
			#endregion
		}

		private void displayImages() {
			UI.displayImage(origImage, Form1.PictureId.Original);
			UI.displayImage(grayImage, Form1.PictureId.Gray);
			UI.displayImage(cannyImage, Form1.PictureId.Canny);
		}

		public void getShapeLists(ref List<Triangle2DF> triangles, ref List<RotatedRect> boxes) {
			lock (visionLock) {
				triangles = new List<Triangle2DF>(triangleList);
				boxes = new List<RotatedRect>(boxList);
			}
		}

		private void getPaperMask() {
			paperMask = grayImage.CopyBlank();

			if (paperPoints == null) {
				paperMask.Draw(new Rectangle(0, 0, paperMask.Width, paperMask.Height), new Gray(255));
			} else {
				Point[] points = new Point[paperPoints.Length];
				for (int i = 0; i < paperPoints.Length; i++) {
					points[i] = new Point((int)(paperMask.Width * paperPoints[i].X), (int)(paperMask.Height * paperPoints[i].Y));
				}
				//paperMask.DrawPolyline(points, true, new Gray(255), -1);
				paperMask.FillConvexPoly(points, new Gray(255));
			}

			paperMaskDirty = false;
		}

		public void setPaperMaskPoints(PointF p1, PointF p2, PointF p3, PointF p4) {
			lock (visionLock) {
				paperPoints = new PointF[] { p1, p2, p3, p4 };
				paperMaskDirty = true;
			}
		}

		public PointF[] getPaperMaskPoints() {
			PointF[] copyArray = new PointF[paperPoints.Length];
			for(int i = 0; i < paperPoints.Length; i++) {
				copyArray[i] = new PointF(paperPoints[i].X, paperPoints[i].Y);
			}

			return copyArray;
		}

		private void calibratePaper() {
			Image<Bgr, byte> rect = origImage.CopyBlank();
			Point[] points = new Point[paperPoints.Length];
			for(int i = 0; i < points.Length; i++) {
				points[i] = new Point((int)(paperPoints[i].X * origImage.Width), (int)(paperPoints[i].Y * origImage.Height));
			}
			//rect.DrawPolyline(points, true, new Bgr(42, 240, 247), 0);
			rect.FillConvexPoly(points, new Bgr(42, 240, 247));
			CvInvoke.AddWeighted(origImage, 0.8, rect, 0.2, 0, origImage);

			foreach(PointF point in paperPoints) {
				origImage.Draw(new CircleF(new PointF(origImage.Width * point.X, origImage.Height * point.Y), 10), new Bgr(42, 240, 247), 3);
			}
			
			paperCalibrater.displayImage(origImage);
		}

		public bool AutoDetectPaper(PaperCalibrater paperCalibrater) {
			lock (visionLock) {

				#region Image Conditioning
				//Grab gray image for shape detection
				grayImage = origImage.Convert<Gray, byte>();
				#endregion

				#region Canny and edge detection
				UMat cannyEdges = new UMat();
				LineSegment2D[] lines;
				double dCannyThreLinking = 120.0;
				double dCannyThres = 180.0;
				CvInvoke.Canny(grayImage, cannyEdges, dCannyThres, dCannyThreLinking);
				lines = CvInvoke.HoughLinesP(cannyEdges,
					1, //Distance resolution in pixel-related units
					Math.PI / 45.0, //Angle resolution measured in radians
					20, //threshold
					30, //min line width
					10); //gap between lines
				cannyImage = grayImage.CopyBlank();
				foreach (LineSegment2D line in lines) {
					cannyImage.Draw(line, new Gray(255), 2);
				}
				//lineImage is output image i guess
				#endregion

				#region Find Triangle and Rectangles
				List<RotatedRect> boxList = new List<RotatedRect>();
				VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
				CvInvoke.FindContours(cannyEdges, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
				int count = contours.Size;
				for (int i = 0; i < count; i++) {
					VectorOfPoint contour = contours[i];
					VectorOfPoint approxContour = new VectorOfPoint();
					CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
					if (CvInvoke.ContourArea(approxContour, false) > 250) { //only consider areas that are large enough.
						if (approxContour.Size == 4) { //Four vertices, must be a square!
							#region Determine if all angles are within 80-100 degrees
							bool isRectangle = true;
							Point[] pts = approxContour.ToArray();
							LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
							for (int j = 0; j < edges.Length; j++) {
								double dAngle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
								if (dAngle < 80 || dAngle > 100) {
									isRectangle = false;
									break;
								}
							}
							#endregion

							if (isRectangle) {
								boxList.Add(CvInvoke.MinAreaRect(approxContour));
							}
						}
					}
				}
				#endregion

				if (boxList.Count == 0) {
					return false;
				}

				#region Find the largest rectangle
				float largestSize = 0;
				RotatedRect largest = boxList[0]; //Assign initial values to avoid compile errors.
				foreach (RotatedRect box in boxList) {
					float area = box.Size.Width * box.Size.Height;
					if (area > largestSize) {
						largest = box;
						largestSize = area;
					}
				}
				#endregion

				#region Organize Points
				List<PointF> points = new List<PointF>();
				points.AddRange(largest.GetVertices());
				PointF[] leftPoints = new PointF[2];

				for (int i = 0; i < 2; i++) {
					float leftVal = float.MaxValue;
					foreach(PointF point in points) {
						if (point.X < leftVal) {
							leftVal = point.X;
							leftPoints[i] = point;
						}
					}
					points.Remove(leftPoints[i]);
				}

				if (leftPoints[0].Y < leftPoints[1].Y) {
					paperPoints[0] = leftPoints[0];
					paperPoints[3] = leftPoints[1];
				} else {
					paperPoints[0] = leftPoints[1];
					paperPoints[3] = leftPoints[0];
				}

				if (points[0].Y < points[1].Y) {
					paperPoints[1] = points[0];
					paperPoints[2] = points[1];
				} else {
					paperPoints[1] = points[1];
					paperPoints[2] = points[0];
				}
				#endregion

				#region Calculate correct point position
				for(int i = 0; i < paperPoints.Length; i++) {
					paperPoints[i].X /= origImage.Width;
					paperPoints[i].Y /= origImage.Height;
				}
				#endregion

				paperCalibrater.refresh(this);
				return true;
			}
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
					return input.setFile(filename);
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
					return input.setFile(filename);
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

				return input.requestLoadInput();
			}
		}

		public bool loadVideo() {
			lock (visionLock) {
				if (!(input is VideoInput)) {
					bool wasPlaying = input.isPlaying();
					input.Dispose();
					input = new ImageInput(null);
					if (!wasPlaying) input.pause();
				}

				return input.requestLoadInput();
			}
		}

		public bool loadInput() {
			lock (visionLock) {
				return input.requestLoadInput();
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
