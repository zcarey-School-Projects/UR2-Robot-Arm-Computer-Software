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

namespace RobotHelpers.InputHandling {

	public abstract class InputHandler {

		protected static OpenFileDialog dialog;

		private Stopwatch timer;
		protected Image<Bgr, byte> frameBuffer;
		private bool playing = true;

		public InputHandler() {
			timer = new Stopwatch();
			dialog = new OpenFileDialog();
			dialog.RestoreDirectory = true;
			timer.Start();
		}

		~InputHandler() {
			Dispose();
		}

		public abstract void Dispose();

		/// <summary>
		/// Returns the path to the current working directory, i.e. Files in your solution.
		/// </summary>
		/// <returns></returns>
		public String getWorkingDirectory() {
			return System.IO.Directory.GetCurrentDirectory() + "\\";
		}

		/// <summary>
		/// <para>Waits the required amount of time then returns the next loadded frame.</para>
		/// <para>If a video for image file is loaded, the method will block to match 30fps.</para>
		/// </summary>
		/// <returns>null if there was no frame to read.</returns>
		public Image<Bgr, byte> getFrame() {
			//Block read until specified delay.
			while(timer.ElapsedMilliseconds < (playing ? getDelayMS() : 67)) {
				Thread.Sleep(1);
			}
			timer.Restart(); //Set elapsed time to 0.

			if (playing) {
				if (isFrameAvailable()) {
					frameBuffer = readFrame();
				} else {
					frameBuffer = null;
				}
			}
			
			if(frameBuffer != null) {
				return frameBuffer.Clone();
			}
			
			return null;
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
		public bool isFrameAvailable() {
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
		public byte[,,] readRawData() {
			if (frameBuffer == null) return null;
			return frameBuffer.Clone().Data;
		}

		///<summary>
		///<para>Reads the next frame of the current input, if available.</para>
		///<para>Note, that if a camera, this method blocks until the camera returns a new frame, which may differ between cameras.</para>
		///<para>Besides cameras, this method should NOT block.</para>
		///</summary>
		///<returns>The next frame, or null if no frame was available.</returns>
		protected abstract Image<Bgr, byte> readFrame();

		public abstract int getWidth();
		public abstract int getHeight();

		///<summary>
		///<para>Opens a dialog for the user to select an input file to load.</para>
		///</summary>
		///<returns>Input loaded.</returns>
		public abstract bool requestLoadInput();

		///<summary>
		///<para>Sets the input to a local file.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		public abstract bool setFile(String filename);

		/// <summary>
		/// <para>Resumes the input of previously paused. If wasn't paused, nothing happens.</para>
		/// </summary>
		public void play() {
			playing = true;
		}

		/// <summary>
		/// <para>Pauses the input. If getFrame is called when paused, the previous frame is returned instead. Runs at 15fps when paused to avoid over computation.</para>
		/// </summary>
		public void pause() {
			playing = false;
		}

		public bool isPlaying() {
			return playing;
		}

		protected void printDebugMsg(String msg) {
			Console.WriteLine("InputHandler: " + msg);
		}
	}
}
