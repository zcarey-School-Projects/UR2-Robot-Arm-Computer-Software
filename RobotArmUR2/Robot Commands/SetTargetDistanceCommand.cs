using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class SetTargetDistanceCommand : SerialCommand {

		float distance;

		public SetTargetDistanceCommand(float distance) {
			this.distance = distance;
		}

		public override string getCommand() {
			return "SetRad";
		}

		public override byte[] GetData() {
			return ToAscii(distance.ToString("N2"));
		}

		public override string GetName() {
			return "Set Target Distance";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			return null;
		}
	}
}
