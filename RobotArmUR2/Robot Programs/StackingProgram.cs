using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;
using System.Collections.Generic;

namespace RobotArmUR2.Robot_Programs {
	public class StackingProgram : RobotProgram {

		private Robot robot;
		private Vision vision;
		private PaperCalibrater paper;

		private List<Triangle2DF> triangles;
		private List<RotatedRect> squares;

		public StackingProgram(Robot robot, Vision vision, PaperCalibrater paper) : base(robot){
			this.robot = robot;
			this.vision = vision;
			this.paper = paper;
		}

		public override void Initialize(RobotInterface serial) {
			serial.MoveToAndWait(5, 0);
		}

		public override bool ProgramStep(RobotInterface serial) {
			//vision.getShapeLists(out triangles, out boxes);
			triangles = vision.Triangles;
			squares = vision.Squares;
			if (triangles.Count > 0) {
				base.moveToPoint(serial, triangles[0].Centeroid);
				pickUpShape(serial, true);
				base.moveToTriangleStack(serial);
				pickUpShape(serial, false);
			} else if (squares.Count > 0) {
				moveToPoint(serial, squares[0].Center); //TODO need to rename function to something more fitting
				pickUpShape(serial, true);
				base.moveToSquareStack(serial);
				pickUpShape(serial, false);
			} else {
				return false;
			}

			return true;
		}

		private void pickUpShape(RobotInterface serial, bool magnetOn) {
			serial.LowerServo();
			serial.SetMagnetState(magnetOn);
			serial.RaiseServo();
		}

		public override void ProgramCancelled(RobotInterface serial) {
			serial.LowerServo();
			serial.MagnetOff();
			serial.RaiseServo();
		}

	}
}
