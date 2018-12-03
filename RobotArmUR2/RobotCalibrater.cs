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
			robot.ManualControlKeyEvent(e.KeyCode, true);
		}

		private void RobotCalibrater_KeyUp(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, false);
		}
		
		private void BLMoveTo_Click(object sender, EventArgs e) {
			robot.Interface.MoveTo(robot.Calibration.BottomLeft);
		}

		private void TLMoveTo_Click(object sender, EventArgs e) {
			robot.Interface.MoveTo(robot.Calibration.TopLeft);
		}

		private void TRMoveTo_Click(object sender, EventArgs e) {
			robot.Interface.MoveTo(robot.Calibration.TopRight);
		}

		private void BRMoveTo_Click(object sender, EventArgs e) {
			robot.Interface.MoveTo(robot.Calibration.BottomRight);
		}

		public void OnCalibrationChanged() { //TODO what 7 things are calling this?
			updateLabel(BLLabel, robot.Calibration.BottomLeft);
			updateLabel(TLLabel, robot.Calibration.TopLeft);
			updateLabel(TRLabel, robot.Calibration.TopRight);
			updateLabel(BRLabel, robot.Calibration.BottomRight);
			updateLabel(TriangleLabel, robot.Calibration.TriangleStack);
			updateLabel(SquareLabel, robot.Calibration.SquareStack);
		}

		private void updateLabel(Label label, RobotPoint pos) {
			BeginInvoke(new Action(() => {
				label.Text = "(" + pos.Rotation.ToString("N2") + (char)248 + ", " + pos.Extension.ToString("N2") + "mm)";
			}));
		}

		private void calibrateClicked(RobotCalibrationPoint pt) {
			if (pt == null) return;
			RobotPoint pos = robot.Interface.GetPosition();
			if (pos != null) {
				pt.Rotation = pos.Rotation;
				pt.Extension = pos.Extension; //TODO put inside class
				OnCalibrationChanged();
			} else {
				MessageBox.Show("Could not retrieve position.");
			}
			
		}

		private void BLCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.BottomLeft);
		}

		private void BRCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.BottomRight);
		}

		private void TLCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.TopLeft);
		}

		private void TRCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.TopRight);
		}

		private static bool confirmReset() {
			return MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes;
		}

		private void resetPoint1() {
			robot.Calibration.BottomLeft.ResetToDefault();
		}

		private void resetPoint2() {
			robot.Calibration.TopLeft.ResetToDefault();
		}

		private void resetPoint3() {
			robot.Calibration.TopRight.ResetToDefault();
		}

		private void resetPoint4() {
			robot.Calibration.BottomRight.ResetToDefault();
		}

		private void resetTriangle() {
			robot.Calibration.TriangleStack.ResetToDefault();
		}

		private void resetSquare() {
			robot.Calibration.SquareStack.ResetToDefault();
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
			robot.Interface.MoveTo(robot.Calibration.TriangleStack);
		}

		private void SquareMoveTo_Click(object sender, EventArgs e) {
			robot.Interface.MoveTo(robot.Calibration.SquareStack);
		}

		private void TriangleCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.TriangleStack);
		}

		private void SquareCalibrate_Click(object sender, EventArgs e) {
			calibrateClicked(robot.Calibration.SquareStack);
		}

		private void ResetTriangle_Click(object sender, EventArgs e) {
			resetTriangle();
		}

		private void ResetSquare_Click(object sender, EventArgs e) {
			resetSquare();
		}
	}
}
