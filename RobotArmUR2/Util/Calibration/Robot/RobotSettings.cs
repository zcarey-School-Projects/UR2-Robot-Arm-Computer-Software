using System;
using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {

	/// <summary>Changes a few misc robot settings.</summary>
	public partial class RobotSettings : Form {

		private RobotControl.Robot robot;
		private int baseSliderValue;
		private int carriageSliderValue;

		public RobotSettings(RobotControl.Robot robot) {
			this.robot = robot;
			InitializeComponent();

			//Attempt to load base prescale
			byte? loadedValue = ApplicationSettings.BasePrescale.Read();
			if (loadedValue != null) {
				BasePrescaleSlider.Value = baseSliderValue = (int)loadedValue;
				updateBaseLabel();
			}

			//Attempt to load carriage prescale
			loadedValue = ApplicationSettings.CarriagePrescale.Read();
			if (loadedValue != null) {
				CarriagePrescaleSlider.Value = carriageSliderValue = (int)loadedValue;
				updateCarriageLabel();
			}

			robot.Interface.OnConnectionChanged += RobotInterface_onRobotConnectionChanged;
		}

		private void RobotSettings_Load(object sender, EventArgs e) {
			
		}

		private void RobotInterface_onRobotConnectionChanged(bool IsConnected, string PortName) {
			if (IsConnected) {
				sendBasePrescale();
				sendCarriagePrescale();
			}
		}

		private void sendBasePrescale() {
			int val = baseSliderValue;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetBasePrescale(value);
		}

		private void sendCarriagePrescale() {
			int val = carriageSliderValue;
			if (val < 0 || val > 255) return;
			byte value = (byte)val;
			robot.Interface.SetCarriagePrescale(value);
		}

		private void updateBaseLabel() {
			BasePrescaleLabel.Text = "Base Prescale: " + baseSliderValue.ToString().PadLeft(2);
		}

		private void updateCarriageLabel() {
			CarriagePrescaleLabel.Text = "Carriage Prescale: " + carriageSliderValue.ToString().PadLeft(2);
		}

		//Change base speed
		private void BasePrescaleSlider_Scroll(object sender, EventArgs e) {
			baseSliderValue = BasePrescaleSlider.Value;
			updateBaseLabel();
			sendBasePrescale();
		}

		//Change carriage speed
		private void CarriagePrescaleSlider_Scroll(object sender, EventArgs e) {
			carriageSliderValue = CarriagePrescaleSlider.Value;
			updateCarriageLabel();
			sendCarriagePrescale();
		}

		//Saves settings to persistant storage.
		private void SaveSettings_Click(object sender, EventArgs e) {
			int val = baseSliderValue;
			if (val >= 0 && val <= 255) ApplicationSettings.BasePrescale.Set((byte)val);

			val = carriageSliderValue;
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
