using System;
using System.Threading;
using System.Windows.Forms;

namespace RobotArmUR2.RobotControl {
	public class Robot {
		private readonly object programLock = new object();
		
		public RobotInterface Interface { get; } = new RobotInterface();

		private volatile Thread programThread;
		private volatile bool endProgram;

		#region Events and Handlers
		public delegate void ProgramStateChangedHandler(bool IsRunning);
		public event ProgramStateChangedHandler OnProgramStateChanged;

		public delegate void ManualControlChangedHandler(Rotation newRotation, Extension newExtension);
		public event ManualControlChangedHandler OnManualControlChanged;
		#endregion

		public Robot() {
			
		}
		
		//Returns if program was started
		public bool RunProgram(RobotProgram program) {
			if (programThread != null) return false;
			lock (programLock) {
				endProgram = false;
				programThread = new Thread(() => ProgramLoop(program));
				programThread.IsBackground = true;
				programThread.Name = "Robot Program";
				programThread.Start();
				return true;
			}
		}

		private void ProgramLoop(RobotProgram program) {
			//Prepare robot for program
			OnProgramStateChanged(true); //fire event
			Interface.DisableManualControl();
			Interface.StopAll();
			Interface.PowerMagnetOff();
			Interface.RaiseServo();
			Thread.Sleep(500); //Give system some settling time

			Console.WriteLine("Initializing...");
			if (program.Initialize(Interface)) {
				Console.WriteLine("Initialize Finished.\nRunning program...");
				bool forceCancel = false;
				while (true) {
					lock (programLock) {
						if (endProgram) forceCancel = true;
					}
					if (forceCancel) {
						Console.WriteLine("Force exiting...");
						program.ProgramCancelled(Interface);
						break;
					} else if (!program.ProgramStep(Interface)) break;
				}
			} else {
				Console.WriteLine("Initialize failed.");
			}
			
			Console.WriteLine("Program finished. \nExiting...");

			//End program
			OnProgramStateChanged(false); //Fire event
			Interface.EnableManualControl();
			programThread = null;
			Console.WriteLine("Exited successfully.");
		}

		public void CancelProgram() {
			if (programThread == null) return;
			lock (programLock) {
				endProgram = true;
			}
			//programThread.Join();
		}

		#region Manual Key Events
		private static readonly object keyEventLock = new object();

		volatile bool keyCCWPressed = false;
		volatile bool keyCWPressed = false;
		volatile bool keyExtendPressed = false;
		volatile bool keyContractPressed = false;
		bool? lastMagnetState = null;
		bool? lastServoState = null;

		private static Rotation getManualControl(bool keyPressed, bool oppositePressed, Rotation rot, Rotation opposite) {
			if (keyPressed) return rot;
			else if (oppositePressed) return opposite;
			return Rotation.None;
		}

		private static Extension getManualControl(bool keyPressed, bool oppositePressed, Extension ext, Extension opposite) {
			if (keyPressed) return ext;
			else if (oppositePressed) return opposite;
			return Extension.None;
		}

		public void ManualControlKeyEvent(Keys key, bool pressed) {
			lock (keyEventLock) {
				if ((key == ApplicationSettings.Keybind_MagnetOn) && (lastMagnetState != true)) {
					lastMagnetState = true;
					Interface.SetManualMagnet(true);
				} else if ((key == ApplicationSettings.Keybind_MagnetOff) && (lastMagnetState != false)) {
					lastMagnetState = false;
					Interface.SetManualMagnet(false);
				} else if ((key == ApplicationSettings.Keybind_RaiseServo) && (lastServoState != true)) {
					lastServoState = true;
					Interface.SetManualServo(true);
				} else if ((key == ApplicationSettings.Keybind_LowerServo) && (lastServoState != false)) {
					lastServoState = false;
					Interface.SetManualServo(false);
				} else {
					Rotation? setRotation = null;
					Extension? setExtension = null;

					if ((key == ApplicationSettings.Keybind_RotateCCW) && (pressed != keyCCWPressed)) {
						keyCCWPressed = pressed;
						setRotation = getManualControl(keyCCWPressed, keyCWPressed, Rotation.CCW, Rotation.CW);
					} else if ((key == ApplicationSettings.Keybind_RotateCW) && (pressed != keyCWPressed)) {
						keyCWPressed = pressed;
						setRotation = getManualControl(keyCWPressed, keyCCWPressed, Rotation.CW, Rotation.CCW);
					} else if ((key == ApplicationSettings.Keybind_ExtendInward) && (pressed != keyContractPressed)) {
						keyContractPressed = pressed;
						setExtension = getManualControl(keyContractPressed, keyExtendPressed, Extension.Inward, Extension.Outward);
					} else if ((key == ApplicationSettings.Keybind_ExtendOutward) && (pressed != keyExtendPressed)) {
						keyExtendPressed = pressed;
						setExtension = getManualControl(keyExtendPressed, keyContractPressed, Extension.Outward, Extension.Inward);
					}

					if (setRotation != null) Interface.SetManualControl((Rotation)setRotation);
					if (setExtension != null) Interface.SetManualControl((Extension)setExtension);

					OnManualControlChanged(((setRotation != null) ? (Rotation)setRotation : Rotation.None), ((setExtension != null) ? (Extension)setExtension : Extension.None)); //Fire event
				}
			}
		}
		#endregion

	}
}
