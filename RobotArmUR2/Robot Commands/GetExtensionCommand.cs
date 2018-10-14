using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class GetExtensionCommand : SerialCommand{

		public override string getCommand() {
			return "GetDist";
		}

		public override byte[] GetData() {
			return null;
		}

		public override string GetName() {
			return "Get Extension";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if (response == null) {
				return null;
			} else {
				float value;
				if (response.ParseFloat(out value)) {
					return value;
				} else {
					return null;
				}
			}
		}
	}
}
