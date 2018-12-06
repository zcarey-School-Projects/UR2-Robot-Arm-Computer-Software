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
		/// <summary>
		/// Used for checking if disposed or not
		/// </summary>
		private static readonly object streamLock = new object(); //TODO SHIT! For multiple objects this doesnt work!

		/// <summary>
		/// Prevent running while a frame is being grabbed.
		/// </summary>
		private static readonly object captureLock = new object();

		/// <summary>
		/// Ensures Dispose is only being called by one thread at a time.
		/// </summary>
		private static readonly object disposeLock = new object();

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
		public int Width { get { lock (captureLock) { return (capture == null) ? 0 : capture.Width; } } } 

		/// <summary>
		/// The image height of the image from the current input source.
		/// </summary>
		public int Height { get { lock (captureLock) { return (capture == null) ? 0 : capture.Height; } } }

		/// <summary>
		/// The width and height of the image from the current input source.
		/// </summary>
		public Size Size { get { lock (captureLock) { return (capture == null) ? (new Size(0, 0)) : (new Size(capture.Width, capture.Height)); } } }

		/// <summary>
		/// Whether or not there is an opened input source.
		/// </summary>
		public bool IsOpened { get { return capture != null; } }

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
				lock (captureLock) { 
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
				lock (captureLock) {
					flipVertical = value;
					if (capture != null) capture.FlipVertical = flipVertical;
				}
			}
		}
		private bool flipVertical = false;

		public float ImageFps { get => imageFPS; set { if (value >= 0) imageFPS = value; } }
		private float imageFPS = 15;

		public StreamType StreamSource { get; private set; } = StreamType.None;
		#endregion

		//TODO add auto-loop option
		//TODO add option to artificially change FPS (i.e. camera runs at 60 fps, but you selected  15 fps)

		public ImageStream() {
			lock (streamLock) {
				grabbingThread = new Thread(imageGrabbingLoop);
				grabbingThread.Name = "Image Grabbing Thread";
				grabbingThread.IsBackground = true;
				grabbingThread.Start();
			}
		}
		
		~ImageStream() {
			Dispose();
		}

		/// <summary>
		/// Stops the grabbing thread and releases any inputs that were being used. Image buffer is cleared and FPS is reset to zero.
		/// </summary>
		/// <remarks>
		/// If a stream was running, calling this fires EndOfStream
		/// </remarks>
		public void Dispose() {
			lock (disposeLock) {
				lock (streamLock) {
					if (grabbingThread == null) return;
				}

				exitThread = true;//TODO on stream end
				grabbingThread.Join();

				Thread thread = grabbingThread;
				lock (streamLock) {
					grabbingThread = null; //Signals to other functions that we are disposed
				}

				StreamSource = StreamType.None;
				if (capture != null) capture.Dispose();
				capture = null;
				imageBuffer = null; //We do NOT want to dispose imageBuffer, in the case the image is being used elsewhere!
				fpsCounter = null;
				timer.Stop();
				timer = null;
			}
		}

		#region Image Grabbing Thread
		private void imageGrabbingLoop() {
			try {
				while (!exitThread) {
					StreamType effectiveSource = StreamType.None; //After settings, what the source effectivley is (i.e. is paused, acts as an iamge)
					Mat newImage = null;
					timer.Restart();

					//Grab images and calculate wait time
					lock (captureLock) {
						if (StreamSource != StreamType.None) { //There is a source we want to read!
							if (!isPlaying || (StreamSource == StreamType.Image && imageBuffer != null)) { //Streaming an image that has already been loaded OR paused stream
								if (imageBuffer != null) { //Check for paused stream (!isPlaying)
									effectiveSource = StreamType.Image;
									newImage = imageBuffer;
								}
							} else { //Attempt to grab a frame from the source.
								newImage = new Mat();
								if (grabImage(newImage)) {
									//Source is still open.
									effectiveSource = StreamSource;
								} else {
									//Source must be closed.
									endStream();//TODO be sure 
								}
							}
						}

						if (effectiveSource == StreamType.None) TargetFPS = 0;
						else {
							TargetFPS = (effectiveSource == StreamType.Image) ? imageFPS : (float)capture.GetCaptureProperty(CapProp.Fps);
							FPS = fpsCounter.Tick();
						}

						imageBuffer = (newImage == null || newImage.IsEmpty) ? null : newImage;
					}

					int delayMS = 0; //The target time to wait;

					if (effectiveSource == StreamType.None) delayMS = 1;
					else {
						OnNewImage?.Invoke(this, newImage);
						if (effectiveSource != StreamType.Camera) delayMS = (int)(1000 / TargetFPS);
					}

					//Wait the necessary time to acieve desired FPS
					timer.Stop();
					long millis = timer.ElapsedMilliseconds;
					if (millis < delayMS && millis >= 0) delayMS -= (int)millis;
					if (delayMS > 0) Thread.Sleep(delayMS);
				}
			} finally {
				endStream();
			}
		}

		private bool grabImage(Mat outputImage) {
			return capture.Grab() && capture.Retrieve(outputImage); //TODO Access violation exception
		}
		#endregion

		private void endStream() {
			lock (captureLock) {
				lock (streamLock) {
					if (grabbingThread == null) return;
				}

				if (capture != null) capture.Dispose();
				capture = null;
				imageBuffer = null;
				TargetFPS = 0;
				FPS = 0;
				fpsCounter.Reset();
				if(StreamSource != StreamType.None) { //Only fire event if the stream is closing, and wasnt already closed.
					StreamSource = StreamType.None;
					OnStreamEnded?.Invoke(this);
				}
			}
		}

		#region Stream Controls
		/// <summary>
		/// Starts or resumes the selected input source and starts grabbing images. 
		/// When an image is grabbed, onNewImage is fired.
		/// </summary>
		/// <returns>true if the source was successfully started.</returns>
		public void Play() {
			isPlaying = true;
		}

		/// <summary>
		/// Stops grabbing images from the source until resumed.
		/// </summary>
		public void Pause() {
			isPlaying = false;
		}

		/// <summary>
		/// Closes the input and fires onStreamEnded event.
		/// </summary>
		public void Stop() {
			endStream();
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
					if (grabbingThread == null) return;
					capture = new VideoCapture();
					StreamSource = StreamType.Camera;
					setupCapture();
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
					if (grabbingThread == null) return;
					capture = new VideoCapture(index);
					StreamSource = StreamType.Camera;
					setupCapture();
				}
			}
		}
		#endregion

		#region Load Files
		//Loads a gif image to read it's frame counts to check if should load as an image or video.
		private static StreamType getGifStreamType(string filepath) {
			try {
				using (Image img = Image.FromFile(filepath)) {
					if (!img.RawFormat.Equals(ImageFormat.Gif)) return StreamType.None;
					FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
					int frameCount = img.GetFrameCount(dimension);

					if (frameCount == 1) return StreamType.Image;
					else if (frameCount > 1) return StreamType.Video;
					else return StreamType.None;
				}
			} catch {
				return StreamType.None;
			}
		}

		private static StreamType getExtensionStreamType(string ext) {
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
		public bool LoadFile(string filepath) {
			lock (captureLock) {
				lock (streamLock) {
					if (grabbingThread == null) return false;
					if (filepath == null || !File.Exists(filepath)) {//Wait until here to check if file exists, in case we had to wait for the lock to be released.
						endStream(); //Failed to load, must end the current stream.
						return false; 
					}
					
					string ext = Path.GetExtension(filepath).ToLower();
					StreamSource = (ext == ".gif") ? getGifStreamType(filepath) : getExtensionStreamType(ext);

					if (StreamSource != StreamType.None) {
						capture = new VideoCapture(filepath);
						setupCapture();
						return true;
					} else {
						endStream();
						return false;
					}
				}
			}
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
			lock (streamLock) {
				if (grabbingThread == null) return false;
			}
			if (openDialog.ShowDialog() == DialogResult.OK) {
				return LoadFile(openDialog.FileName);
			} else {
				return false;
			}
		}
		#endregion

		//Sets up a capture so it is ready to use.
		//Assumed to be called from inside a streamLock
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
			Mat screenshot = imageBuffer; //Atomic thread-safe grab of the buffer
			if (screenshot == null || screenshot.IsEmpty) return null;
			else{
				Mat copy = new Mat();
				screenshot.CopyTo(copy);
				return copy;
			}
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

		public static bool SaveScreenshot(Mat img, string filepath) {
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
			if (img == null || img.IsEmpty) return false;

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
			return PromptUserSaveScreenshot(captureScreenshot());
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
