
namespace RobotArmUR2.Util.Calibration.Robot {

	/// <summary>Calibration used to link real-world paper with image paper.</summary>
	public class RobotCalibration {
		
		public RobotCalibrationPoint BottomLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BLRobotRotation), nameof(Properties.Settings.Default.BLRobotExtension));
		public RobotCalibrationPoint TopLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TLRobotRotation), nameof(Properties.Settings.Default.TLRobotExtension));
		public RobotCalibrationPoint TopRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TRRobotRotation), nameof(Properties.Settings.Default.TRRobotExtension));
		public RobotCalibrationPoint BottomRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BRRobotRotation), nameof(Properties.Settings.Default.BRRobotExtension));

		public RobotCalibrationPoint TriangleStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TriangleStackRotation), nameof(Properties.Settings.Default.TriangleStackExtension));
		public RobotCalibrationPoint SquareStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.SquareStackRotation), nameof(Properties.Settings.Default.SquareStackExtension));

		public RobotCalibration() {
			
		}

		/// <summary>Saves every point to persistant storage.</summary>
		public void SaveAllSettings() { 
			BottomLeft.Save();
			TopLeft.Save();
			TopRight.Save();
			BottomRight.Save();
			TriangleStack.Save();
			SquareStack.Save();
			ApplicationSettings.SaveSettings();
		}

		/// <summary>Resets every point to their default.</summary>
		public void ResetAllToDefault() {
			BottomLeft.ResetToDefault();
			TopLeft.ResetToDefault();
			TopRight.ResetToDefault();
			BottomRight.ResetToDefault();
			TriangleStack.ResetToDefault();
			SquareStack.ResetToDefault();
		}

	}
}
