
using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class MoveToCommand : SerialCommand {

		private RobotPoint target;

		public MoveToCommand(RobotPoint target) {
			this.target = target;
		}

		public virtual string GetCommand() {
			return "MoveTo";
		}

		public string[] GetArguments() {
			return new string[] { "R" + target.Rotation.ToString("N2"), "E" + target.Extension.ToString("N2") };
		}

		public virtual string GetName() {
			return "Move To Position";
		}

		public virtual object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
