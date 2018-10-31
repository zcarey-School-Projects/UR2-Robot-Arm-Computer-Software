using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace RobotHelpers.InputHandling {

	public abstract class InputHandler {

		protected static readonly object inputLock = new object();
		protected static readonly object screenshotLock = new object();

		private Stopwatch timer;
		private Image<Bgr, byte> frameBuffer;
		private volatile bool playing = true;

		private static SaveFileDialog saveDialog;

		static InputHandler(){
			saveDialog = new SaveFileDialog();
			saveDialog.RestoreDirectory = true;
			saveDialog.AddExtension = true;
			saveDialog.Filter = "BMP (*.bmp)|*.bmp|EMF (*.emf)|*.emf|EXIF (*.exif)|*.exif|GIF (*.gif)|*.gif|Icon(*.ico)|*.ico|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|TIFF (*.tiff)|*.tiff|WMF (*.wmf)|*.wmf";
			saveDialog.DefaultExt = "*.png";
		}

		protected InputHandler() {
			timer = new Stopwatch();
			timer.Start();
		}

		~InputHandler() {
			Dispose();
		}

		public void Dispose() {
			lock (inputLock) {
				if (frameBuffer != null) frameBuffer.Dispose();
				frameBuffer = null;
				playing = false;
				onDispose();
			}
		}

		protected abstract void onDispose();

		/// <summary>
		/// Returns the path to the current working directory, i.e. Files in your solution.
		/// </summary>
		/// <returns></returns>
		public static String GetWorkingDirectory() {
			return System.IO.Directory.GetCurrentDirectory() + "\\";
		}

		/// <summary>
		/// <para>Waits the required amount of time then returns the next loadded frame.</para>
		/// <para>If an image file is loaded, the method will block to match 15fps.</para>
		/// </summary>
		/// <returns>null if there was no frame to read.</returns>
		public Image<Bgr, byte> GetFrame() {
			//Block read until specified delay.
			while(timer.ElapsedMilliseconds < (playing ? getDelayMS() : 67)) {
				Thread.Sleep(1);
			}
			timer.Restart(); //Set elapsed time to 0.

			lock (inputLock) {
				if (playing) {
					//Safetly dispose of the previous frame from memory. Since we always pass copies we can do this.
					if (frameBuffer != null) {
						frameBuffer.Dispose();
						frameBuffer = null;
					}

					//If a new frame is available, store it in memory for future use.
					if (IsFrameAvailable()) {
						frameBuffer = readFrame();
					}
				}

				//If there is a frame stored in memory, return a copy of it.
				if (frameBuffer != null) {
					return frameBuffer.Clone();
				}

				//We couldn't find an input image, so return null.
				return null;
			}
		}

		///<summary>
		/// <returns> The MS that should be waited until the next frame is returned 
		/// <para>For images, this should return 67 (for 15fps, to keep computation low).</para>
		/// <para>Cameras should also return 0, since they will auto-block when reading</para>
		/// <para>Videos return the value saved. (15fps = 67ms, 30fps = 33ms, 60fps = 17ms)</para>
		/// </returns>
		/// </summary>
		protected abstract int getDelayMS();

		///<summary>
		/// <returns>If there is a frame available to be read. 
		/// </returns>
		/// </summary>
		public bool IsFrameAvailable() {
			//if (timer.ElapsedMilliseconds < getDelayMS()) return false;
			return isNextFrameAvailable();
		}

		///<summary>
		/// <returns>If there is a frame available to be read. 
		/// <para>For images always returns true.</para>
		/// <para>Videos return false at the end of the stream.</para>
		/// <para>Cameras return true if they are open.</para>
		/// </returns>
		/// </summary>
		protected abstract bool isNextFrameAvailable();

		///<summary>
		///<para>Reads the raw bytes of the last loaded frame.</para>
		///</summary>
		///<returns>Bytes in [y,x, channel] format.</returns>
		public byte[,,] ReadRawData() {
			lock (inputLock) {
				if (frameBuffer == null) return null;
				return frameBuffer.Clone().Data;
			}
		}

		///<summary>
		///<para>Reads the next frame of the current input, if available.</para>
		///<para>Note, that if a camera, this method blocks until the camera returns a new frame, which may differ between cameras.</para>
		///<para>Besides cameras, this method should NOT block.</para>
		///</summary>
		///<returns>The next frame, or null if no frame was available.</returns>
		protected abstract Image<Bgr, byte> readFrame();

		public abstract int GetWidth();
		public abstract int GetHeight();

		/// <summary>
		/// <para>Resumes the input of previously paused. If wasn't paused, nothing happens.</para>
		/// </summary>
		public void Play() {
			lock (inputLock) {
				playing = true;
			}
		}

		/// <summary>
		/// <para>Pauses the input. If getFrame is called when paused, the previous frame is returned instead. Runs at 15fps when paused to avoid over computation.</para>
		/// </summary>
		public void Pause() {
			lock (inputLock) {
				playing = false;
			}
		}

		public bool IsPlaying() {
			lock (inputLock) {
				return playing;
			}
		}

		public void UserPromptSaveScreenshot() {
			lock (screenshotLock) {
				Image<Bgr, byte> screenshot;
				lock (inputLock) {
					if (frameBuffer == null) return;
					screenshot = frameBuffer.Copy();
				}
				if (screenshot == null) return;
				if (saveDialog.ShowDialog() == DialogResult.OK) {
					string filename = saveDialog.FileName;
					ImageFormat format = null;
					

					switch (System.IO.Path.GetExtension(filename)) {
						case ".bmp": format = ImageFormat.Bmp; break;
						case ".emf": format = ImageFormat.Emf; break;
						case ".exif": format = ImageFormat.Exif; break;
						case ".gif": format = ImageFormat.Gif; break;
						case ".ico": format = ImageFormat.Icon; break;
						case ".jpeg": format = ImageFormat.Jpeg; break;
						case ".png": format = ImageFormat.Png; break;
						case ".tiff": format = ImageFormat.Tiff; break;
						case ".wmf": format = ImageFormat.Wmf; break;
					}

					if(format != null) {
						screenshot.Bitmap.Save(filename, format);
					}
				}
			}
		}

		protected void printDebugMsg(String msg) {
			Console.WriteLine("InputHandler: " + msg);
		}
	}
}
