using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class EndMoveCommand : SerialCommand {

		public override void OnSerialResponse(SerialResponse response) {
			throw new NotImplementedException();
		}

		public override string GetName() {
			return "End Move Command";
		}

		public override string getCommand() {
			return "Stop";
		}

		public override byte[] GetData() {
			return null;
		}

	}
}
