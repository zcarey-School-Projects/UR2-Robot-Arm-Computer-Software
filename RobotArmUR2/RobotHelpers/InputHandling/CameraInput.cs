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

		protected override void onDispose() {
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
			//try {
				if (isNextFrameAvailable()) {
					Mat rawFormat = captureDevice.QueryFrame();
					if (rawFormat != null) {
						return rawFormat.ToImage<Bgr, byte>();
					}
				}
			//} catch {
			//	Dispose();
			//}

			return null;
		}

		public override int GetWidth() {
			if (captureDevice == null) return 0;
			return captureDevice.Width;
		}

		public override int GetHeight() {
			if (captureDevice == null) return 0;
			return captureDevice.Height;
		}

	}
}
