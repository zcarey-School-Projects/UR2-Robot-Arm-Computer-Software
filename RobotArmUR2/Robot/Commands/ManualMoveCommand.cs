using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class ManualMoveCommand : SerialCommand{

		private Rotation rotationMove; //TODO move enum to folder
		private Extension extensionMove;

		public ManualMoveCommand(Rotation rotation, Extension extension) {
			rotationMove = rotation;
			extensionMove = extension;
		}

		public override string GetName() {
			return "Manual Move Command";
		}

		public override string getCommand() {
			return "ManualMove;";
		}

		public override string GetData() {
			return getRotationValue() + ":" + getExtensionValue() + ":";
		}

		private string getRotationValue() {
			switch (rotationMove) {
				case Rotation.CCW: return "L";
				case Rotation.CW: return "R";
				case Rotation.None:
				default: return "N";
			}
		}

		private string getExtensionValue() {
			switch (extensionMove) {
				case Extension.Inward: return "I";
				case Extension.Outward: return "O";
				case Extension.None:
				default: return "P";
			}
		}

		public override object OnSerialResponse(SerialCommunicator serial, SerialResponse response) {
			string res = response.ToString();
			if (res != "ManualMove") {
				Console.WriteLine(res);
				serial.close();
			}

			return null;
		}
	}
}
