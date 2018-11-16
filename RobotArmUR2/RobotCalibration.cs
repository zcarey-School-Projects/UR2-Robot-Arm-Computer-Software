using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2 {
	public class RobotCalibration {
		
		//TODO make calibration property naming consistant
		public RobotCalibrationPoint BottomLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BLRobotRotation), nameof(Properties.Settings.Default.BLRobotDistance));
		public RobotCalibrationPoint TopLeft { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TLRobotRotation), nameof(Properties.Settings.Default.TLRobotDistance));
		public RobotCalibrationPoint TopRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TRRobotRotation), nameof(Properties.Settings.Default.TRRobotDistance));
		public RobotCalibrationPoint BottomRight { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.BRRobotRotation), nameof(Properties.Settings.Default.BRRobotDistance));

		public RobotCalibrationPoint TriangleStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.TrianglePileAngle), nameof(Properties.Settings.Default.TrianglePileDistance));
		public RobotCalibrationPoint SquareStack { get; private set; } = new RobotCalibrationPoint(nameof(Properties.Settings.Default.SquarePileAngle), nameof(Properties.Settings.Default.SquarePileDistance));

		public RobotCalibration() {
			
		}

		public void SaveSettings() { //TODO put inside CalibrationPoint
			Properties.Settings.Default.BLRobotRotation = BottomLeft.Rotation;
			Properties.Settings.Default.BLRobotDistance = BottomLeft.Extension;

			Properties.Settings.Default.TLRobotRotation = TopLeft.Rotation;
			Properties.Settings.Default.TLRobotDistance = TopLeft.Extension;

			Properties.Settings.Default.TRRobotRotation = TopRight.Rotation;
			Properties.Settings.Default.TRRobotDistance = TopRight.Extension;

			Properties.Settings.Default.BRRobotRotation = BottomRight.Rotation;
			Properties.Settings.Default.BRRobotDistance = BottomRight.Extension;

			Properties.Settings.Default.TrianglePileAngle = TriangleStack.Rotation;
			Properties.Settings.Default.TrianglePileDistance = TriangleStack.Extension;

			Properties.Settings.Default.SquarePileAngle = SquareStack.Rotation;
			Properties.Settings.Default.SquarePileDistance = SquareStack.Extension;

			Properties.Settings.Default.Save();
		}

	}
}
