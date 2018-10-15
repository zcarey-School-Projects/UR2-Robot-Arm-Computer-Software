using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class MoveServoCommand : SerialCommand {

		private bool raiseServo;

		public MoveServoCommand(bool raiseServo) {
			this.raiseServo = raiseServo;
		}

		public override string getCommand() {
			return (raiseServo ? "ServoR" : "ServoL");
		}

		public override byte[] GetData() {
			return null;
		}

		public override string GetName() {
			return "Move Servo Command";
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			return null;
		}
	}
}
