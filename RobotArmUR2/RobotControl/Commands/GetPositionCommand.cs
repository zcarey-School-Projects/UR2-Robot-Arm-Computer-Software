using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class GetPositionCommand : SerialCommand {

		public GetPositionCommand() {

		}

		public string GetName() {
			return "Get Position Command";
		}

		public string GetCommand() {
			return "GetPos";
		}

		public string[] GetArguments() {
			return new string[] { "R", "E" };
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			if (parameters.Length == 2) {
				float rot;
				float ext;
				if (float.TryParse(parameters[0], out rot) && float.TryParse(parameters[1], out ext)) {
					return new RobotPoint(rot, ext);
				}
			}

			return null;
		}

	}
}
