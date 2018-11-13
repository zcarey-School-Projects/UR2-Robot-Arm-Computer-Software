﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.Robot_Programs {
	public class MoveToWaitProgram : RobotProgram {

		private float angle;
		private float distance;

		public MoveToWaitProgram(Robot robot, float angle, float distance) : base(robot) {
			this.angle = angle;
			this.distance = distance;
		}

		public override void Initialize(RobotInterface serial) {
			serial.MoveToAndWait(angle, distance);
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}

	}
}