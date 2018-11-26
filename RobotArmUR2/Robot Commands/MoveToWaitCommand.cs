using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class MoveToWaitCommand : SerialCommand {

		private RobotPoint pos;

		public MoveToWaitCommand(RobotPoint pos) {
			this.pos = new RobotPoint(pos);
		}

		public override string getCommand() {
			return "MoveToWait;";
		}

		public override string GetData() {
			return "R" + pos.Rotation.ToString("N2") + ":E" + pos.Extension.ToString("N2") + ":";
		}

		public override string GetName() {
			return "Move To and Wait";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			while (response != null) {
				if (response.ToString() == "MoveToWait") {
					return null; //Exit with a happy face
				}
				if (response.Data.Length != 1) {
					Console.WriteLine("Wrong data length.");
					break; //Exit with frowny face
				}
				if (response.Data[0] != 'W') {
					Console.WriteLine("Wrong data recieved.");
					break; //Exit with ANGRY face.
				}
				response = serial.ReadLine(); //We need MMOORREE
			}

			return null;
		}
	}
}
