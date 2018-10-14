using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class GetRotationCommand : SerialCommand {

		public override string getCommand() {
			return "GetRot";
		}

		public override byte[] GetData() {
			return null;
		}

		public override string GetName() {
			return "Get Rotation";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if(response == null) {
				return null;
			} else {
				float rotationResponse;
				if (response.ParseFloat(out rotationResponse)) {
					return rotationResponse;
				} else {
					return null;
				}
			}
		}
	}
}
