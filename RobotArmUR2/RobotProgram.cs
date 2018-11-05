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
			float percentA1 = (1f - relativePaperCoords.X) * Robot.Calibration.Angle2 + relativePaperCoords.X * Robot.Calibration.Angle3;
			float percentA2 = (1f - relativePaperCoords.X) * Robot.Calibration.Angle1 + relativePaperCoords.X * Robot.Calibration.Angle4;
			float targetAngle = (1f - relativePaperCoords.Y) * percentA1 + relativePaperCoords.Y * percentA2;

			float percentD1 = (1f - relativePaperCoords.X) * Robot.Calibration.Distance2 + relativePaperCoords.X * Robot.Calibration.Distance3;
			float percentD2 = (1f - relativePaperCoords.X) * Robot.Calibration.Distance1 + relativePaperCoords.X * Robot.Calibration.Distance4;
			float targetDistance = (1f - relativePaperCoords.Y) * percentD1 + relativePaperCoords.Y * percentD2;

			serial.MoveToAndWait(targetAngle, targetDistance);
		}

		protected void moveToTriangleStack(RobotInterface serial) {
			serial.MoveToAndWait(Robot.Calibration.TriangleStackAngle, Robot.Calibration.TriangleStackDistance);
		}

		protected void moveToSquareStack(RobotInterface serial) {
			serial.MoveToAndWait(Robot.Calibration.SquareStackAngle, Robot.Calibration.SquareStackDistance);
		}

	}
}
