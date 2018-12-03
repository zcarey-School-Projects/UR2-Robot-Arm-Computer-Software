using RobotArmUR2.VisionProcessing;
using System;
using System.Drawing;

namespace RobotArmUR2 {
	public abstract class RobotProgram {

		public RobotProgram() {
			
		}

		//Runs once at start of the program.
		public abstract bool Initialize(RobotInterface serial);

		//Return true to keep running, false will stop it.
		public abstract bool ProgramStep(RobotInterface serial);

		//Called if the program is forcfully cancelled.
		public abstract void ProgramCancelled(RobotInterface serial);

		private static double ToRad(double degrees) { return degrees * Math.PI / 180.0; }
		private static double ToDegree(double radians) { return radians * 180.0 / Math.PI; }

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

		protected bool moveRobotToPaperPoint(RobotInterface serial, PaperPoint relativePaperCoords) {
			RobotPoint targetCoords = CalculateRobotCoordinates(ApplicationSettings.RobotCalibration, relativePaperCoords);
			Console.WriteLine("Target: [{0}°, {1}mm]\n", targetCoords.Rotation, targetCoords.Extension);

			return serial.MoveToWait(targetCoords);
		}
		
		protected bool moveToTriangleStack(RobotInterface serial) {
			return serial.MoveToWait(ApplicationSettings.RobotCalibration.TriangleStack);
		}

		protected bool moveToSquareStack(RobotInterface serial) {
			return serial.MoveToWait(ApplicationSettings.RobotCalibration.SquareStack);
		}

	}
}
