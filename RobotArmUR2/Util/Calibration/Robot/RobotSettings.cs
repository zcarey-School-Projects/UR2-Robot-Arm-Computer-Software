using System;
using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {

	/// <summary>Changes a few misc robot settings.</summary>
	public partial class RobotSettings : Form {

		private RobotControl.Robot robot;

		public RobotSettings(RobotControl.Robot robot) {
			this.robot = robot;
			InitializeComponent();

			//Attempt to load base prescale
			byte? loadedValue = ApplicationSettings.BasePrescale.Read();
			if (loadedValue != null) {
				BasePrescaleSlider.Value = (int)loadedValue;
				BasePrescaleSlider_Scroll(null, null);
			}

			//Attempt to load carriage prescale
			loadedValue = ApplicationSettings.CarriagePrescale.Read();
			if (loadedValue != null) {
				CarriagePrescaleSlider.Value = (int)loadedValue;
				CarriagePrescaleSlider_Scroll(null, null);
			}
		}

		private void RobotSettings_Load(object sender, EventArgs e) {
			
		}


		public void SendSettings() {
			BasePrescaleSlider_Scroll(null, null);
			CarriagePrescaleSlider_Scroll(null, null);
		}

		//Change base speed
		private void BasePrescaleSlider_Scroll(object sender, EventArgs e) {
			int val = BasePrescaleSlider.Value;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetBasePrescale(value);
			BasePrescaleLabel.Text = "Base Prescale: " + value.ToString().PadLeft(2);
		}

		//Change carriage speed
		private void CarriagePrescaleSlider_Scroll(object sender, EventArgs e) {
			int val = CarriagePrescaleSlider.Value;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetCarriagePrescale(value);
			CarriagePrescaleLabel.Text = "Carriage Prescale: " + value.ToString().PadLeft(2);
		}

		//Saves settings to persistant storage.
		private void SaveSettings_Click(object sender, EventArgs e) {
			int val = BasePrescaleSlider.Value;
			if (val >= 0 && val <= 255) ApplicationSettings.BasePrescale.Set((byte)val);

			val = CarriagePrescaleSlider.Value;
			if (val >= 0 && val <= 255) ApplicationSettings.CarriagePrescale.Set((byte)val);

			ApplicationSettings.SaveSettings();
			MessageBox.Show("Successfully saved."); //It lies. Just lets user now the button worked.
		}

		//Key event to move robot.
		private void RobotSettings_KeyDown(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, true);
		}

		//Key event to move robot.
		private void RobotSettings_KeyUp(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, false);
		}
	}
}
