using RobotArmUR2.Robot_Programs;
using System;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public partial class RobotCalibrater : Form {

		private Robot robot;

		public RobotCalibrater(Robot robot) {
			this.robot = robot;
			InitializeComponent();
		}

		private void RobotCalibrater_Load(object sender, EventArgs e) {
			OnCalibrationChanged();
		}

		private void RobotCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			robot.Calibration.SaveSettings();
		}

		private void RobotCalibrater_KeyDown(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, true);
		}

		private void RobotCalibrater_KeyUp(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, false);
		}

		public void formKeyEvent(Keys key, bool pressed) {
			if ((key == Keys.A) || (key == Keys.Left)) {
				robot.ManualControlKeyEvent(Robot.Key.RotateCCW, pressed);
			} else if ((key == Keys.D) || (key == Keys.Right)) {
				robot.ManualControlKeyEvent(Robot.Key.RotateCW, pressed);
			} else if ((key == Keys.W) || (key == Keys.Up)) {
				robot.ManualControlKeyEvent(Robot.Key.ExtendOutward, pressed);
			} else if ((key == Keys.S) || (key == Keys.Down)) {
				robot.ManualControlKeyEvent(Robot.Key.ExtendInward, pressed);
			}else if((key == Keys.E)) {
				robot.ManualControlKeyEvent(Robot.Key.RaiseServo, pressed);
			}else if(key == Keys.Q) {
				robot.ManualControlKeyEvent(Robot.Key.LowerServo, pressed);
			}else if(key == Keys.M) {
				robot.ManualControlKeyEvent(Robot.Key.MagnetOn, pressed);
			}else if(key == Keys.N) {
				robot.ManualControlKeyEvent(Robot.Key.MagnetOff, pressed);
			}
		}

		private void BLMoveTo_Click(object sender, EventArgs e) {
			//TODO make a new "RobotPosition" class in Calibration that consists of the angle, and distance.
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.Angle1, robot.Calibration.Distance1));
		}

		private void TLMoveTo_Click(object sender, EventArgs e) {
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.Angle2, robot.Calibration.Distance2));
		}

		private void TRMoveTo_Click(object sender, EventArgs e) {
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.Angle3, robot.Calibration.Distance3));
		}

		private void BRMoveTo_Click(object sender, EventArgs e) {
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.Angle4, robot.Calibration.Distance4));
		}

		public void OnCalibrationChanged() {
			updateLabel(BLLabel, robot.Calibration.Angle1, robot.Calibration.Distance1);
			updateLabel(TLLabel, robot.Calibration.Angle2, robot.Calibration.Distance2);
			updateLabel(TRLabel, robot.Calibration.Angle3, robot.Calibration.Distance3);
			updateLabel(BRLabel, robot.Calibration.Angle4, robot.Calibration.Distance4);
			updateLabel(TriangleLabel, robot.Calibration.TriangleStackAngle, robot.Calibration.TriangleStackDistance);
			updateLabel(SquareLabel, robot.Calibration.SquareStackAngle, robot.Calibration.SquareStackDistance);
		}

		private void updateLabel(Label label, float angle, float distance) {
			BeginInvoke(new Action(() => {
				label.Text = "(" + angle.ToString("N2") + (char)248 + ", " + distance.ToString("N2") + "mm)";
			}));
		}

		private void calibrateClicked(int pointNumber) {
			robot.RunProgram(new CalibrationProgram(robot, this, pointNumber));
		}

		private void BLCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(1);
		}

		private void BRCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(4);
		}

		private void TLCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(2);
		}

		private void TRCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(3);
		}

		private static bool confirmReset() {
			return MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes;
		}

		private void resetPoint1() {
			robot.Calibration.Angle1 = RobotCalibration.Angle1Default;
			robot.Calibration.Distance1 = RobotCalibration.Distance1Default;
		}

		private void resetPoint2() {
			robot.Calibration.Angle2 = RobotCalibration.Angle2Default;
			robot.Calibration.Distance2 = RobotCalibration.Distance2Default;
		}

		private void resetPoint3() {
			robot.Calibration.Angle3 = RobotCalibration.Angle3Default;
			robot.Calibration.Distance3 = RobotCalibration.Distance3Default;
		}

		private void resetPoint4() {
			robot.Calibration.Angle4 = RobotCalibration.Angle4Default;
			robot.Calibration.Distance4 = RobotCalibration.Distance4Default;
		}

		private void resetTriangle() {
			robot.Calibration.TriangleStackAngle = RobotCalibration.TriangleStackAngleDefault;
			robot.Calibration.TriangleStackDistance = RobotCalibration.TriangleStackDistanceDefault;
		}

		private void resetSquare() {
			robot.Calibration.SquareStackAngle = RobotCalibration.SquareStackAngleDefault;
			robot.Calibration.SquareStackDistance = RobotCalibration.SquareStackDistanceDefault;
		}

		private void ResetBL_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint1();
				OnCalibrationChanged();
			}
		}

		private void ResetBR_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint4();
				OnCalibrationChanged();
			}
		}

		private void ResetTL_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint2();
				OnCalibrationChanged();
			}
		}

		private void ResetTR_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint3();
				OnCalibrationChanged();
			}
		}

		private void ResetAll_Click(object sender, EventArgs e) {
			if(confirmReset()) {
				resetPoint1();
				resetPoint2();
				resetPoint3();
				resetPoint4();
				resetTriangle();
				resetSquare();
				OnCalibrationChanged();
			}
		}

		private void TriangleMoveTo_Click(object sender, EventArgs e) {
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.TriangleStackAngle, robot.Calibration.TriangleStackDistance));
		}

		private void SquareMoveTo_Click(object sender, EventArgs e) {
			robot.RunProgram(new MoveToPosProgram(robot, robot.Calibration.SquareStackAngle, robot.Calibration.SquareStackDistance));
		}

		private void TriangleCalibrate_Click(object sender, EventArgs e) {
			robot.RunProgram(new CalibrationProgram(robot, this, 5));
		}

		private void SquareCalibrate_Click(object sender, EventArgs e) {
			robot.RunProgram(new CalibrationProgram(robot, this, 6));
		}

		private void ResetTriangle_Click(object sender, EventArgs e) {
			resetTriangle();
		}

		private void ResetSquare_Click(object sender, EventArgs e) {
			resetSquare();
		}
	}
}
