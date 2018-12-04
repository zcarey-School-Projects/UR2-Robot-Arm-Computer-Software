
namespace RobotArmUR2.RobotControl.Programs {
	public class ReturnHomeProgram : RobotProgram {

		public ReturnHomeProgram() {

		}

		public override bool Initialize(RobotInterface serial) {
			return true;
		}

		public override bool ProgramStep(RobotInterface serial) {
			return serial.ReturnHome();
		}

		public override void ProgramCancelled(RobotInterface serial) {
			
		}
	}
}
