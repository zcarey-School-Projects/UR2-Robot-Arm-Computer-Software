using RobotArmUR2.Util;
using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Moves the robot to a specified position.</summary>
	class MoveToCommand : ISerialCommand {
		//TODO Combine MoveTo and MoveToWait, using a bool.
		private RobotPoint target;

		public MoveToCommand(RobotPoint target) {
			this.target = target;
		}

		public virtual string GetCommand() {
			return "MoveTo";
		}

		public string[] GetArguments() {
			if (target == null) return new string[] { };
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
