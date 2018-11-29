using Emgu.CV;
using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace RobotArmUR2.Robot_Programs {
	public class StackingProgram : RobotProgram {

		private Robot robot;
		private Vision vision;
		private PaperCalibrater paper;

		private int emptyFrameCount = 0;
		private const int EmptyFramesNeeded = 20;

		public StackingProgram(Robot robot, Vision vision, PaperCalibrater paper) : base(robot){
			this.robot = robot;
			this.vision = vision;
			this.paper = paper;
		}

		public override void Initialize(RobotInterface serial) {
			//serial.MoveToAndWait(5, 0);
			serial.ReturnHome();
			serial.RaiseServo();
			serial.PowerMagnetOff();
			Thread.Sleep(1000);
		}
		
		public override bool ProgramStep(RobotInterface serial) {
			//vision.getShapeLists(out triangles, out boxes);
			DetectedShapes shapes = vision.DetectedShapes; //TODO add "GetPaperPoints"
			if (shapes.RelativeTrianglePoints.Count > 0) {
				PaperPoint center = shapes.RelativeTrianglePoints[0];
				base.moveToPoint(serial, center);
				pickUpShape(serial, true);
				base.moveToTriangleStack(serial);
				pickUpShape(serial, false);
				emptyFrameCount = 0;
			} else if (shapes.RelativeSquarePoints.Count > 0) {
				PaperPoint center = shapes.RelativeSquarePoints[0];
				moveToPoint(serial, center); //TODO need to rename function to something more fitting
				pickUpShape(serial, true);
				base.moveToSquareStack(serial);
				pickUpShape(serial, false);
				emptyFrameCount = 0;
			} else {
				emptyFrameCount++;
				if (emptyFrameCount >= EmptyFramesNeeded) {
					return false;
				}
			}

			return true;
		}

		private void pickUpShape(RobotInterface serial, bool magnetOn) {
			Thread.Sleep(1000);
			serial.LowerServo();
			Thread.Sleep(1000);
			serial.PowerMagnet(magnetOn);
			Thread.Sleep(1000);
			serial.RaiseServo();
			Thread.Sleep(1000);
		}

		public override void ProgramCancelled(RobotInterface serial) {
			pickUpShape(serial, false);
		}

	}
}
