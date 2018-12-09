using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	//TODO move to where RobotInterface is.
	/// <summary>Send a manual move event. Used automatically by RobotInterface</summary>
	class ManualMoveCommand : ISerialCommand{

		private Rotation rotationMove; 
		private Extension extensionMove;

		public ManualMoveCommand(Rotation rotation, Extension extension) {
			rotationMove = rotation;
			extensionMove = extension;
		}

		public string GetName() {
			return "Manual Move Command";
		}

		public string GetCommand() {
			return "ManualMove";
		}

		public string[] GetArguments() {
			return new string[] { getRotationValue(), getExtensionValue() };
		}

		private string getRotationValue() {
			switch (rotationMove) {
				case Rotation.CCW: return "L";
				case Rotation.CW: return "R";
				case Rotation.None:
				default: return "N";
			}
		}

		private string getExtensionValue() {
			switch (extensionMove) {
				case Extension.Inward: return "I";
				case Extension.Outward: return "O";
				case Extension.None:
				default: return "P";
			}
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
