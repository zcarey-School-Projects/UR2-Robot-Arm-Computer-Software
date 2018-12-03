
using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class MoveServoCommand : SerialCommand {

		private bool raiseServo;

		public MoveServoCommand(bool raiseServo) {
			this.raiseServo = raiseServo;
		}

		public string GetCommand() {
			return "Servo";
		}

		public string[] GetArguments() {
			return new string[] { (raiseServo ? "R" : "L") };
		}

		public string GetName() {
			return "Move Servo Command";
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
