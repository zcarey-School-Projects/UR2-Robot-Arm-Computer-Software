using System;
using Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class MoveToWaitCommand : MoveToCommand {

		public MoveToWaitCommand(RobotPoint target) : base(target) {
			
		}

		public override string GetCommand() {
			return "MoveToWait";
		}

		public override string GetName() {
			return "Move To and Wait";
		}

		public override object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			byte? response = serial.ReadChar();
			while(response != null) {
				char c = (char)response;
				if (c == 'E') return true;
				else if(c != 'W') {
					Console.WriteLine(GetName() + ": Invalid wait response, assuming communication error.");
					serial.close();
					return false;
				}

				response = serial.ReadChar();
			}

			return false;
		}
	}
}
