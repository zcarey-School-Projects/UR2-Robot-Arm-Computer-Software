using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class UpdateSpeedCommand : SerialCommand{

		private int baseSpeedMS;

		public UpdateSpeedCommand(int speedMS) {
			if (speedMS < 1 || speedMS > 500) {
				baseSpeedMS = 10;
			} else {
				baseSpeedMS = speedMS;
			}
		}

		public override void OnSerialResponse(SerialResponse response) {
			throw new NotImplementedException();
		}

		public override string getCommand() {
			return "SetSpeed";
		}

		public override byte[] GetData() {
			return ToAscii(baseSpeedMS);
		}

		public override string GetName() {
			return "Update Robot Speed";
		}

	}
}
