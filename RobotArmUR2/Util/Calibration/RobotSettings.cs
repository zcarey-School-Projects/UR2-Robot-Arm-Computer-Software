using System;
using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration {
	public partial class RobotSettings : Form {

		private RobotControl.Robot robot;

		public RobotSettings(RobotControl.Robot robot) {
			this.robot = robot;
			InitializeComponent();

			byte? loadedValue = ApplicationSettings.BasePrescale.Load();
			if (loadedValue != null) {
				BasePrescaleSlider.Value = (int)loadedValue;
				BasePrescaleSlider_Scroll(null, null);
			}

			loadedValue = ApplicationSettings.CarriagePrescale.Load();
			if (loadedValue != null) {
				CarriagePrescaleSlider.Value = (int)loadedValue;
				CarriagePrescaleSlider_Scroll(null, null);
			}
		}

		private void RobotSettings_Load(object sender, EventArgs e) {
			
		}

		private void BasePrescaleSlider_Scroll(object sender, EventArgs e) {
			int val = BasePrescaleSlider.Value;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetBasePrescale(value);
			BasePrescaleLabel.Text = "Base Prescale: " + value.ToString().PadLeft(2);
		}

		private void CarriagePrescaleSlider_Scroll(object sender, EventArgs e) {
			int val = CarriagePrescaleSlider.Value;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetCarriagePrescale(value);
			CarriagePrescaleLabel.Text = "Carriage Prescale: " + value.ToString().PadLeft(2);
		}

		private void SaveSettings_Click(object sender, EventArgs e) {
			int val = BasePrescaleSlider.Value;
			if (val >= 0 && val <= 255) ApplicationSettings.BasePrescale.Save((byte)val);

			val = CarriagePrescaleSlider.Value;
			if (val >= 0 && val <= 255) ApplicationSettings.CarriagePrescale.Save((byte)val);

			ApplicationSettings.SaveSettings();
			MessageBox.Show("Successfully saved."); //It lies. Just lets user now the button worked.
		}
	}
}
