using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	class StackingProgram {

		private Robot robot;
		private Vision vision;

		private List<Triangle2DF> triangles;
		private List<RotatedRect> boxes;

		public StackingProgram(Robot robot, Vision vision) {
			this.robot = robot;
			this.vision = vision;
		}

		public void runProgram() {
			robot.moveToAndWait(5, 0);
			while (true) {
				vision.getShapeLists(out triangles, out boxes);
				if (triangles.Count > 0) {
					moveToPoint(triangles[0].Centeroid);
					pickUpShape(true);
					moveToTriangleStack();
					pickUpShape(false);
				} else if (boxes.Count > 0) {
					moveToPoint(boxes[0].Center);
					pickUpShape(true);
					moveToSquareStack();
					pickUpShape(false);
				} else {
					break;
				}
			}
		}

		private void moveToPoint(PointF shape) {
			//Calculate angle/distance from screen point
			//Move arm to point and wait
		}

		private void pickUpShape(bool magnetOn) {
			robot.lowerServo();
			//Turn on/off magnet
			robot.raiseServo();
		}

		private void moveToTriangleStack() {

		}

		private void moveToSquareStack() {

		}

	}
}
