using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public partial class RobotCalibrater : Form {

		private Robot robot;

		public RobotCalibrater(Robot robot) {
			this.robot = robot;
			InitializeComponent();
		}

		private void RobotCalibrater_Load(object sender, EventArgs e) {
			updateLabels();
		}

		private void RobotCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			robot.saveSettings();
		}

		private void RobotCalibrater_KeyDown(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, true);
		}

		private void RobotCalibrater_KeyUp(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, false);
		}

		public void formKeyEvent(Keys key, bool pressed) {
			if ((key == Keys.A) || (key == Keys.Left)) {
				robot.ManualControlKeyEvent(Robot.Key.Left, pressed);
			} else if ((key == Keys.D) || (key == Keys.Right)) {
				robot.ManualControlKeyEvent(Robot.Key.Right, pressed);
			} else if ((key == Keys.W) || (key == Keys.Up)) {
				robot.ManualControlKeyEvent(Robot.Key.Up, pressed);
			} else if ((key == Keys.S) || (key == Keys.Down)) {
				robot.ManualControlKeyEvent(Robot.Key.Down, pressed);
			}
		}

		private void BLMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.Angle1, robot.Distance1);
		}

		private void TLMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.Angle2, robot.Distance2);
		}

		private void TRMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.Angle3, robot.Distance3);
		}

		private void BRMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.Angle4, robot.Distance4);
		}

		private void calibrateClicked(int pointNumber) {
			float rotation = 0;
			float distance = 0;
			if (!robot.requestRotation(ref rotation) || !robot.requestExtension(ref distance)) {
				MessageBox.Show("Error", "Could not retrieve data.", MessageBoxButtons.OK);
				return;
			}

			switch (pointNumber) {
				case 1: robot.Angle1 = rotation; robot.Distance1 = distance; break;
				case 2: robot.Angle2 = rotation; robot.Distance2 = distance; break;
				case 3: robot.Angle3 = rotation; robot.Distance3 = distance; break;
				case 4: robot.Angle4 = rotation; robot.Distance4 = distance; break;
				default:
					Console.WriteLine("Internal Error: Point does not exist: " + pointNumber);
					break;
			}
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

		private static void updateLabel(Label label, float angle, float distance) {
			label.Text = "(" + angle.ToString("N2") + (char)248 + ", " + distance.ToString("N2") + "mm)";
		}

		private void updateLabels() {
			updateLabel(BLLabel, robot.Angle1, robot.Distance1);
			updateLabel(TLLabel, robot.Angle2, robot.Distance2);
			updateLabel(TRLabel, robot.Angle3, robot.Distance3);
			updateLabel(BRLabel, robot.Angle4, robot.Distance4);
			updateLabel(TriangleLabel, robot.TriangleStackAngle, robot.TriangleStackDistance);
			updateLabel(SquareLabel, robot.SquareStackAngle, robot.SquareStackDistance);
		}

		private static bool confirmReset() {
			return MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes;
		}

		private void resetPoint1() {
			robot.Angle1 = Robot.Angle1Default;
			robot.Distance1 = Robot.Distance1Default;
		}

		private void resetPoint2() {
			robot.Angle2 = Robot.Angle2Default;
			robot.Distance2 = Robot.Distance2Default;
		}

		private void resetPoint3() {
			robot.Angle3 = Robot.Angle3Default;
			robot.Distance3 = Robot.Distance3Default;
		}

		private void resetPoint4() {
			robot.Angle4 = Robot.Angle4Default;
			robot.Distance4 = Robot.Distance4Default;
		}

		private void resetTriangle() {
			robot.TriangleStackAngle = Robot.TriangleStackAngleDefault;
			robot.TriangleStackDistance = Robot.TriangleStackDistanceDefault;
		}

		private void resetSquare() {
			robot.SquareStackAngle = Robot.SquareStackAngleDefault;
			robot.SquareStackDistance = Robot.SquareStackDistanceDefault;
		}

		private void ResetBL_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint1();
				updateLabels();
			}
		}

		private void ResetBR_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint4();
				updateLabels();
			}
		}

		private void ResetTL_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint2();
				updateLabels();
			}
		}

		private void ResetTR_Click(object sender, EventArgs e) {
			if (confirmReset()) {
				resetPoint3();
				updateLabels();
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
				updateLabels();
			}
		}

		private void TriangleMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.TriangleStackAngle, robot.TriangleStackDistance);
		}

		private void SquareMoveTo_Click(object sender, EventArgs e) {
			robot.moveTo(robot.SquareStackAngle, robot.SquareStackDistance);
		}

		private void TriangleCalibrate_Click(object sender, EventArgs e) {
			float rotation = 0;
			float distance = 0;
			if (!robot.requestRotation(ref rotation) || !robot.requestExtension(ref distance)) {
				MessageBox.Show("Error", "Could not retrieve data.", MessageBoxButtons.OK);
				return;
			}

			robot.TriangleStackAngle = rotation;
			robot.TriangleStackDistance = distance;
		}

		private void SquareCalibrate_Click(object sender, EventArgs e) {
			float rotation = 0;
			float distance = 0;
			if (!robot.requestRotation(ref rotation) || !robot.requestExtension(ref distance)) {
				MessageBox.Show("Error", "Could not retrieve data.", MessageBoxButtons.OK);
				return;
			}

			robot.SquareStackAngle = rotation;
			robot.SquareStackDistance = distance;
		}

		private void ResetTriangle_Click(object sender, EventArgs e) {
			resetTriangle();
		}

		private void ResetSquare_Click(object sender, EventArgs e) {
			resetSquare();
		}
	}
}
