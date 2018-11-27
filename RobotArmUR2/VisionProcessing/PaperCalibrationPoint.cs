using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotArmUR2.VisionProcessing {
	public class PaperCalibrationPoint : PaperPoint {

		private Setting<float> xSetting;
		private Setting<float> ySetting;

		public PaperCalibrationPoint(string XName, string YName) {
			xSetting = new Setting<float>(XName);
			ySetting = new Setting<float>(YName);
			float? x = xSetting.Load();
			float? y = ySetting.Load();
			if(x == null || y == null) {
				MessageBox.Show("Could not retrieve saved data: " + XName + " & " + YName);
			} else {
				X = (float)x;
				Y = (float)y;
			}
		}

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

		public bool Save() {
			bool savedX = xSetting.Save(X);
			bool savedY = ySetting.Save(Y);
			if(!savedX || !savedY) {
				MessageBox.Show("Error saving property.");
				return false;
			} else {
				return true;
			}
		}
	}
}
