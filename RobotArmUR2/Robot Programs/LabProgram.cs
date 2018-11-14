using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	public class LabProgram : RobotProgram {

		private float x;
		private float y;

		public LabProgram(Robot robot, float x, float y) : base(robot) {
			this.x = x;
			this.y = y;
		}

		public override void Initialize(RobotInterface serial) {
			serial.Lab(x, y);
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {

		}
	}
}
