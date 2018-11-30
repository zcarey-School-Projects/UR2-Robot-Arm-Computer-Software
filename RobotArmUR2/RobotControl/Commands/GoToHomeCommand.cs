using System;
using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class GoToHomeCommand : SerialCommand {

		public string GetName() {
			return "Return Home";
		}

		public string GetCommand() {
			return "ReturnHome";
		}

		public string[] GetArguments() {
			return null;
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			byte? response = serial.ReadChar();

			while(response != null) {
				char c = (char)response;

				if (c == 'E') return true;
				else if (c != 'W') {
					Console.WriteLine(GetName() + ": Invalid wait response, assuming communication error.");
					serial.close();
					return false;
				}

				response = serial.ReadChar();
			}

			return false;
		}
	}
}
