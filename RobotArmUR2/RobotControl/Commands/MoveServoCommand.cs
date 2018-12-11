using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Either raises or lowers the servo holding the magnet.</summary>
	class MoveServoCommand : ISerialCommand {

		private bool raiseServo;

		public MoveServoCommand(bool raiseServo) {
			this.raiseServo = raiseServo;
		}

		public string GetCommand() {
			return "Servo";
		}

		public string[] GetArguments() {
			return new string[] { (raiseServo ? "R" : "L"), "T1" }; //TODO get control over this
		}

		public string GetName() {
			return "Move Servo Command";
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
