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
		}//TODO get rid of this program

		public override void Initialize(RobotInterface serial) {
			float? rotation = serial.GetRotation();
			float? distance = serial.GetExtension();
			if((rotation == null) || (distance == null)) {
				MessageBox.Show("Error", "Could not retrieve data.", MessageBoxButtons.OK);
			} else {
				float rot = (float)rotation;
				float ext = (float)distance;
				switch (pointNumber) {
					case 1: Robot.Calibration.BottomLeft.Rotation = rot; Robot.Calibration.BottomLeft.Extension = ext; break;
					case 2: Robot.Calibration.TopLeft.Rotation = rot; Robot.Calibration.TopLeft.Extension = ext; break;
					case 3: Robot.Calibration.TopRight.Rotation = rot; Robot.Calibration.TopRight.Extension = ext; break;
					case 4: Robot.Calibration.BottomRight.Rotation = rot; Robot.Calibration.BottomRight.Extension = ext; break;
					case 5: Robot.Calibration.TriangleStack.Rotation = rot; Robot.Calibration.TriangleStack.Extension = ext; break;
					case 6: Robot.Calibration.SquareStack.Rotation = rot; Robot.Calibration.SquareStack.Extension = ext; break;
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
