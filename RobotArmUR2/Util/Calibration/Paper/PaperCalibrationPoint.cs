using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Paper {

	/// <summary>Saves and loads paper calibration points from application settings.</summary>
	public class PaperCalibrationPoint : PaperPoint {

		/// <summary>X position in application settings</summary>
		private Setting<float> xSetting;

		/// <summary>Y position in application settings</summary>
		private Setting<float> ySetting;

		/// <summary>Initializes the setting by finding the setting with the given name and loading the data.</summary>
		/// <param name="XName"></param>
		/// <param name="YName"></param>
		public PaperCalibrationPoint(string XName, string YName) {
			xSetting = new Setting<float>(XName);
			ySetting = new Setting<float>(YName);
			float? x = xSetting.Read();
			float? y = ySetting.Read();
			if(x == null || y == null) {
				MessageBox.Show("Could not retrieve saved data: " + XName + " & " + YName);
			} else {
				X = (float)x;
				Y = (float)y;
			}
		}

		/// <summary>Attempts to reset the point to its default values.</summary>
		/// <returns></returns>
		public bool ResetToDefault() {
			string xDefault = xSetting.GetDefaultValue();
			string yDefault = ySetting.GetDefaultValue();
			float x, y;
			if(!float.TryParse(xDefault, out x) || !float.TryParse(yDefault, out y)) {
				MessageBox.Show("Unable to retrieve default value.");
				return false;
			} else {
				X = x;
				Y = y;
				return true;
			}
		}

		/// <summary> Writes the current value to the application settings.
		/// NOTE: DOES NOT SAVE data to persistant storage. Must call Properties.Settings.Default.Save().</summary>
		/// <returns></returns>
		public bool Save() {
			bool savedX = xSetting.Set(X);
			bool savedY = ySetting.Set(Y);
			if(!savedX || !savedY) {
				MessageBox.Show("Error saving property.");
				return false;
			} else {
				return true;
			}
		}
	}
}
