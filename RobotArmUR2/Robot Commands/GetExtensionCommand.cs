using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class GetExtensionCommand : SerialCommand{

		public override string getCommand() {
			return "GetPos;";
		}

		public override string GetData() {
			return "E:";
		}

		public override string GetName() {
			return "Get Extension";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if (response != null) {
				string str = response.ToString();
				int start = str.IndexOf('{');
				int stop = str.IndexOf('}');
				if (start < 0 || stop < 0) return null;
				string value = str.Substring(start + 1, stop - start - 1);
				float ext;
				if (!float.TryParse(value, out ext)) return null;
				return ext;
			}

			return null;
		}
	}
}
