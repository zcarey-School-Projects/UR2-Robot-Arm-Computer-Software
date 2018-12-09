using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Stops all movement from the robot.</summary>
	class EndMoveCommand : ISerialCommand {

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
			return true;
		}
	}
}
