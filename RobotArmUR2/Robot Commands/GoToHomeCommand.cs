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

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			while(response != null) {
				if (response.Data.Length < 1) break;
				if (response.Data[0] == 1) break;
				response = serial.ReadBytes(1);
			}

			return null;
		}
	}
}
