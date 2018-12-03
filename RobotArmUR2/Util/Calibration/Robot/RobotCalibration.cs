using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2 {
	public class RobotCalibration {
		
		public RobotCalibrationPoint BottomLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BLRobotRotation), nameof(Properties.Settings.Default.BLRobotExtension));
		public RobotCalibrationPoint TopLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TLRobotRotation), nameof(Properties.Settings.Default.TLRobotExtension));
		public RobotCalibrationPoint TopRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TRRobotRotation), nameof(Properties.Settings.Default.TRRobotExtension));
		public RobotCalibrationPoint BottomRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BRRobotRotation), nameof(Properties.Settings.Default.BRRobotExtension));

		public RobotCalibrationPoint TriangleStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TriangleStackRotation), nameof(Properties.Settings.Default.TriangleStackExtension));
		public RobotCalibrationPoint SquareStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.SquareStackRotation), nameof(Properties.Settings.Default.SquareStackExtension));

		public RobotCalibration() {
			
		}

		public void SaveSettings() { 
			BottomLeft.Save();
			TopLeft.Save();
			TopRight.Save();
			BottomRight.Save();
			TriangleStack.Save();
			SquareStack.Save();

			Properties.Settings.Default.Save();
		}

		public void ResetToDefault() {
			BottomLeft.ResetToDefault();
			TopLeft.ResetToDefault();
			TopRight.ResetToDefault();
			BottomRight.ResetToDefault();
			TriangleStack.ResetToDefault();
			SquareStack.ResetToDefault();
		}

	}
}
