using RobotArmUR2.Util;
using System;
using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Moves to a position and blocks until the move is finished.</summary>
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
			while(response != null) { //Send 'W' or waits, to prevent serial from timeing out. Send 'E' or end, to mark a finished move.
				char c = (char)response;
				if (c == 'E') return true;
				else if(c != 'W') {
					Console.WriteLine(GetName() + ": Invalid wait response, assuming communication error.");
					serial.Close();
					return false;
				}

				response = serial.ReadChar();
			}

			return false;
		}
	}
}
