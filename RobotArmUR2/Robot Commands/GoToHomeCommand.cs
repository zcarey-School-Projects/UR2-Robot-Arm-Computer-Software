using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotHelpers.Serial;

namespace RobotArmUR2.Robot_Commands {
	class GoToHomeCommand : SerialCommand {
		public override string GetName() {
			return "Go To Home";
		}

		public override string getCommand() {
			return "ReturnHome";
		}

		public override byte[] GetData() {
			return null;
		}
	}
}
