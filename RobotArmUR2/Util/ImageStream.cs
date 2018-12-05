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

		//Locks that help with race conditions.
		private static readonly object streamLock = new object();
		private static readonly object captureLock = new object();
		//private static readonly object listenerLock = new object();

		//Used to prompt the user to open/save files
		private static OpenFileDialog openDialog;
		private static SaveFileDialog saveDialog;

		private static string[][] imageFileExtensions = new string[][] {
			new string[]{ ".bmp", "Bmp" },
			new string[]{ ".emf", "Emf" },
			new string[]{ ".exif", "Exif" },
			new string[]{ ".gif", "Gif" },
			new string[]{ ".ico", "Icon" },
			new string[]{ ".jpeg", "Jpeg" },
			new string[]{ ".jpg", "Jpg" },
			new string[]{ ".png", "Png" },
			new string[]{ ".tiff", "Tiff" },
		};

		private static string[] videoFileExtensions = new string[] { ".mkv", ".flv", ".f4v", ".ogv", ".ogg", ".gifv", ".mng", ".avi", ".mov", ".wmv", ".rm", ".rmvb", ".asf", ".amv", ".mp4", ".m4p", ".m4v", ".m4v", ".svi" };

		//Sets up the file dialogs
		static ImageStream() {
			string openFilter = "Media Files (Image, Video)|";
			foreach (string[] extNames in imageFileExtensions) {
				openFilter += "*" + extNames[0] + ";";
			}
			foreach(string ext in videoFileExtensions) {
				openFilter += "*" + ext + ";";
			}
			openFilter = openFilter.TrimEnd(';');

			openDialog = new OpenFileDialog();
			openDialog.RestoreDirectory = true;
			openDialog.Filter = openFilter;

			string saveFilter = "";
			foreach(string[] extNames in imageFileExtensions) {
				saveFilter += extNames[1] + "(*" + extNames[0] + ")|*" + extNames[0] + "|";
			}
			saveFilter = saveFilter.TrimEnd('|');

			saveDialog = new SaveFileDialog();
			saveDialog.RestoreDirectory = true;
			saveDialog.AddExtension = true;
			saveDialog.Filter = saveFilter;
			saveDialog.FilterIndex = 7;
		}

		private VideoCapture capture; //EmguCV class that assists in loading camera/files
		private StreamType streamType = StreamType.None;
		private Mat imageBuffer; //The last image that was grabbed, used for taking screenshots
		private Stopwatch timer = new Stopwatch(); //Used to set the correct FPS for video playback.
		private FPSCounter fpsCounter = new FPSCounter(); //Calculates the FPS.

		#region Events and Handlers
		public delegate void NewImageHandler(ImageStream sender, Mat image); //Called when a new image is parsed.
		public event NewImageHandler OnNewImage;

		public delegate void StreamEndedHandler(ImageStream sender); //Called when the current stream ends and is closed.
		public event StreamEndedHandler OnStreamEnded;

		//public delegate void NullImageHandler(ImageStream sender);
		//public event NullImageHandler OnNullImage();
		#endregion

		/// <value>The image width of the image from the current input source.</value>
		public int Width { get { lock (streamLock) { if (capture == null) return 0; else return capture.Width; } } }

		/// <value>The image height of the image from the current input source.</value>
		public int Height { get { lock (streamLock) { if (capture == null) return 0; else return capture.Height; } } }

		/// <value>The width and height of the image from the current input source.</value>
		public Size Size { get { lock (streamLock) { if (capture == null) return new Size(0, 0); else return new Size(capture.Width, capture.Height); } } }

		/// <value>Whether or not there is an opened input source.</value>
		public bool IsOpened { get { lock (streamLock) { if (capture == null) return false; else return capture.IsOpened; } } } 

		/// <value>The target FPS defined by the input source.</value>
		public float TargetFPS { get; private set; } = 0f;

		/// <value>The estimated measured FPS that is being achieved. Not accurate, but close.</value>
		public float FPS { get; private set; } = 0f;

		/// <value>Whether or not the input image should be flipped horizontally.</value>
		public bool FlipHorizontal {
			get {
				return flipHorizontal;
			}

			set {
				lock (streamLock) {
					flipHorizontal = value;
					if (capture != null) capture.FlipHorizontal = flipHorizontal;
				}
			}
		}
		private bool flipHorizontal = false;

		/// <value>Whether or not the input image should be flipped vertically.</value>
		public bool FlipVertical {
			get {
				return flipVertical;
			}

			set {
				lock (streamLock) {
					flipVertical = value;
					if (capture != null) capture.FlipVertical = flipVertical;
				}
			}
		}
		private bool flipVertical = false;

		private Thread GrabbingThread;
		private volatile bool exitThread = false;
		private volatile bool isPlaying = false;
		private float imageFPS = 15; //TODO public

		//TODO add capture type (camera, video, image)
		//TODO add auto-loop option
		//TODO add option to artificially change FPS (i.e. camera runs at 60 fps, but you selected  15 fps)

		public ImageStream() {
			GrabbingThread = new Thread(ImageGrabbingLoop);
			GrabbingThread.Name = "Image Grabbing Thread";
			GrabbingThread.IsBackground = true;
			GrabbingThread.Start();
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
			exitThread = true;
			GrabbingThread.Join();
			GrabbingThread = null;
			lock (streamLock) {
				if (capture != null) capture.Dispose();
				capture = null;
				//We do NOT want to dispose imageBuffer, in the case the image is being used elsewhere!
				imageBuffer = null;
				fpsCounter.Reset();
			}
		}

		private void ImageGrabbingLoop() {
			while (!exitThread) {
				int waitMS = 0;
				lock (captureLock) {
					StreamType streamType = StreamType.None;
					bool isPlaying = false;
					Mat imageBuffer = null;
					lock (streamLock) {
						streamType = this.streamType;
						isPlaying = this.isPlaying;
						imageBuffer = this.imageBuffer;
					}

					if (streamType == StreamType.None) {
						waitMS = (1000 / 120);
					} else if (!isPlaying) {
						if (imageBuffer != null) {
							TargetFPS = imageFPS;
							FPS = fpsCounter.Tick();
							OnNewImage?.Invoke(this, imageBuffer);
							waitMS = ((int)(1000 / imageFPS));
						}
					} else {
						Mat newImage = new Mat();
						if (capture.Grab() && capture.Retrieve(newImage)) {//Access violation exception
							//Source is still open.
							this.imageBuffer = newImage;
							if (streamType == StreamType.Image) {
								TargetFPS = imageFPS;
								waitMS = ((int)(1000 / imageFPS));
							} else if (streamType == StreamType.Video) {
								float targetFPS = (float)capture.GetCaptureProperty(CapProp.Fps);
								TargetFPS = targetFPS;
								waitMS = ((int)(1000 / targetFPS));
							} else if (streamType == StreamType.Camera) {
								TargetFPS = (float)capture.GetCaptureProperty(CapProp.Fps);
							} else { 
								throw new NotImplementedException(); //TODO exception
							}
							FPS = fpsCounter.Tick();
							OnNewImage?.Invoke(this, newImage);
						} else if (streamType == StreamType.Image && imageBuffer != null) {
							//An image source was loaded, and we already grabbed the image.
							TargetFPS = imageFPS;
							FPS = fpsCounter.Tick(); //TODO just put tick into a "SendNewImage" method?
							OnNewImage?.Invoke(this, imageBuffer);
							waitMS = ((int)(1000 / imageFPS));
						} else {
							lock (streamLock) {
								//Source must be closed.
								this.imageBuffer = null;
								this.streamType = StreamType.None;
								//TODO OnStream End
								TargetFPS = 0;
								FPS = 0;
								fpsCounter.Reset();
								OnStreamEnded?.Invoke(this);
							}

						}
					}
				}

				if (waitMS > 0) Thread.Sleep(waitMS);
			}
		}
		/*
		//Method event listener that fires when a new image is grabbed from source.
		private void onNewImage(object sender, EventArgs e) {
			int delayTime = 0;
			lock (listenerLock) {
				Mat tempBuffer = null;
				timer.Restart();
				FPS = fpsCounter.Tick();
				lock (streamLock) {
					if (capture == null || !capture.IsOpened ) return;
					capture.Retrieve(imageBuffer); //Documentation says gray image, but actually retrieves a colored image. //TODO nullptr checks
					TargetFPS = (float)capture.GetCaptureProperty(CapProp.Fps);
					tempBuffer = imageBuffer;
				}

				if (tempBuffer != null) OnNewImage?.Invoke(this, tempBuffer); //Use temp buffer so imageBuffer can be modified without cause for threading concern.
				
				if(TargetFPS != 0) {
					delayTime = (int)(1000.0 / TargetFPS); //The total time between frames
					long timerMS = timer.ElapsedMilliseconds; //Time it took us to proccess one frame
					if(timerMS <= delayTime) {
						delayTime -= (int)timerMS;
					} else {
						delayTime = 0;
					}
				}
			}

			if (delayTime < 0) delayTime = 1000 / 15; //Default to 15 FPS for overflow errors
			if(delayTime > 0) {
				if (delayTime > (1000 / 15)) delayTime = 1000 / 15; //Anything slower that 15 FPS we don't want to allow.
				Thread.Sleep(delayTime);
			}
		}
	*/
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
					} else {
						foreach(string[] extNames in imageFileExtensions) {
							if (ext == extNames[0]) {
								streamType = StreamType.Image;
								break;
							}
						}

						if(streamType == StreamType.None) {
							foreach(string extName in videoFileExtensions) {
								if(ext == extName) {
									streamType = StreamType.Video;
									break;
								}
							}
						}

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

		//Sets up a capture so it is ready to use.
		private void setupCapture() {
			//capture.ImageGrabbed += onNewImage;
			capture.FlipHorizontal = flipHorizontal;
			capture.FlipVertical = flipVertical;
			//imageBuffer = new Mat();
			imageBuffer = null;
			isPlaying = false;
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

		private enum StreamType {
			None,
			Camera,
			Image,
			Video
		}

	}

}
