using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotArmUR2.Robot_Programs {
	public class CalibrationProgram : RobotProgram {

		private RobotCalibrater ui;
		private int pointNumber;

		public CalibrationProgram(Robot robot, RobotCalibrater ui, int pointNumber) : base(robot) {
			this.ui = ui;
			this.pointNumber = pointNumber;
		}

		public override void Initialize(RobotInterface serial) {
			float rotation = 0;
			float distance = 0;
			if(!serial.RequestRotation(ref rotation) || !serial.RequestDistance(ref distance)) {
				MessageBox.Show("Error", "Could not retrieve data.", MessageBoxButtons.OK);
			} else {
				switch (pointNumber) {
					case 1: Robot.Calibration.Angle1 = rotation; Robot.Calibration.Distance1 = distance; break;
					case 2: Robot.Calibration.Angle2 = rotation; Robot.Calibration.Distance2 = distance; break;
					case 3: Robot.Calibration.Angle3 = rotation; Robot.Calibration.Distance3 = distance; break;
					case 4: Robot.Calibration.Angle4 = rotation; Robot.Calibration.Distance4 = distance; break;
					case 5: Robot.Calibration.TriangleStackAngle = rotation; Robot.Calibration.TriangleStackDistance = distance; break;
					case 6: Robot.Calibration.SquareStackAngle = rotation; Robot.Calibration.SquareStackDistance = distance; break;
					default:
						Console.WriteLine("Internal Error: Point does not exist: " + pointNumber);
						return;
				}

				ui.OnCalibrationChanged();
			}
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}
	}
}
