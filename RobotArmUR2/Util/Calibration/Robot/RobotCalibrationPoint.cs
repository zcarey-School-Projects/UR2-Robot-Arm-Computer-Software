using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {

	/// <summary>Saves and loads robot calibration points from application settings.</summary>
	public class RobotCalibrationPoint : RobotPoint {

		/// <summary>rotation setting in application settings</summary>
		private Setting<float> rotationSetting;

		/// <summary>extension setting in application settings</summary>
		private Setting<float> extensionSetting;

		/// <summary>Initializes the setting by finding the setting with the given name and loading the data.</summary>
		/// <param name="RotationName"></param>
		/// <param name="ExtensionName"></param>
		public RobotCalibrationPoint(string RotationName, string ExtensionName){
			rotationSetting = new Setting<float>(RotationName);
			extensionSetting = new Setting<float>(ExtensionName);
			float? rot = rotationSetting.Read();
			float? ext = extensionSetting.Read();
			if (rot == null || ext == null) {
				MessageBox.Show("Could not retrieve saved data: " + RotationName + " & " + ExtensionName);
			} else {
				Rotation = (float)rot;
				Extension = (float)ext;
			}
		}

		/// <summary>Attempts to reset the point to its default values.</summary>
		/// <returns></returns>
		public bool ResetToDefault() {
			string rotDefault = rotationSetting.GetDefaultValue();
			string extDefault = extensionSetting.GetDefaultValue();
			float rot, ext;
			if (!float.TryParse(rotDefault, out rot) || !float.TryParse(extDefault, out ext)) {
				MessageBox.Show("Unable to retieve default value.");
				return false;
			} else {
				Rotation = rot;
				Extension = ext;
				return true;
			}
		}

		/// <summary> Writes the current value to the application settings.
		/// NOTE: DOES NOT SAVE data to persistant storage. Must call Properties.Settings.Default.Save().</summary>
		/// <returns></returns>
		public bool Save() {
			bool savedRot = rotationSetting.Set(Rotation);
			bool savedExt = extensionSetting.Set(Extension);
			if (!savedRot || !savedExt) {
				MessageBox.Show("Error saving property.");
				return false;
			} else {
				return true;
			}
		}

	}

}
