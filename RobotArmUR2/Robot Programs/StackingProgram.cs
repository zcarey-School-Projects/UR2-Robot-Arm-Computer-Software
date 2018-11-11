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

		private List<Triangle2DF> triangles;
		private List<RotatedRect> squares;

		public StackingProgram(Robot robot, Vision vision, PaperCalibrater paper) : base(robot){
			this.robot = robot;
			this.vision = vision;
			this.paper = paper;
		}

		public override void Initialize(RobotInterface serial) {
			//serial.MoveToAndWait(5, 0);
			serial.GoToHome();
			serial.RaiseServo();
			serial.MagnetOff();
			Thread.Sleep(1000);
		}

		public override bool ProgramStep(RobotInterface serial) {
			//vision.getShapeLists(out triangles, out boxes);
			triangles = vision.Triangles;
			squares = vision.Squares;
			Image<Gray, byte> temp = vision.WarpedImage;
			int width = temp.Width;
			int height = temp.Height;
			if (triangles.Count > 0) {
				PointF center = triangles[0].Centeroid;
				PointF pt = new PointF((float)center.X / width, (float)center.Y / height);
				base.moveToPoint(serial, pt);
				pickUpShape(serial, true);
				base.moveToTriangleStack(serial);
				pickUpShape(serial, false);
			} else if (squares.Count > 0) {
				PointF center = squares[0].Center;
				PointF pt = new PointF((float)center.X / width, (float)center.Y / height);
				moveToPoint(serial, pt); //TODO need to rename function to something more fitting
				pickUpShape(serial, true);
				base.moveToSquareStack(serial);
				pickUpShape(serial, false);
			} else {
				return false;
			}

			return true;
		}

		private void pickUpShape(RobotInterface serial, bool magnetOn) {
			Thread.Sleep(1000);
			serial.LowerServo();
			Thread.Sleep(1000);
			serial.SetMagnetState(magnetOn); 
			Thread.Sleep(1000);
			serial.RaiseServo();
			Thread.Sleep(1000);
		}

		public override void ProgramCancelled(RobotInterface serial) {
			pickUpShape(serial, false);
		}

	}
}
