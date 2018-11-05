using RobotHelpers.Serial;

namespace RobotArmUR2 {
	public interface IRobotUI : SerialUIListener {

		void ChangeManualRotateImage(Robot.Rotation state);
		void ChangeManualExtensionImage(Robot.Extension state);
		void ProgramStateChanged(bool running);

	}

	public class RobotUIInvoker {

		public IRobotUI Listener { get; set; }

		public void ChangeManualRotateImage(Robot.Rotation state) { IRobotUI listener = Listener; if (listener != null) listener.ChangeManualRotateImage(state); }
		public void ChangeManualExtensionImage(Robot.Extension state) { IRobotUI listener = Listener; if (listener != null) listener.ChangeManualExtensionImage(state); }
		public void ProgramStateChanged(bool running) { IRobotUI listener = Listener; if (listener != null) listener.ProgramStateChanged(running); }

	}
}
