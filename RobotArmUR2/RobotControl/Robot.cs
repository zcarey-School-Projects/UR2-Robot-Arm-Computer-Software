using System;
using System.Threading;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public class Robot {
		private static readonly object programLock = new object();
		
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
			program.Initialize(Interface);
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

		bool keyCCWPressed = false;
		bool keyCWPressed = false;
		bool keyExtendPressed = false;
		bool keyContractPressed = false;

		public void ManualControlKeyEvent(Keys key, bool pressed) {
			if (key == ApplicationSettings.Keybind_MagnetOn) {
				Interface.SetManualMagnet(true);
			} else if (key == ApplicationSettings.Keybind_MagnetOff) {
				Interface.SetManualMagnet(false);
			}else if(key == ApplicationSettings.Keybind_RaiseServo) {
				Interface.SetManualServo(true);
			}else if(key == ApplicationSettings.Keybind_LowerServo) {
				Interface.SetManualServo(false);
			} else {
				Rotation? setRotation = null;
				Extension? setExtension = null;

				if (key == ApplicationSettings.Keybind_RotateCCW) { //TODO cleanup
					keyCCWPressed = pressed;
					if (keyCCWPressed) setRotation = Rotation.CCW;
					else if (keyCWPressed) setRotation = Rotation.CW;
					else setRotation = Rotation.None;
				} else if (key == ApplicationSettings.Keybind_RotateCW) {
					keyCWPressed = pressed;
					if (keyCWPressed) setRotation = Rotation.CW;
					else if (keyCCWPressed) setRotation = Rotation.CCW;
					else setRotation = Rotation.None;
				} else if (key == ApplicationSettings.Keybind_ExtendOutward) {
					keyExtendPressed = pressed;
					if (keyExtendPressed) setExtension = Extension.Outward;
					else if (keyContractPressed) setExtension = Extension.Inward;
					else setExtension = Extension.None;
				} else if (key == ApplicationSettings.Keybind_ExtendInward) {
					keyContractPressed = pressed;
					if (keyContractPressed) setExtension = Extension.Inward;
					else if (keyExtendPressed) setExtension = Extension.Outward;
					else setExtension = Extension.None;
				}

				if(setRotation != null) Interface.SetManualControl((Rotation)setRotation);
				if(setExtension != null) Interface.SetManualControl((Extension)setExtension);

				OnManualControlChanged(((setRotation != null) ? (Rotation)setRotation : Rotation.None), ((setExtension != null) ? (Extension)setExtension : Extension.None)); //Fire event
			}
		}

	}
}
