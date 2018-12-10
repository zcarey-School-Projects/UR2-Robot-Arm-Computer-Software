using RobotArmUR2.Util.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Enables the robot when a new connection is established by turning on steppers,
	/// fan, and other devices.</summary>
	class EnableRobotCommand : ISerialCommand {

		bool enable;

		public EnableRobotCommand(bool enable) {
			this.enable = enable;
		}

		public string GetName() {
			return "EnableRobot Command";
		}

		public string GetCommand() {
			return "Robot";
		}

		public string[] GetArguments() {
			return new string[] { "" + (enable ? 'E' : 'D') };
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
