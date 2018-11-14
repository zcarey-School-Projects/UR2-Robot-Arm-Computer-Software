using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {

	class LabCommand : SerialCommand {

		private float x;
		private float y;

		public LabCommand(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public override string GetName() {
			return "Lab temp";
		}

		public override string getCommand() {
			return "Lab;";
		}

		public override string GetData() {
			return x.ToString("N2") + ":" + y.ToString("N2") + ":";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			float x;
			float y;
			Console.WriteLine(response.ToString());
			if(float.TryParse(response.ToString(), out x)) {
				response = serial.ReadLine();
				Console.WriteLine(response.ToString());
				if (float.TryParse(response.ToString(), out y)) {
					Console.WriteLine("<{0}, {1}>", x.ToString("N2"), y.ToString("N2"));
					response = serial.ReadLine();
					while (response != null) {
						if (response.ToString() == "Lab") {
							return null; //Exit happily
						}
						if (response.Data.Length != 1) {
							Console.WriteLine("Wrong data length.");
							break; //Exit angrily
						}
						if (response.Data[0] != '0') {
							Console.WriteLine("Wrong data received.");
							break; //Exit VERY angrily
						}

						response = serial.ReadLine(); //MORE DATA
					}
				}
			}

			Console.WriteLine("Error lab.");
			serial.close();

			return null;
		}
	}
}
