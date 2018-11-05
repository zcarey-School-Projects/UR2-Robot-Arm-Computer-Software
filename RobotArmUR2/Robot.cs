using System;
using System.Threading;
using RobotHelpers.Serial;
using System.Windows.Forms;
using RobotArmUR2.Robot_Commands;
using RobotArmUR2.Robot_Programs;
using RobotArmUR2.VisionProcessing;

//NOTE setting speed only works in manual moves
//TODO fix manual control stuff
namespace RobotArmUR2 {
	public class Robot {
		private static readonly object settingsLock = new object();
		private static readonly object programLock = new object();

		public IRobotUI UIListener { get => uiListener.Listener; set { uiListener.Listener = value; serial.setSerialUIListener(value); } }
		public RobotCalibration Calibration { get; private set; } = new RobotCalibration();
		
		
		private SerialCommunicator serial = new SerialCommunicator();
		private RobotInterface serialInterface;
		private RobotUIInvoker uiListener = new RobotUIInvoker();
		private System.Windows.Forms.Timer manualTimer = new System.Windows.Forms.Timer();

		private volatile Thread programThread;
		private volatile bool endProgram;

		private bool? setMagnetState = null;
		private bool? setServoState = null;
		private byte? setPrescale = null;
		private byte? setSpeed = null;
		private Rotation? setRotation = null;
		private Extension? setExtension = null;

		public Robot() {
			serialInterface = new RobotInterface(serial);
			manualTimer.Interval = 50;
			manualTimer.Tick += onTimerTick;
			manualTimer.Start();
		}

		public void ConnectToRobot() {
			if (serial.autoConnect()) {
				lock (settingsLock) {
					setMagnetState = false;
					setServoState = true;
					//TODO speed settings
					//setPrescale = null;
					//setSpeed = null;
					setRotation = Rotation.None;
					setExtension = Extension.None;
					//GoToHome()
				}
			} else MessageBox.Show("Device not found.");
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
				uiListener.ProgramStateChanged(true);
				return true;
			}
		}

		private void ProgramLoop(RobotProgram program) {
			//Prepare robot for program
			lock (settingsLock) { manualTimer.Stop(); }
			serialInterface.EndMove();
			serialInterface.MagnetOff();
			serialInterface.RaiseServo();
			Thread.Sleep(500); //Give system some settling time

			program.Initialize(serialInterface);
			bool forceCancel = false;
			while (true) {
				lock (programLock) {
					if (endProgram) forceCancel = true;
				}
				if (forceCancel) {
					program.ProgramCancelled(serialInterface);
					break;
				} else if (!program.ProgramStep(serialInterface)) break;
			}

			//End program
			uiListener.ProgramStateChanged(false);
			lock (settingsLock) {
				manualTimer.Start();
				programThread = null;
			}
		}

		public void CancelProgram() {
			if (programThread == null) return;
			lock (programLock) {
				endProgram = true;
			}
			//programThread.Join();
		}

		public void SetPrescale(byte prescale) { lock (settingsLock) { if (programThread == null) setPrescale = prescale; } }
		public void SetSpeed(byte speed) {
			if (speed < 5 || speed > 235) return;
			lock (settingsLock) {
				if (programThread == null) setSpeed = speed;
			}
		}

		bool keyCCWPressed = false;
		bool keyCWPressed = false;
		bool keyExtendPressed = false;
		bool keyContractPressed = false;

		public void ManualControlKeyEvent(Key key, bool pressed) {
			lock (settingsLock) {
				if (key == Key.MagnetOn) {
					setMagnetState = true;
				} else if (key == Key.MagnetOff) {
					setMagnetState = false;
				}else if(key == Key.RaiseServo) {
					setServoState = true;
				}else if(key == Key.LowerServo) {
					setServoState = false;
				} else {
					if (key == Key.RotateCCW) {
						keyCCWPressed = pressed;
						if (keyCCWPressed) setRotation = Rotation.CCW;
						else if (keyCWPressed) setRotation = Rotation.CW;
						else setRotation = Rotation.None;
					} else if (key == Key.RotateCCW) {
						keyCWPressed = pressed;
						if (keyCWPressed) setRotation = Rotation.CW;
						else if (keyCCWPressed) setRotation = Rotation.CCW;
						else setRotation = Rotation.None;
					} else if (key == Key.ExtendOutward) {
						keyExtendPressed = pressed;
						if (keyExtendPressed) setExtension = Extension.Outward;
						else if (keyContractPressed) setExtension = Extension.Inward;
						else setExtension = Extension.None;
					} else if (key == Key.ExtendInward) {
						keyContractPressed = pressed;
						if (keyContractPressed) setExtension = Extension.Inward;
						else if (keyExtendPressed) setExtension = Extension.Outward;
						else setExtension = Extension.None;
					}

					if(setRotation != null) uiListener.ChangeManualRotateImage((Rotation)setRotation);
					if(setExtension != null) uiListener.ChangeManualExtensionImage((Extension)setExtension);
				}
			}
		}

		private void onTimerTick(object sender, EventArgs e) {
			lock (settingsLock) {
				if (setRotation == null) setRotation = Rotation.None;
				if (setExtension == null) setExtension = Extension.None;
				if((setRotation == Rotation.None) && (setExtension == Extension.None)) {
					serial.sendCommand(new EndMoveCommand());
				} else {
					serial.sendCommand(new StartMoveCommand((Rotation)setRotation, (Extension)setExtension));
				}
				setRotation = null;
				setExtension = null;

				if (setMagnetState != null) serial.sendCommand(new SetMagnetCommand((bool)setMagnetState));
				setMagnetState = null;

				if (setServoState != null) serial.sendCommand(new MoveServoCommand((bool)setServoState));
				setServoState = null;

				if (setPrescale != null) serial.sendCommand(new SetPrescaleCommand((byte)setPrescale));
				setPrescale = null;

				if (setSpeed != null) serial.sendCommand(new UpdateSpeedCommand((byte)setSpeed));
				setSpeed = null;
			}
		}

		public enum Rotation {
			None,
			CW,
			CCW
		}

		public enum Extension {
			None,
			Outward,
			Inward
		}

		public enum Key {
			RotateCCW,
			RotateCW,
			ExtendOutward,
			ExtendInward,
			RaiseServo,
			LowerServo,
			MagnetOn,
			MagnetOff
		}

	}
}
