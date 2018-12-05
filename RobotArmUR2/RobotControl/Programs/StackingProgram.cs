using RobotArmUR2.Util;
using RobotArmUR2.VisionProcessing;
using System.Threading;

namespace RobotArmUR2.RobotControl.Programs {
	public class StackingProgram : RobotProgram {

		private Vision vision;

		private int emptyFrameCount = 0;
		private const int EmptyFramesNeeded = 20;

		public StackingProgram(Vision vision) {
			this.vision = vision;
		}

		public override bool Initialize(RobotInterface serial) {
			if (!serial.RaiseServo()) return false;
			if (!serial.PowerMagnetOff()) return false;
			if (!serial.ReturnHome()) return false;
			Thread.Sleep(1000);
			return true;
		}
		
		public override bool ProgramStep(RobotInterface serial) {
			VisionImages images = vision.Images;
			if (images == null || images.Shapes == null) return false;
			DetectedShapes shapes = images.Shapes; 
			if (shapes.RelativeTrianglePoints.Count > 0) {
				PaperPoint center = shapes.RelativeTrianglePoints[0];
				if (!stackShape(serial, center)) return false;
			} else if (shapes.RelativeSquarePoints.Count > 0) {
				PaperPoint center = shapes.RelativeSquarePoints[0];
				if (!stackShape(serial, center)) return false;
			} else {
				emptyFrameCount++;
				if (emptyFrameCount >= EmptyFramesNeeded) {
					return false;
				}
			}

			return true;
		}

		private bool stackShape(RobotInterface serial, PaperPoint center) {
			if (!moveRobotToPaperPoint(serial, center)) return false;
			if (!pickUpShape(serial, true)) return false;
			if (!moveToTriangleStack(serial)) return false;
			if (!pickUpShape(serial, false)) return false;
			emptyFrameCount = 0;
			return true;
		}

		private bool pickUpShape(RobotInterface serial, bool magnetOn) {
			Thread.Sleep(1000);
			if (!serial.LowerServo()) return false;
			Thread.Sleep(1000);
			if(!serial.PowerMagnet(magnetOn)) return false;
			Thread.Sleep(1000);
			if (!serial.RaiseServo()) return false;
			Thread.Sleep(1000);

			return true;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			pickUpShape(serial, false);
		}

	}
}
