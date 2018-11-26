using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class MoveToCommand : SerialCommand {

		private RobotPoint pos;

		public MoveToCommand(RobotPoint pos) {
			this.pos = pos;
		}

		public override string getCommand() {
			return "MoveTo;";
		}

		public override string GetData() {
			return "R" + pos.Rotation.ToString("N2") + ":E" + pos.Extension.ToString("N2") + ":";
		}

		public override string GetName() {
			return "Move To Position";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if ((response == null) || (response.ToString() != "MoveTo")) {
				serial.close(); //TODO This should be in a parent class
			}
			return null;
		}
	}
}
