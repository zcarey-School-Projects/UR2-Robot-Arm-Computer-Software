using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	public class SetMagnetCommand : SerialCommand {

		private bool isOn;

		public SetMagnetCommand(bool isOn) {
			this.isOn = isOn;
		}

		public override string getCommand() {
			return "Mag" + (isOn ? "1" : "0");
		}

		public override byte[] GetData() {
			return null;
		}

		public override string GetName() {
			return "Set Magnet State";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			return null;
		}
	}
}
