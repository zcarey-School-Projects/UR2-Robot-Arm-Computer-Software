using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {
	public interface IVisionUI {

		void VisionUI_SetFPSCounter(float fps);
		void VisionUI_SetNativeResolutionText(Size resolution);
		//void VisionUI_DisplayImage<TColor, TDepth>(Image<TColor, TDepth> image, PictureId pictureId) where TColor : struct, IColor where TDepth : new();
		void VisionUI_NewFrameFinished(Vision vision);
	}

	
	public class VisionUIInvoker {

		public IVisionUI Listener { get; set; }

		public void SetFPSCounter(float fps) { if (Listener != null) Listener.VisionUI_SetFPSCounter(fps); }
		public void SetNativeResolutionText(Size resolution) { if (Listener != null) Listener.VisionUI_SetNativeResolutionText(resolution); }
		public void NewFrameFinished(Vision vision) { if (Listener != null) Listener.VisionUI_NewFrameFinished(vision); }

	}
	
	/*
	public enum PictureId {
		//If you add a picturebox, don't forget to update "getPictureBoxFromId" method!
		Original,
		Gray,
		Canny
	}
	*/
	/*
	class VisionUIListener {

		private IVisionUI listener;

		public VisionUIListener() { }
		public VisionUIListener(IVisionUI listener) {
			this.listener = listener;
		}

		public void SetListener(IVisionUI newListener) { listener = newListener; }

		public void Invoke_SetFPSCounter(float fps) { if (listener != null) listener.VisionUI_SetFPSCounter(fps); }
		public void Invoke_SetNativeResolutionText(Size resolution) { if (listener != null) listener.VisionUI_SetNativeResolutionText(resolution); }
		public void Invoke_DisplayImage<TColor, TDepth>(Image<TColor, TDepth> image, PictureId pictureId) where TColor : struct, IColor where TDepth : new() { if (listener != null) listener.VisionUI_DisplayImage(image, pictureId); }

	}
	*/
}
