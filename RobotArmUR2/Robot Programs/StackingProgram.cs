using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;
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
		private PaperCalibrater paper;

		private List<Triangle2DF> triangles;
		private List<RotatedRect> boxes;

		public StackingProgram(Robot robot, Vision vision, PaperCalibrater paper) {
			this.robot = robot;
			this.vision = vision;
			this.paper = paper;

			robot.moveToAndWait(5, 0);
		}

		public bool programStep() {
			//vision.getShapeLists(out triangles, out boxes);
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
				return false;
			}

			return true;
		}

		private void moveToPoint(PointF shape) {
			PointF paperCoords = calculatePaperCoords(paper, shape.X, shape.Y);
			float percentA1 = (1f - paperCoords.X) * robot.Angle2 + paperCoords.X * robot.Angle3;
			float percentA2 = (1f - paperCoords.X) * robot.Angle1 + paperCoords.X * robot.Angle4;
			float targetAngle = (1f - paperCoords.Y) * percentA1 + paperCoords.Y * percentA2;

			float percentD1 = (1f - paperCoords.X) * robot.Distance2 + paperCoords.X * robot.Distance3;
			float percentD2 = (1f - paperCoords.X) * robot.Distance1 + paperCoords.X * robot.Distance4;
			float targetDistance = (1f - paperCoords.Y) * percentD1 + paperCoords.Y * percentD2;

			robot.moveToAndWait(targetAngle, targetDistance);
		}

		private void pickUpShape(bool magnetOn) {
			robot.lowerServo();
			//Turn on/off magnet
			robot.raiseServo();
		}

		private void moveToTriangleStack() {
			robot.moveToAndWait(robot.TriangleStackAngle, robot.TriangleStackDistance);
		}

		private void moveToSquareStack() {
			robot.moveToAndWait(robot.SquareStackAngle, robot.SquareStackDistance);
		}

		private PointF calculatePaperCoords(PaperCalibrater paper, float x, float y) {
			//PointF[] paperPoints = paper.paperPoints.ToArray();
			//double p1x = /*paper.*/paperPoints[0].X;// * PaperPicture.Width;
			//double p1y = /*paper.*/paperPoints[0].Y;// * PaperPicture.Height;

			//double dx1 = /*paper.*/paperPoints[1].X - p1x;
			//double dx2 = /*paper.*/paperPoints[2].X - /*paper.*/paperPoints[3].X;
			//double dy1 = /*paper.*/paperPoints[3].Y - p1y;
			//double dy2 = /*paper.*/paperPoints[2].Y - /*paper.*/paperPoints[1].Y;

			//double D = dx2 - dx1;
			//double E = /*paper.*/paperPoints[3].X - p1x;
			//double F = dy2 - dy1;
			//double G = /*paper.*/paperPoints[1].Y - p1y;

			//double alpha = (-E * F) + (D * dy1);
			//double hat = (F * x) - (E * G) - (p1x * F) + (D * p1y) + (dx1 * dy1) - (y * D);
			//double e = (G * x) - (p1x * G) + (dx1 * p1y) - (y * dx1);
			//double b = (-hat + Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);
			//double b2 = (-hat - Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);

			//double a = (x - E * b - p1x) / (D * b + dx1);
			//double a2 = (x - E * b2 - H) / (D * b2 + dx1);

			//return new PointF((float)a, (float)b);
			return new PointF();
		}

	}
}
