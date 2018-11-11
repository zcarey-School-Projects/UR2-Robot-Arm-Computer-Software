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

		protected void moveToPoint(RobotInterface serial, PointF relativePaperCoords) {
			/*
			float percentA1 = (1f - relativePaperCoords.X) * Robot.Calibration.Angle2 + relativePaperCoords.X * Robot.Calibration.Angle3;
			float percentA2 = (1f - relativePaperCoords.X) * Robot.Calibration.Angle1 + relativePaperCoords.X * Robot.Calibration.Angle4;
			float targetAngle = (1f - relativePaperCoords.Y) * percentA1 + relativePaperCoords.Y * percentA2;

			float percentD1 = (1f - relativePaperCoords.X) * Robot.Calibration.Distance2 + relativePaperCoords.X * Robot.Calibration.Distance3;
			float percentD2 = (1f - relativePaperCoords.X) * Robot.Calibration.Distance1 + relativePaperCoords.X * Robot.Calibration.Distance4;
			float targetDistance = (1f - relativePaperCoords.Y) * percentD1 + relativePaperCoords.Y * percentD2;
			*/
			RobotCalibration calib = Robot.Calibration;
			double x1 = calib.Distance1 * Math.Cos(calib.Angle1 * Math.PI / 180);
			double x2 = calib.Distance2 * Math.Cos(calib.Angle2 * Math.PI / 180);
			double x3 = calib.Distance3 * Math.Cos(calib.Angle3 * Math.PI / 180);
			double x4 = calib.Distance4 * Math.Cos(calib.Angle4 * Math.PI / 180);

			double y1 = calib.Distance1 * Math.Sin(calib.Angle1 * Math.PI / 180);
			double y2 = calib.Distance2 * Math.Sin(calib.Angle2 * Math.PI / 180);
			double y3 = calib.Distance3 * Math.Sin(calib.Angle3 * Math.PI / 180);
			double y4 = calib.Distance4 * Math.Sin(calib.Angle4 * Math.PI / 180);

			double alpha = relativePaperCoords.Y;
			double beta = relativePaperCoords.X;

			double x = alpha * beta * (x2 + x4 - x1 - x3) + alpha * (x1 - x2) + beta * (x3 - x2) + x2;
			double y = alpha * beta * (y2 + y4 - y1 - y3) + alpha * (y1 - y2) + beta * (y3 - y2) + y2;
			Console.WriteLine("Relative: [{0}, {1}]", x, y);

			double targetAngle = Math.Abs(Math.Atan(y / x)) * 180 / Math.PI;
			double targetDistance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
			Console.WriteLine("Target: [{0}, {1}mm]\n", targetAngle, targetDistance);

			//Console.WriteLine("[{0}, {1}]", targetAngle, targetDistance);
			serial.MoveToAndWait((float)targetAngle, (float)targetDistance);
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
