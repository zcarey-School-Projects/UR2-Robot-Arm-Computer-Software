
namespace RobotArmUR2.Robot_Programs {
	public class ReturnHomeProgram : RobotProgram {

		public ReturnHomeProgram(Robot robot) : base(robot) {

		}

		public override void Initialize(RobotInterface serial) {
			serial.ReturnHome();
		}

		public override bool ProgramStep(RobotInterface serial) {
			return false;
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}
	}
}
