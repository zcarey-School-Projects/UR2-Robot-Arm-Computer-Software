using System.Drawing;

namespace RobotArmUR2.VisionProcessing {
	public interface IVisionUI {

		void VisionUI_SetFPSCounter(float fps);
		void VisionUI_SetNativeResolutionText(Size resolution);
		void VisionUI_NewFrameFinished(Vision vision);

	}

	
	public class VisionUIInvoker {

		public IVisionUI Listener { get; set; }

		public void SetFPSCounter(float fps) { IVisionUI listener = Listener; if (listener != null) listener.VisionUI_SetFPSCounter(fps); }
		public void SetNativeResolutionText(Size resolution) { IVisionUI listener = Listener; if (listener != null) listener.VisionUI_SetNativeResolutionText(resolution); }
		public void NewFrameFinished(Vision vision) { IVisionUI listener = Listener; if (listener != null) listener.VisionUI_NewFrameFinished(vision); }

	}
}
