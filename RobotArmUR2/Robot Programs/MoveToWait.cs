using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	public class MoveToWaitProgram : RobotProgram {

		private RobotPoint pos;

		public MoveToWaitProgram(Robot robot, RobotPoint pos) : base(robot) {
			this.pos = pos;
		}

		public override void Initialize(RobotInterface serial) {
			serial.MoveToWait(pos);
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}

	}
}
