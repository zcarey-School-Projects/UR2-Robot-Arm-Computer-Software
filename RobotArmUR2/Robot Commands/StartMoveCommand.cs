using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class StartMoveCommand : SerialCommand{

		private Robot.Rotation rotationMove;
		private Robot.Extension extensionMove;

		public StartMoveCommand(Robot.Rotation rotation, Robot.Extension extension) {
			rotationMove = rotation;
			extensionMove = extension;
		}

		public override string GetName() {
			return "Start Move Command";
		}

		public override string getCommand() {
			return "ManualMove";
		}

		public override byte[] GetData() {
			return GetBytes(getRotationValue() + "" + getExtensionValue());
		}

		private string getRotationValue() {
			switch (rotationMove) {
				case Robot.Rotation.CCW: return "L";
				case Robot.Rotation.CW: return "R";
				case Robot.Rotation.None:
				default: return "N";
			}
		}

		private string getExtensionValue() {
			switch (extensionMove) {
				case Robot.Extension.Inward: return "I";
				case Robot.Extension.Outward: return "O";
				case Robot.Extension.None:
				default: return "N";
			}
		}

	}
}
