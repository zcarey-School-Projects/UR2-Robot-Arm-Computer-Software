﻿using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	public class SetMagnetCommand : SerialCommand {

		private bool isOn;

		public SetMagnetCommand(bool isOn) {
			this.isOn = isOn;
		}

		public string GetCommand() {
			return "Magnet";
		}

		public string[] GetArguments() {
			return new string[] { (isOn ? "1" : "0") };
		}

		public string GetName() {
			return "Set Magnet State";
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
