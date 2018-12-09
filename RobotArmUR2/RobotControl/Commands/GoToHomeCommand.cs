using System;
using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Moves the robot to its home position and calibrates it. </summary>
	class GoToHomeCommand : ISerialCommand {

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
					serial.Close();
					return false;
				}

				response = serial.ReadChar();
			}

			return false;
		}
	}
}
