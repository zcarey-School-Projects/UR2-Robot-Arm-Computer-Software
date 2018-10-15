using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class MoveToCommand : SerialCommand {

		private float angle;
		private float distance;

		public MoveToCommand(float angle, float distance) {
			this.angle = angle;
			this.distance = distance;
		}

		public override string getCommand() {
			return "MoveTo";
		}

		public override byte[] GetData() {
			return GetBytes(angle.ToString("N2") + ":" + distance.ToString("N2"));
		}

		public override string GetName() {
			return "Move To Position";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			return null;
		}
	}
}
