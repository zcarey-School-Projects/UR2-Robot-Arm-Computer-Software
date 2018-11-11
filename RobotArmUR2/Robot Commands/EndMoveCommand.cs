﻿using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class EndMoveCommand : SerialCommand {

		public override string GetName() {
			return "End Move Command";
		}

		public override string getCommand() {
			return "Stop;";
		}

		public override byte[] GetData() {
			return null;
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			if (response.ToString() != "Stop") {
				serial.close();
			}

			return null;
		}
	}
}
