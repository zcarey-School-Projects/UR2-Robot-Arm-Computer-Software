
namespace RobotArmUR2.RobotControl.Programs {

	/// <summary>Program that returns the robot home. This is intended to be used by the user, so they can click a button.
	/// the program causes the UI to "lock" while the robots goes home, since serial can't be used while calling the
	/// ReturnHome program.</summary>
	public class ReturnHomeProgram : RobotProgram {

		public ReturnHomeProgram() {

		}

		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.Initialize(RobotInterface)"/>
		public override bool Initialize(RobotInterface serial) {
			return true;
		}

		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.ProgramStep(RobotInterface)"/>
		public override bool ProgramStep(RobotInterface serial) {
			return !serial.ReturnHome();
		}

		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.ProgramCancelled(RobotInterface)"/>
		public override void ProgramCancelled(RobotInterface serial) {
			
		}
	}
}
