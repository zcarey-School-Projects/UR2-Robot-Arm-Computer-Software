using RobotHelpers.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Commands {
	class SetTargetAngleCommand : SerialCommand{

		float angle;

		public SetTargetAngleCommand(float angle) {
			this.angle = angle;
		}

		public override string getCommand() {
			return "SetAngle";
		}

		public override byte[] GetData() {
			return ToAscii(angle.ToString("N2"));
		}

		public override string GetName() {
			return "Set Target Angle";
		}

	}
}
