
using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class EndMoveCommand : SerialCommand {

		public string GetName() {
			return "End Move Command";
		}

		public string GetCommand() {
			return "Stop";
		}

		public string[] GetArguments() {
			return null;
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return null;
		}
	}
}
