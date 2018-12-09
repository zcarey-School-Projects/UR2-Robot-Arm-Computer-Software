using System;
using System.Threading;
using System.Windows.Forms;

namespace RobotArmUR2.RobotControl {

	/// <summary> Handles running robot commands, as well as key events for manual control. </summary>
	public class Robot {

		/// <summary>Prevents multiple programs from running at the same time.</summary>
		private readonly object programLock = new object();
		
		/// <summary>The interface used to communicate with the robot.</summary>
		public RobotInterface Interface { get; } = new RobotInterface();

		/// <summary>If a program is running, the thread it is running on.</summary>
		private volatile Thread programThread;

		/// <summary>Flag to cancel a program.</summary>
		private volatile bool endProgram;

		#region Events and Handlers
		/// <summary>Fires when a program is either started or stopped.</summary>
		public event ProgramStateChangedHandler OnProgramStateChanged;
		public delegate void ProgramStateChangedHandler(bool IsRunning);

		/// <summary>Fired when a manual move state is changed (i.e. When a key is pressed for moving the robot)</summary>
		public event ManualControlChangedHandler OnManualControlChanged;
		public delegate void ManualControlChangedHandler(Rotation newRotation, Extension newExtension);
		#endregion

		public Robot() {
			
		}
		
		/// <summary>Attempts to start a new program.</summary>
		/// <param name="program">Program to attempt to run.</param>
		/// <returns>true if the program was started, false if could not start because one was already running.</returns>
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

		/// <summary>The thread that runs the program.</summary>
		/// <param name="program">Program that is running</param>
		private void ProgramLoop(RobotProgram program) {
			//Prepare robot/UI for program
			OnProgramStateChanged(true); 
			Interface.DisableManualControl();
			Interface.StopAll();
			Interface.PowerMagnetOff();
			Interface.RaiseServo();
			Thread.Sleep(500); //Give system some settling time

			Console.WriteLine("Initializing...");
			if (program.Initialize(Interface)) { //Returns false if the program shouldn't run past initialization.
				Console.WriteLine("Initialize Finished.\nRunning program...");
				while (true) {
					if (endProgram) { //Check exit flag
						Console.WriteLine("Force exiting...");
						program.ProgramCancelled(Interface);
						break;
					} else if (!program.ProgramStep(Interface)) break; //Step program. returns false when finished.
				}
			} else {
				Console.WriteLine("Initialize failed.");
			}
			
			Console.WriteLine("Program finished. \nExiting...");

			//End program
			OnProgramStateChanged(false);
			Interface.EnableManualControl();
			Console.WriteLine("Exited successfully.");
			programThread = null;
			
		}

		/// <summary>Flags the current program to exit forcefully. Does not block waiting for program to finish.</summary>
		public void CancelProgram() {
			lock (programLock) {
				if (programThread == null) return;
				endProgram = true;
			}
		}

		#region Manual Key Events
		/// <summary>Only allows one key event at a time.</summary>
		private static readonly object keyEventLock = new object();

		//Key states for events
		volatile bool keyCCWPressed = false;
		volatile bool keyCWPressed = false;
		volatile bool keyExtendPressed = false;
		volatile bool keyContractPressed = false;
		bool? lastMagnetState = null;
		bool? lastServoState = null;

		/// <summary>For a given key event, returns the appropriate new state.</summary>
		/// <param name="keyPressed">The key that was pressed</param>
		/// <param name="oppositePressed">The opposite key from the one that was pressed (i.e. if rotate CW key was pressed, opposite would be rotate CCW)</param>
		/// <param name="rot">The rotation associated with the pressed key.</param>
		/// <param name="opposite">The rotation associated with the opposite key.</param>
		/// <returns></returns>
		private static Rotation getManualControl(bool keyPressed, bool oppositePressed, Rotation rot, Rotation opposite) {
			if (keyPressed) return rot;
			else if (oppositePressed) return opposite;
			return Rotation.None;
		}

		/// <summary>For a given key event, returns the appropriate new state.</summary>
		/// <param name="keyPressed">The key that was pressed</param>
		/// <param name="oppositePressed">The opposite key from the one that was pressed (i.e. if rotate CW key was pressed, opposite would be rotate CCW)</param>
		/// <param name="rot">The rotation associated with the pressed key.</param>
		/// <param name="opposite">The rotation associated with the opposite key.</param>
		/// /// <returns></returns>
		private static Extension getManualControl(bool keyPressed, bool oppositePressed, Extension ext, Extension opposite) {
			if (keyPressed) return ext;
			else if (oppositePressed) return opposite;
			return Extension.None;
		}

		/// <summary>For every key event, checks if the pressed key was a manaul move key. If so, moves the robot accordingly.</summary>
		/// <param name="key">The key that was pressed.</param>
		/// <param name="pressed">If the key was pressed, or released.</param>
		public void ManualControlKeyEvent(Keys key, bool pressed) {
			lock (keyEventLock) { 
				if ((key == ApplicationSettings.Keybind_MagnetOn) && (lastMagnetState != true)) { //Check the key, and ensure the fired pressed state is different from the last one.
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

					if ((key == ApplicationSettings.Keybind_RotateCCW) && (pressed != keyCCWPressed)) { //Check the key, and ensures the fired pressed state is different from the last one
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

					//Send manual states as needed.
					if (setRotation != null) Interface.SetManualControl((Rotation)setRotation);
					if (setExtension != null) Interface.SetManualControl((Extension)setExtension);

					//Fires event.
					OnManualControlChanged(((setRotation != null) ? (Rotation)setRotation : Rotation.None), ((setExtension != null) ? (Extension)setExtension : Extension.None)); //Fire event
				}
			}
		}
		#endregion

	}
}
