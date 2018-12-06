using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace RobotArmUR2.Util {

	/// <summary>
	/// Allows easier use of various input sources. Loads images from camera, images (including animated GIFs), and videos.
	/// Also has functions to prompt the user to load a file, or save a screenshot.
	/// </summary>
	public class ImageStream : IDisposable {
		private static readonly object streamLock = new object();
		private static readonly object captureLock = new object();

		//Used to prompt the user to open/save files
		private static OpenFileDialog openDialog;
		private static SaveFileDialog saveDialog;

		private static string[,] imageFileExtensions = {
			{ ".bmp", "Bmp" }, 
			{ ".emf", "Emf" },
			{ ".exif", "Exif" },
			{ ".gif", "Gif" },
			{ ".ico", "Icon" },
			{ ".jpeg", "Jpeg" },
			{ ".jpg", "Jpg" },
			{ ".png", "Png" },
			{ ".tiff", "Tiff" }
		};

		private static string[] videoFileExtensions = new string[] { ".mkv", ".flv", ".f4v", ".ogv", ".ogg", ".gifv", ".mng", ".avi", ".mov", ".wmv", ".rm", ".rmvb", ".asf", ".amv", ".mp4", ".m4p", ".m4v", ".m4v", ".svi" };

		//Sets up the file dialogs
		static ImageStream() {
			openDialog = new OpenFileDialog();
			openDialog.RestoreDirectory = true;

			string openFilter = "Media Files (Image, Video)|";
			imageFileExtensions.GetLength(0);
			for(int i = 0; i < imageFileExtensions.GetLength(0); i++) {
				imageFileExtensions[i, 0] = imageFileExtensions[i, 0].ToLower();
				openFilter += "*" + imageFileExtensions[i, 0] + ";";
			}
			for(int i = 0; i < videoFileExtensions.Length; i++) {
				videoFileExtensions[i] = videoFileExtensions[i].ToLower();
				openFilter += "*" + videoFileExtensions[i] + ";";
			}
			openFilter = openFilter.TrimEnd(';');
			openDialog.Filter = openFilter;


			saveDialog = new SaveFileDialog();
			saveDialog.RestoreDirectory = true;
			saveDialog.AddExtension = true;

			string saveFilter = "";
			string ext;
			for(int i = 0; i < imageFileExtensions.GetLength(0); i++) {
				ext = imageFileExtensions[i, 0];
				if(ext == ".png") saveDialog.FilterIndex = i + 1;
				saveFilter += imageFileExtensions[i, 1] + "(*" + ext + ")|*" + ext + "|";
			}
			saveFilter = saveFilter.TrimEnd('|');
			saveDialog.Filter = saveFilter;
		}

		private Thread grabbingThread; //New thread that continously tries to grab images from the source.
		private VideoCapture capture; //EmguCV class that assists in loading camera/files
		private Mat imageBuffer; //The last image that was grabbed, used for taking screenshots

		private volatile bool exitThread = false;
		private volatile bool isPlaying = false;

		private Stopwatch timer = new Stopwatch(); //Used to set the correct FPS for video playback.
		private FPSCounter fpsCounter = new FPSCounter(); //Calculates the FPS.

		#region Events and Handlers
		public delegate void NewImageHandler(ImageStream sender, Mat image); //Called when a new image is parsed.
		public event NewImageHandler OnNewImage;

		public delegate void StreamEndedHandler(ImageStream sender); //Called when the current stream ends and is closed.
		public event StreamEndedHandler OnStreamEnded;
		#endregion

		#region Properties
		/// <summary>
		/// The image width of the image from the current input source.
		/// </summary>
		public int Width { get { lock (streamLock) { if (capture == null) return 0; else return capture.Width; } } }

		/// <summary>
		/// The image height of the image from the current input source.
		/// </summary>
		public int Height { get { lock (streamLock) { if (capture == null) return 0; else return capture.Height; } } }

		/// <summary>
		/// The width and height of the image from the current input source.
		/// </summary>
		public Size Size { get { lock (streamLock) { if (capture == null) return new Size(0, 0); else return new Size(capture.Width, capture.Height); } } }

		/// <summary>
		/// Whether or not there is an opened input source.
		/// </summary>
		public bool IsOpened { get { lock (streamLock) { return capture != null; } } }

		/// <summary>
		/// The target FPS defined by the input source.
		/// </summary>
		public float TargetFPS { get; private set; } = 0f;

		/// <summary>
		/// The estimated measured FPS that is being achieved. Not accurate, but close.
		/// </summary>
		public float FPS { get; private set; } = 0f;

		/// <summary>
		/// Whether or not the input image should be flipped horizontally.
		/// </summary>
		public bool FlipHorizontal {
			get { return flipHorizontal; }

			set {
				lock (streamLock) {
					flipHorizontal = value;
					if (capture != null) capture.FlipHorizontal = flipHorizontal;
				}
			}
		}
		private bool flipHorizontal = false;

		/// <summary>
		/// Whether or not the input image should be flipped vertically.
		/// </summary>
		public bool FlipVertical {
			get { return flipVertical; }

			set {
				lock (streamLock) {
					flipVertical = value;
					if (capture != null) capture.FlipVertical = flipVertical;
				}
			}
		}
		private bool flipVertical = false;

		public float ImageFps { get => imageFPS; set { if (value >= 0) imageFPS = value; } }
		private float imageFPS = 15;

		public StreamType StreamSource { get => streamType; }
		private StreamType streamType = StreamType.None;
		#endregion

		//TODO add auto-loop option
		//TODO add option to artificially change FPS (i.e. camera runs at 60 fps, but you selected  15 fps)

		public ImageStream() {
			grabbingThread = new Thread(ImageGrabbingLoop);
			grabbingThread.Name = "Image Grabbing Thread";
			grabbingThread.IsBackground = true;
			grabbingThread.Start();
		}
		
		~ImageStream() {
			Dispose();
		}

		/// <summary>
		/// Stops the stream and releases any inputs that were being used. Image buffer is cleared and FPS is reset to zero.
		/// </summary>
		/// <remarks>
		/// Unlike most implementations of Dispose, the class can still be used after calling Dispose().
		/// </remarks>
		public void Dispose() {
			exitThread = true;//TODO on stream end
			grabbingThread.Join();
			grabbingThread = null;
			lock (streamLock) {
				if (capture != null) capture.Dispose();
				capture = null;
				//We do NOT want to dispose imageBuffer, in the case the image is being used elsewhere!
				imageBuffer = null;
				fpsCounter.Reset();
			}
		}

		#region Image Grabbing Thread
		private void ImageGrabbingLoop() {
			while (!exitThread) {
				int waitMS = 0;
				timer.Restart();
				lock (captureLock) {
					//Safely and quickly grab current settings
					StreamType streamType = StreamType.None;
					bool isPlaying = false;
					Mat imageBuffer = null;
					lock (streamLock) {
						streamType = this.streamType;
						isPlaying = this.isPlaying;
						imageBuffer = this.imageBuffer;
					}

					//Check state
					if(streamType == StreamType.None) {
						waitMS = (1000 / 120); //Nothing to capture, just wait for another input.
					}else if (!isPlaying) { //check if paused
						if(imageBuffer != null) { //If paused and there is a buffered image, keep sending it
							waitMS = fireNewFrame(StreamType.Image, imageBuffer);
						}
					}else if(streamType == StreamType.Image && imageBuffer != null) { //Streaming an image that has already been loaded
						waitMS = fireNewFrame(streamType, imageBuffer);
					} else {
						waitMS = grabImage(streamType);
					}
				}
				long millis = timer.ElapsedMilliseconds;
				timer.Stop();
				if ((waitMS > 0) && (millis < waitMS)) {
					int ms = (int)millis;
					waitMS -= ms;
					if(waitMS > 0) Thread.Sleep(waitMS);
				}
			}
		}

		private int grabImage(StreamType streamType) { //Returns Target FPS is ms
			if (streamType == StreamType.None) throw new NotImplementedException();
			lock (captureLock) {
				int waitMS = 0;
				Mat newImage = new Mat();
				if (capture.Grab() && capture.Retrieve(newImage)) {//TODO Access violation exception
					//Source is still open.
					this.imageBuffer = newImage;
					waitMS = fireNewFrame(streamType, newImage);
				} else {
					lock (streamLock) {
						//Source must be closed.
						this.imageBuffer = null;
						this.streamType = StreamType.None;
						TargetFPS = 0;
						FPS = 0;
						fpsCounter.Reset();
						OnStreamEnded?.Invoke(this);
						waitMS = 1000 / 120;
					}

				}
				return waitMS;
			}
		}

		private float getTargetFPS(StreamType type) {
			switch (type) {
				case StreamType.Image: return imageFPS;
				case StreamType.Video:
				case StreamType.Camera:
					return (float)capture.GetCaptureProperty(CapProp.Fps);
				default: return 0;
			}
		}

		private int fireNewFrame(StreamType type, Mat image) { //Returns "delayMS" value
			int delayMS = 0;
			float targetFps = getTargetFPS(type);
			TargetFPS = targetFps;
			if (type != StreamType.Camera /*&& TargetFPS != 0*/) delayMS = (int)(1000 / targetFps);
			FPS = fpsCounter.Tick();
			OnNewImage?.Invoke(this, image);
			return delayMS;
		}
		#endregion

		#region Stream Controls
		/// <summary>
		/// Starts or resumes the selected input source and starts grabbing images. 
		/// When an image is grabbed, onNewImage is fired.
		/// </summary>
		/// <returns>true if the source was successfully started.</returns>
		public bool Play() {
			lock (streamLock) {
				/*	if (capture != null && capture.IsOpened) {
						capture.Start();
						return true;
					} else {
						return false;
					}*/
				isPlaying = true;
				return true;
			}

		}

		/// <summary>
		/// Stops grabbing images from the source until resumed.
		/// </summary>
		public void Pause() {
			lock (streamLock) {
				//capture.Stop
				isPlaying = false;
			}
		}

		/// <summary>
		/// Closes the input and fires onStreamEnded event.
		/// </summary>
		public void Stop() {
			/*lock (listenerLock) {//Ensures that event is fired only after image grabbed by capture is finished sending. 
				lock (streamLock) {
					Dispose();
					OnStreamEnded?.Invoke(this); //TODO Call all events like this
				}
			}*/
			lock (captureLock) {
				lock (streamLock) {
					capture = null;
					isPlaying = false;
					streamType = StreamType.None;
					imageBuffer = null;
				}
			}
		}
		#endregion

		#region Input Source Selection

		#region Select Camera
		/// <summary>
		/// Selects the default camera as the source.
		/// </summary>
		public void SelectCamera() {
			lock (captureLock) {
				lock (streamLock) {
					//Stop();
					capture = new VideoCapture();
					setupCapture();
					streamType = StreamType.Camera;
				}
			}
		}

		/// <summary>
		/// Select a camera with the specified index as the source.
		/// </summary>
		/// <param name="index">Index of camera to be selected.</param>
		public void SelectCamera(int index) {
			lock (captureLock) {
				lock (streamLock) {
					//Stop();
					capture = new VideoCapture(index);
					setupCapture();
					streamType = StreamType.Camera;
				}
			}
		}
		#endregion

		#region Load Files
		private bool loadGifFromFile(string filepath) {
			try {
				using (Image img = Image.FromFile(filepath)) {
					if (!img.RawFormat.Equals(ImageFormat.Gif)) return false;
					FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
					int frameCount = img.GetFrameCount(dimension);

					if (frameCount == 1) streamType = StreamType.Image;
					else if (frameCount > 1) streamType = StreamType.Video;
					else return false;

					capture = new VideoCapture(filepath);
					setupCapture();
					return true;
				}
			} catch {
				return false;
			}
		}

		private static StreamType checkExtension(string ext) {
			ext = ext.ToLower();
			for(int i = 0; i < imageFileExtensions.GetLength(0); i++) {
				if (ext == imageFileExtensions[i, 0]) return StreamType.Image;
			}

			foreach (string extName in videoFileExtensions) {
				if (ext == extName) return StreamType.Video;
			}

			return StreamType.None;
		}

		/// <summary>
		/// Loads a file to be used as the source. Supports most image and video files.
		/// </summary>
		/// <param name="filepath">Full path to the file to be loaded.</param>
		/// <returns>true if the file was successfully loaded.</returns>
		public bool LoadFile(string filepath) { //TODO method too big
			if (filepath == null) return false;
			lock (captureLock) {
				lock (streamLock) {
					if (!File.Exists(filepath)) return false; //Wait until here to check if file exists, in case we had to wait for the lock to be released.
					streamType = StreamType.None;
					string ext = Path.GetExtension(filepath).ToLower();
					if(ext == ".gif") {
						return loadGifFromFile(filepath);
					} else {
						streamType = checkExtension(ext);
						if (streamType != StreamType.None) {
							capture = new VideoCapture(filepath);
							setupCapture();
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Loads a file from the local solution files.
		/// </summary>
		/// <param name="filepath">Relative path to file from solution root file.</param>
		/// <returns>true of the file was successfully loaded.</returns>
		public bool LoadLocalFile(string filepath) {
			return LoadFile(System.IO.Directory.GetCurrentDirectory() + "\\" + filepath); //NOTE: '\\' translates to '\' in a string, because it is a special character.
		}

		/// <summary>
		/// Prompts the user to select a file, then attempts to load the selected file as the source.
		/// </summary>
		/// <returns>true if the file was successfully loaded, also returns false if user cancelled operation.</returns>
		public bool PromptUserLoadFile() {
			if (openDialog.ShowDialog() == DialogResult.OK) {
				return LoadFile(openDialog.FileName);
			} else {
				return false;
			}
		}
		#endregion

		//Sets up a capture so it is ready to use.
		private void setupCapture() {
			capture.FlipHorizontal = flipHorizontal;
			capture.FlipVertical = flipVertical;
			imageBuffer = null;
			isPlaying = false;
			fpsCounter.Reset();
			FPS = 0;
			TargetFPS = 0;
		}
		#endregion

		#region Save Screenshots
		//Captures a screenshot, returns null if it can't
		private Mat captureScreenshot() {
			Mat screenshot = new Mat();
			lock (streamLock) {
				if (imageBuffer == null) return null;
				imageBuffer.CopyTo(screenshot);
			}
			if (screenshot.IsEmpty) return null;
			return screenshot;
		}

		/// <summary>
		/// Take a screenshot and save it to the file specified.
		/// </summary>
		/// <remarks>
		/// The path should include the file name and extension.
		/// </remarks>
		/// <param name="filepath">The full path to where the file should be saved.</param>
		/// <returns>true if the screenshot was successfully saved.</returns>
		public bool SaveScreenshot(string filepath) {
			Mat screenshot = captureScreenshot();
			return SaveScreenshot(screenshot, filepath);
		}

		public bool SaveScreenshot(Mat img, string filepath) {
			if (img == null || filepath == null) return false;
			img.Save(filepath);
			return true;
		}

		/// <summary>
		/// Takes a screenshot and saves it to local solution files.
		/// </summary>
		/// <param name="filepath">Relative path to save location in solution files.</param>
		/// <returns>true if successfully saved.</returns>
		public bool SaveLocalScreenshot(string filepath) {
			return SaveLocalScreenshot(System.IO.Directory.GetCurrentDirectory() + "\\" + filepath); //NOTE: '\\' translates to '\' in a string, because it is a special character.
		}

		public bool SaveLocalScreenshot(Mat img, string filepath) {
			return SaveScreenshot(img, System.IO.Directory.GetCurrentDirectory() + "\\" + filepath); //NOTE: '\\' translates to '\' in a string, because it is a special character.
		}

		public bool PromptUserSaveScreenshot(Mat img) {
			if (img == null) return false;

			if (saveDialog.ShowDialog() == DialogResult.OK) {
				img.Save(saveDialog.FileName);
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Takes a screenshot and prompts the user to select a save location and extension.
		/// </summary>
		/// <returns>true if successfully saved, also returns false if user cancelled operation.</returns>
		public bool PromptUserSaveScreenshot() {
			Mat screenshot = captureScreenshot();
			return PromptUserSaveScreenshot(screenshot);
		}
		#endregion

		public enum StreamType {
			None,
			Camera,
			Image,
			Video
		}

	}

}
