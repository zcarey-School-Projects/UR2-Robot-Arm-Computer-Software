using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace RobotHelpers.InputHandling {
	public class CameraInput : InputHandler{

		private VideoCapture captureDevice;

		public CameraInput(int cameraId) : base() {
			captureDevice = new VideoCapture(cameraId);
		}

		public override void Dispose() {
			if (captureDevice == null) return;
			captureDevice.Stop();
			captureDevice.Dispose();
			captureDevice = null;
		}

		protected override int getDelayMS() {
			return 0;
		}

		protected override bool isNextFrameAvailable() {
			if (captureDevice == null) return false;
			return captureDevice.IsOpened;
		}

		protected override Image<Bgr, byte> readFrame() {
			if (isNextFrameAvailable()) {
				return captureDevice.QueryFrame().ToImage<Bgr, byte>();
			}

			return null;
		}

		public override int getWidth() {
			if (captureDevice == null) return 0;
			return captureDevice.Width;
		}

		public override int getHeight() {
			if (captureDevice == null) return 0;
			return captureDevice.Height;
		}

		public override bool requestLoadInput() {
			throw new NotImplementedException("Can't open a file for a camera input."); 
		}

		public override bool setFile(string filename) {
			throw new NotImplementedException("Can't open a file for a camera input.");
		}

	}
}
