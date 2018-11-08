using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotHelpers.Serial;

namespace RobotArmUR2.Robot_Commands {
	class GoToHomeCommand : SerialCommand {

		private bool foundStart = false;

		public override string GetName() {
			return "Go To Home";
		}

		public override string getCommand() {
			return "ReturnHome";
		}

		public override byte[] GetData() {
			return null;
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			while(response != null) {
				if (response.Data.Length > 0) {
					if (!foundStart) {
						if (response.Data[0] == '1') {
							foundStart = true;
							Console.WriteLine("Found it!");
						} else {
							Console.WriteLine("Data: " + printData(response.Data));
						}
					} else {
						Console.WriteLine("Exit: " + printData(response.Data));
						if (response.Data[0] == '0') break;
					}
				}
				response = serial.ReadBytes(1);
			}

			return null;
		}

		private static string printData(byte[] data) {
			string s = "{";
			bool first = true;
			foreach(byte b in data) {
				if (!first) {
					s += ", ";
				}
				s += b;
				first = false;
			}
			return s + "}";
		}
	}
}
