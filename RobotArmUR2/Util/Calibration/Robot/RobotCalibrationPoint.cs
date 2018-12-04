using System.Windows.Forms;

namespace RobotArmUR2.Util.Calibration.Robot {
	public class RobotCalibrationPoint : RobotPoint {

		private Setting<float> rotationSetting;
		private Setting<float> extensionSetting;

		public RobotCalibrationPoint(string RotationName, string ExtensionName){
			rotationSetting = new Setting<float>(RotationName);
			extensionSetting = new Setting<float>(ExtensionName);
			float? rot = rotationSetting.Load();
			float? ext = extensionSetting.Load();
			if (rot == null || ext == null) {
				MessageBox.Show("Could not retrieve saved data: " + RotationName + " & " + ExtensionName);
			} else {
				Rotation = (float)rot;
				Extension = (float)ext;
			}
		}

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

		public bool Save() {
			bool savedRot = rotationSetting.Save(Rotation);
			bool savedExt = extensionSetting.Save(Extension);
			if (!savedRot || !savedExt) {
				MessageBox.Show("Error saving property.");
				return false;
			} else {
				return true;
			}
		}

	}

}
