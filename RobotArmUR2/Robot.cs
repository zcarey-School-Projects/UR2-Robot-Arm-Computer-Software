using System;
using System.Threading;
using RobotHelpers.Serial;
using System.Windows.Forms;
using RobotArmUR2.Robot_Commands;
using RobotArmUR2.Robot_Programs;
using RobotArmUR2.VisionProcessing;
using System.Windows.Input;

namespace RobotArmUR2 {
	public class Robot {
		private static readonly object settingsLock = new object();
		private static readonly object programLock = new object();

		public IRobotUI UIListener { get => uiListener.Listener; set { uiListener.Listener = value; robotInterface.UIListener = value; } }
		public RobotCalibration Calibration { get; private set; } = new RobotCalibration();
		
		private RobotInterface robotInterface = new RobotInterface();
		private RobotUIInvoker uiListener = new RobotUIInvoker();

		private volatile Thread programThread;
		private volatile bool endProgram;

		public Robot() {
			
		}

		public Robot(IRobotUI listener) {
			UIListener = listener;
		}

		public bool ConnectToRobot() {
			return robotInterface.ConnectToRobot();
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
			uiListener.ProgramStateChanged(true);
			robotInterface.DisableManualControl();
			robotInterface.StopAll();
			robotInterface.PowerMagnetOff();
			robotInterface.RaiseServo();
			Thread.Sleep(500); //Give system some settling time

			Console.WriteLine("Initializing...");
			program.Initialize(robotInterface);
			Console.WriteLine("Initialize Finished.\nRunning program...");
			bool forceCancel = false; 
			while (true) {
				lock (programLock) {
					if (endProgram) forceCancel = true;
				}
				if (forceCancel) {
					Console.WriteLine("Force exiting...");
					program.ProgramCancelled(robotInterface);
					break;
				} else if (!program.ProgramStep(robotInterface)) break;
			}
			Console.WriteLine("Program finished. \nExiting...");

			//End program
			uiListener.ProgramStateChanged(false);
			robotInterface.EnableManualControl();
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
			lock (settingsLock) {
				if (key == ApplicationSettings.Key_MagnetOn) {
					robotInterface.SetManualMagnet(true);
				} else if (key == ApplicationSettings.Key_MagnetOff) {
					robotInterface.SetManualMagnet(false);
				}else if(key == ApplicationSettings.Key_RaiseServo) {
					robotInterface.SetManualServo(true);
				}else if(key == ApplicationSettings.Key_LowerServo) {
					robotInterface.SetManualServo(false);
				} else {
					Rotation? setRotation = null;
					Extension? setExtension = null;

					if (key == ApplicationSettings.Key_RotateCCW) { //TODO cleanup
						keyCCWPressed = pressed;
						if (keyCCWPressed) setRotation = Rotation.CCW;
						else if (keyCWPressed) setRotation = Rotation.CW;
						else setRotation = Rotation.None;
					} else if (key == ApplicationSettings.Key_RotateCW) {
						keyCWPressed = pressed;
						if (keyCWPressed) setRotation = Rotation.CW;
						else if (keyCCWPressed) setRotation = Rotation.CCW;
						else setRotation = Rotation.None;
					} else if (key == ApplicationSettings.Key_ExtendOutward) {
						keyExtendPressed = pressed;
						if (keyExtendPressed) setExtension = Extension.Outward;
						else if (keyContractPressed) setExtension = Extension.Inward;
						else setExtension = Extension.None;
					} else if (key == ApplicationSettings.Key_ExtendInward) {
						keyContractPressed = pressed;
						if (keyContractPressed) setExtension = Extension.Inward;
						else if (keyExtendPressed) setExtension = Extension.Outward;
						else setExtension = Extension.None;
					}

					if(setRotation != null) {
						robotInterface.SetManualControl((Rotation)setRotation);
						uiListener.ChangeManualRotateImage((Rotation)setRotation);
					}

					if(setExtension != null) {
						robotInterface.SetManualControl((Extension)setExtension);
						uiListener.ChangeManualExtensionImage((Extension)setExtension);
					}
				}
			}
		}

	}
}
