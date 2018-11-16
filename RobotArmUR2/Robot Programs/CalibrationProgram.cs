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
					case 1: Robot.Calibration.BottomLeft.Rotation = rotation; Robot.Calibration.BottomLeft.Extension = distance; break;
					case 2: Robot.Calibration.TopLeft.Rotation = rotation; Robot.Calibration.TopLeft.Extension = distance; break;
					case 3: Robot.Calibration.TopRight.Rotation = rotation; Robot.Calibration.TopRight.Extension = distance; break;
					case 4: Robot.Calibration.BottomRight.Rotation = rotation; Robot.Calibration.BottomRight.Extension = distance; break;
					case 5: Robot.Calibration.TriangleStack.Rotation = rotation; Robot.Calibration.TriangleStack.Extension = distance; break;
					case 6: Robot.Calibration.SquareStack.Rotation = rotation; Robot.Calibration.SquareStack.Extension = distance; break;
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
