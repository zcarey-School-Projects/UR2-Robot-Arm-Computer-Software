using RobotArmUR2.Util;
using RobotArmUR2.Util.Calibration.Robot;
using System;

namespace RobotArmUR2.RobotControl {

	/// <summary>Used to run "programs", or sequences of moves for the robot to perform autonomously.</summary>
	public abstract class RobotProgram {

		public RobotProgram() {
			
		}

		/// <summary>Runs once at start of the program.</summary>
		/// <param name="serial"></param>
		/// <returns>true to continue running, false to cancel the program.</returns>
		public abstract bool Initialize(RobotInterface serial);

		/// <summary>A step of the program. Is called infinitely until false is returned. Between each step is checked for cancellation request,
		/// so keep as brief as possible.</summary>
		/// <param name="serial"></param>
		/// <returns>true to keep running, false to end the program</returns>
		public abstract bool ProgramStep(RobotInterface serial);

		/// <summary>Function is called if the program was forcefully cancelled.</summary>
		/// <param name="serial"></param>
		public abstract void ProgramCancelled(RobotInterface serial);

		private static double ToRad(double degrees) { return degrees * Math.PI / 180.0; }
		private static double ToDegree(double radians) { return radians * 180.0 / Math.PI; }

		/// <summary>Given the calibration and paper point, calculate the position the robot needs to move to.</summary>
		/// <param name="calib"></param>
		/// <param name="relativePaperCoords"></param>
		/// <returns></returns>
		public static RobotPoint CalculateRobotCoordinates(RobotCalibration calib, PaperPoint relativePaperCoords) {
			double x1 = calib.BottomLeft.Extension * Math.Cos(ToRad(180 - calib.BottomLeft.Rotation));
			double x2 = calib.TopLeft.Extension * Math.Cos(ToRad(180 - calib.TopLeft.Rotation));
			double x3 = calib.TopRight.Extension * Math.Cos(ToRad(180 - calib.TopRight.Rotation));
			double x4 = calib.BottomRight.Extension * Math.Cos(ToRad(180 - calib.BottomRight.Rotation));
			
			double y1 = calib.BottomLeft.Extension * Math.Sin(ToRad(180 - calib.BottomLeft.Rotation));
			double y2 = calib.TopLeft.Extension * Math.Sin(ToRad(180 - calib.TopLeft.Rotation));
			double y3 = calib.TopRight.Extension * Math.Sin(ToRad(180 - calib.TopRight.Rotation));
			double y4 = calib.BottomRight.Extension * Math.Sin(ToRad(180 - calib.BottomRight.Rotation));

			double alpha = relativePaperCoords.X;
			double beta = relativePaperCoords.Y;

			double x = alpha * (x3 - x2) + beta * (x1 - x2) + alpha * beta * (x4 + x2 - x1 - x3) + x2;
			double y = alpha * (y3 - y2) + beta * (y1 - y2) + alpha * beta * (y4 + y2 - y1 - y3) + y2;

			double targetAngle = ToDegree(Math.PI - Math.Atan2(y, x));
			double targetDistance = Math.Sqrt(x * x + y * y);

			return new RobotPoint((float)targetAngle, (float)targetDistance);
		}

		/// <summary>Given an interface and paper point, moves the robot to given point on paper. Blocks until move is finished.</summary>
		/// <param name="serial"></param>
		/// <param name="relativePaperCoords"></param>
		/// <returns></returns>
		protected bool moveRobotToPaperPoint(RobotInterface serial, PaperPoint relativePaperCoords) {
			RobotPoint targetCoords = CalculateRobotCoordinates(ApplicationSettings.RobotCalibration, relativePaperCoords);
			Console.WriteLine("Target: [{0}°, {1}mm]\n", targetCoords.Rotation, targetCoords.Extension);

			return serial.MoveToWait(targetCoords);
		}
		
		/// <summary>Moves the robot to the triangle stack. Blocks until finished.</summary>
		/// <param name="serial"></param>
		/// <returns></returns>
		protected bool moveToTriangleStack(RobotInterface serial) {
			return serial.MoveToWait(ApplicationSettings.RobotCalibration.TriangleStack);
		}

		/// <summary>Moves the robot to the square stack. Blocks until finished.</summary>
		/// <param name="serial"></param>
		/// <returns></returns>
		protected bool moveToSquareStack(RobotInterface serial) {
			return serial.MoveToWait(ApplicationSettings.RobotCalibration.SquareStack);
		}

	}
}
