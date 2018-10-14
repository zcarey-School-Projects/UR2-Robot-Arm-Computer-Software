using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class SetPrescaleCommand : SerialCommand {

		private byte prescale;

		public SetPrescaleCommand(byte prescale) {
			this.prescale = prescale;
		}

		public override string getCommand() {
			return "SetPrescale";
		}

		public override byte[] GetData() {
			return new byte[] { prescale };
		}

		public override string GetName() {
			return "Set Prescale Command";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			return null;
		}
	}
}
