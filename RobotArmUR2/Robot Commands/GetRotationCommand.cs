using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class GetRotationCommand : SerialCommand {

		public override string getCommand() {
			return "GetPos;";
		}

		public override string GetData() {
			return "R:";
		}

		public override string GetName() {
			return "Get Rotation";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if(response != null) {
				string str = response.ToString();
				Console.WriteLine(str);
				int start = str.IndexOf('{');
				int stop = str.IndexOf('}');
				if (start < 0 || stop < 0) return null;
				string value = str.Substring(start + 1, stop - start - 1);
				float rot;
				if (!float.TryParse(value, out rot)) return null;
				return rot;
			}

			return null;
		}
	}
}
