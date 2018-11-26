using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	public class MoveToPosProgram : RobotProgram {

		private RobotPoint pos;

		public MoveToPosProgram(Robot robot, RobotPoint pos) : base(robot) {
			this.pos = new RobotPoint(pos); //Ensure our copy can't be modified.
		}

		public override void Initialize(RobotInterface serial) {
			serial.MoveTo(pos);
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}

	}
}
