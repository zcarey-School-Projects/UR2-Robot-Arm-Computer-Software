using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands { //TODO fix namespaces
	class EndMoveCommand : SerialCommand {

		public override string GetName() {
			return "End Move Command";
		}

		public override string getCommand() {
			return "Stop;";
		}

		public override string GetData() {
			return null;
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if (response.ToString() != "Stop") {
				//Attempt a resend. If unsuccessfull, we have an issue,
				serial.close();
			}

			return null;
		}
	}
}
