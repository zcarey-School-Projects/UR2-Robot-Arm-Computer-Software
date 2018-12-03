using RobotArmUR2.VisionProcessing;
using System;
using System.Drawing;

namespace RobotArmUR2 {
	public abstract class RobotProgram {

		protected Robot Robot;

		public RobotProgram(Robot robot) {
			this.Robot = robot;
		}

		//Runs once at start of the program.
		public abstract void Initialize(RobotInterface serial);

		//Return true to keep running, false will stop it.
		public abstract bool ProgramStep(RobotInterface serial);

		//Called if the program is forcfully cancelled.
		public abstract void ProgramCancelled(RobotInterface serial);

		public static RobotPoint CalculateRobotCoordinates(RobotCalibration calib, PaperPoint relativePaperCoords) { //TODO simplify with points?
			double x1 = calib.BottomLeft.Extension * Math.Cos((180 - calib.BottomLeft.Rotation) * Math.PI / 180);
			double x2 = calib.TopLeft.Extension * Math.Cos((180 - calib.TopLeft.Rotation) * Math.PI / 180);
			double x3 = calib.TopRight.Extension * Math.Cos((180 - calib.TopRight.Rotation) * Math.PI / 180);
			double x4 = calib.BottomRight.Extension * Math.Cos((180 - calib.BottomRight.Rotation) * Math.PI / 180);

			double y1 = calib.BottomLeft.Extension * Math.Sin((180 - calib.BottomLeft.Rotation) * Math.PI / 180);
			double y2 = calib.TopLeft.Extension * Math.Sin((180 - calib.TopLeft.Rotation) * Math.PI / 180);
			double y3 = calib.TopRight.Extension * Math.Sin((180 - calib.TopRight.Rotation) * Math.PI / 180);
			double y4 = calib.BottomRight.Extension * Math.Sin((180 - calib.BottomRight.Rotation) * Math.PI / 180);

			double alpha = relativePaperCoords.X;
			double beta = relativePaperCoords.Y;

			double x = alpha * (x3 - x2) + beta * (x1 - x2) + alpha * beta * (x4 + x2 - x1 - x3) + x2;
			double y = alpha * (y3 - y2) + beta * (y1 - y2) + alpha * beta * (y4 + y2 - y1 - y3) + y2;

			double targetAngle = 180 - (Math.Atan2(y, x) * 180 / Math.PI);
			double targetDistance = Math.Sqrt(x * x + y * y);

			return new RobotPoint((float)targetAngle, (float)targetDistance);
		}

		protected void moveRobotToPaperPoint(RobotInterface serial, PaperPoint relativePaperCoords) {
			RobotPoint targetCoords = CalculateRobotCoordinates(ApplicationSettings.RobotCalibration, relativePaperCoords);
			Console.WriteLine("Target: [{0}°, {1}mm]\n", targetCoords.Rotation, targetCoords.Extension);

			serial.MoveToWait(targetCoords);
		}
		//TODO whenever a command fails, we need to cancel the program.
		protected void moveToTriangleStack(RobotInterface serial) {
			serial.MoveToWait(ApplicationSettings.RobotCalibration.TriangleStack);
		}

		protected void moveToSquareStack(RobotInterface serial) {
			serial.MoveToWait(ApplicationSettings.RobotCalibration.SquareStack);
		}

	}
}
