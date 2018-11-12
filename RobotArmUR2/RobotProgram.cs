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

		public static PointF CalculateRobotCoordinates(RobotCalibration calib, PointF relativePaperCoords) {
			double x1 = calib.Distance1 * Math.Cos((180 - calib.Angle1) * Math.PI / 180);
			double x2 = calib.Distance2 * Math.Cos((180 - calib.Angle2) * Math.PI / 180);
			double x3 = calib.Distance3 * Math.Cos((180 - calib.Angle3) * Math.PI / 180);
			double x4 = calib.Distance4 * Math.Cos((180 - calib.Angle4) * Math.PI / 180);

			double y1 = calib.Distance1 * Math.Sin((180 - calib.Angle1) * Math.PI / 180);
			double y2 = calib.Distance2 * Math.Sin((180 - calib.Angle2) * Math.PI / 180);
			double y3 = calib.Distance3 * Math.Sin((180 - calib.Angle3) * Math.PI / 180);
			double y4 = calib.Distance4 * Math.Sin((180 - calib.Angle4) * Math.PI / 180);

			double alpha = relativePaperCoords.X;
			double beta = relativePaperCoords.Y;

			double x = alpha * (x3 - x2) + beta * (x1 - x2) + alpha * beta * (x4 + x2 - x1 - x3) + x2;
			double y = alpha * (y3 - y2) + beta * (y1 - y2) + alpha * beta * (y4 + y2 - y1 - y3) + y2;
			//Console.WriteLine("Relative: [{0}, {1}]", x, y); //TODO remove

			double targetAngle = 180 - (Math.Atan2(y, x) * 180 / Math.PI);
			double targetDistance = Math.Sqrt(x * x + y * y);

			return new PointF((float)targetAngle, (float)targetDistance);
		}

		protected void moveToPoint(RobotInterface serial, PointF relativePaperCoords) {
			PointF targetCoords = CalculateRobotCoordinates(Robot.Calibration, relativePaperCoords);
			Console.WriteLine("Target: [{0}°, {1}mm]\n", targetCoords.X, targetCoords.Y);

			//Console.WriteLine("[{0}, {1}]", targetAngle, targetDistance);
			serial.MoveToAndWait(targetCoords.X, targetCoords.Y);
		}
		//TODO whenever a command fails, we need to cancel the program.
		protected void moveToTriangleStack(RobotInterface serial) {
			serial.MoveToAndWait(Robot.Calibration.TriangleStackAngle, Robot.Calibration.TriangleStackDistance);
		}

		protected void moveToSquareStack(RobotInterface serial) {
			serial.MoveToAndWait(Robot.Calibration.SquareStackAngle, Robot.Calibration.SquareStackDistance);
		}

	}
}
