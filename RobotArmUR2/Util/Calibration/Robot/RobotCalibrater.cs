using System;
using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {

	/// <summary>Calibrates the robot to link the real-world paper with the image paper.</summary>
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
		/// <summary>Called when a key is pressed.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RobotCalibrater_KeyDown(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, true);
		}

		/// <summary>called when a key is released.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RobotCalibrater_KeyUp(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, false);
		}
		#endregion

		/// <summary>Internal function that sets a label to display a robot point.</summary>
		/// <param name="label"></param>
		/// <param name="pos"></param>
		private void updateLabel(Label label, RobotPoint pos) {
			label.Text = "(" + pos.Rotation.ToString("N2") + (char)248 + ", " + pos.Extension.ToString("N2") + "mm";
		}


		/// <summary>Updates all labels in the form.</summary>
		public void UpdateAllLabels() {
			updateLabel(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft);
			updateLabel(TLLabel, ApplicationSettings.RobotCalibration.TopLeft);
			updateLabel(TRLabel, ApplicationSettings.RobotCalibration.TopRight);
			updateLabel(BRLabel, ApplicationSettings.RobotCalibration.BottomRight);
			updateLabel(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack);
			updateLabel(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack);
		}

		//Every move-to button
		#region Move-to buttons
		private void BLMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.BottomLeft); }
		private void TLMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TopLeft); }
		private void TRMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TopRight); }
		private void BRMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.BottomRight); }
		private void TriangleMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.TriangleStack); }
		private void SquareMoveTo_Click(object sender, EventArgs e) { robot.Interface.MoveTo(ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		/// <summary>Calibrates a point and updates its label.</summary>
		/// <param name="label"></param>
		/// <param name="pt"></param>
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

		//Every calibrate button
		#region Calibrate buttons
		private void BLCalibrate_Click(object sender, EventArgs e) { calibrateClicked(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft); }
		private void BRCalibrate_Click(object sender, EventArgs e) { calibrateClicked(BRLabel, ApplicationSettings.RobotCalibration.BottomRight); }
		private void TLCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TLLabel, ApplicationSettings.RobotCalibration.TopLeft); }
		private void TRCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TRLabel, ApplicationSettings.RobotCalibration.TopRight); }
		private void TriangleCalibrate_Click(object sender, EventArgs e) { calibrateClicked(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack); }
		private void SquareCalibrate_Click(object sender, EventArgs e) { calibrateClicked(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		/// <summary>Confirms with the user to perform a reset.</summary>
		/// <returns></returns>
		private static bool confirmReset() { return MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes; }

		/// <summary>When a reset button is clicked, confirm with user, reset point and update its label.</summary>
		/// <param name="label"></param>
		/// <param name="pt"></param>
		private void resetClicked(Label label, RobotCalibrationPoint pt) {
			if(confirmReset()) {
				pt.ResetToDefault();
				updateLabel(label, pt);
			}
		}

		//Every reset button
		#region Reset Buttons
		private void ResetBL_Click(object sender, EventArgs e) { resetClicked(BLLabel, ApplicationSettings.RobotCalibration.BottomLeft); }
		private void ResetBR_Click(object sender, EventArgs e) { resetClicked(BRLabel, ApplicationSettings.RobotCalibration.BottomRight); }
		private void ResetTL_Click(object sender, EventArgs e) { resetClicked(TLLabel, ApplicationSettings.RobotCalibration.TopLeft); }
		private void ResetTR_Click(object sender, EventArgs e) { resetClicked(TRLabel, ApplicationSettings.RobotCalibration.TopRight); }
		private void ResetTriangle_Click(object sender, EventArgs e) { resetClicked(TriangleLabel, ApplicationSettings.RobotCalibration.TriangleStack); }
		private void ResetSquare_Click(object sender, EventArgs e) { resetClicked(SquareLabel, ApplicationSettings.RobotCalibration.SquareStack); }
		#endregion

		/// <summary>Resets every point.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ResetAll_Click(object sender, EventArgs e) {
			if(confirmReset()) {
				ApplicationSettings.RobotCalibration.ResetAllToDefault();
				UpdateAllLabels();
			}
		}

		/// <summary>Saves all points to persistant storage.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Save_Click(object sender, EventArgs e) {
			ApplicationSettings.RobotCalibration.SaveAllSettings();
			MessageBox.Show("Successfully saved."); //The cake is a lie, just let user know buttons worked.
		}
	}
}
