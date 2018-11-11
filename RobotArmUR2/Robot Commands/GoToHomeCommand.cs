using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotHelpers.Serial;

namespace RobotArmUR2.Robot_Commands {
	class GoToHomeCommand : SerialCommand {

		public override string GetName() {
			return "Return Home";
		}

		public override string getCommand() {
			return "ReturnHome;";
		}

		public override byte[] GetData() {
			return null;
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			while(response != null) {
				if (response.ToString() == "ReturnHome") {
					return null; //Exit happily
				}
				if(response.Data.Length != 1) {
					Console.WriteLine("Wrong data length.");
					break; //Exit angrily
				}
				if(response.Data[0] !='0'){
					Console.WriteLine("Wrong data received.");
					break; //Exit VERY angrily
				}

				response = serial.ReadLine(); //MORE DATA
			}

			Console.WriteLine("Error returning home.");
			serial.close();

			return null;
		}
	}
}
