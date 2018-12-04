using System;
using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {
	public partial class RobotCalibrater : Form {

		private RobotControl.Robot robot;

		public RobotCalibrater(RobotControl.Robot robot) {
			this.robot = robot;
			InitializeComponent();
		}

		private void RobotCalibrater_Load(object sender, EventArgs e) {
			UpdateAllLabels();
		}

		private void RobotCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			//ApplicationSettings.RobotCalibration.SaveAllSettings();
		}

		#region Manual Control Key Events
		private void RobotCalibrater_KeyDown(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, true);
		}

		private void RobotCalibrater_KeyUp(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, false);
		}
		#endregion

		private void updateLabel(Label label, RobotPoint pos) {
			label.Text = "(" + pos.Rotation.ToString("N2") + (char)248 + ", " + pos.Extension.ToString("N2") + "mm";
		}

		public void UpdateAllLabels() {
			updateLabel(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft);
			updateLabel(TLLabel, ApplicationSettings.RobotCalibration.TopLeft);
			updateLabel(TRLabel, ApplicationSettings.RobotCalibration.TopRight);
			updateLabel(BRLabel, ApplicationSettings.RobotCalibration.BottomRight);
			updateLabel(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack);
			updateLabel(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack);
		}

		#region Move-to buttons
		private void BLMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.BottomLeft); }
		private void TLMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TopLeft); }
		private void TRMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TopRight); }
		private void BRMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.BottomRight); }
		private void TriangleMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TriangleStack); }
		private void SquareMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		private void calibrateClicked(Label label, RobotCalibrationPoint pt) {
			if (pt == null) return;
			RobotPoint pos = robot.Interface.GetPosition();
			if (pos != null) {
				pt.SetPoint(pos);
				updateLabel(label, pt);
			} else {
				MessageBox.Show("Could not retrieve position.");
			}
			
		}

		#region Calibrate buttons
		private void BLCalibrate_Click(object sender, EventArgs e) { calibrateClicked(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft); }
		private void BRCalibrate_Click(object sender, EventArgs e) { calibrateClicked(BRLabel, ApplicationSettings.RobotCalibration.BottomRight); }
		private void TLCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TLLabel, ApplicationSettings.RobotCalibration.TopLeft); }
		private void TRCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TRLabel, ApplicationSettings.RobotCalibration.TopRight); }
		private void TriangleCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack); }
		private void SquareCalibrate_Click(object sender, EventArgs e) { calibrateClicked(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		private static bool confirmReset() { return MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes; }

		private void resetClicked(Label label, RobotCalibrationPoint pt) {
			if(confirmReset()) {
				pt.ResetToDefault();
				updateLabel(label, pt);
			}
		}

		#region Reset Buttons
		private void ResetBL_Click(object sender, EventArgs e) { resetClicked(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft); }
		private void ResetBR_Click(object sender, EventArgs e) { resetClicked(BRLabel, ApplicationSettings.RobotCalibration.BottomRight); }
		private void ResetTL_Click(object sender, EventArgs e) { resetClicked(TLLabel, ApplicationSettings.RobotCalibration.TopLeft); }
		private void ResetTR_Click(object sender, EventArgs e) { resetClicked(TRLabel, ApplicationSettings.RobotCalibration.TopRight); }
		private void ResetTriangle_Click(object sender, EventArgs e) { resetClicked(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack); }
		private void ResetSquare_Click(object sender, EventArgs e) { resetClicked(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		private void ResetAll_Click(object sender, EventArgs e) {
			if(confirmReset()) {
				ApplicationSettings.RobotCalibration.ResetAllToDefault();
				UpdateAllLabels();
			}
		}

		private void Save_Click(object sender, EventArgs e) {
			ApplicationSettings.RobotCalibration.SaveAllSettings();
			MessageBox.Show("Successfully saved."); //The cake is a lie, just let user know buttons worked.
		}
	}
}
