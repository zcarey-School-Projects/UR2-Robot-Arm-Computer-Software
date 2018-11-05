using RobotArmUR2.Robot_Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	public class ReturnHomeProgram : RobotProgram {

		public ReturnHomeProgram(Robot robot) : base(robot) {

		}

		public override void Initialize(RobotInterface serial) {
			serial.GoToHome();
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}
	}
}
