using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class MoveToWaitCommand : SerialCommand {

		private float angle;
		private float distance;

		public MoveToWaitCommand(float angle, float distance) {
			this.angle = angle;
			this.distance = distance;
		}

		public override string getCommand() {
			return "MoveToW";
		}

		public override byte[] GetData() {
			return GetBytes(angle.ToString("N2") + ":" + distance.ToString("N2"));
		}

		public override string GetName() {
			return "Move To and Wait";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			while (response != null) {
				if (response.Data.Length < 1) break;
				if (response.Data[0] == 1) break;
				response = serial.ReadBytes(1);
			}

			return null;
		}
	}
}
