using RobotArmUR2.RobotControl.Commands;
using RobotArmUR2.Util;
using RobotArmUR2.Util.Serial;
using System;
using System.Timers;

/*
 To add new manual control command, add a variable (at top of class),
 add line in "resetManualMoveVars" to reset to default,
 and add to onTimerTick to actually send command.
 */

namespace RobotArmUR2.RobotControl { 
	public class RobotInterface {
		private static readonly object timerLock = new object(); //Allows easy stopping of the timer.
		private static readonly object settingsLock = new object(); //Prevents manual events mishaps
		private static readonly object connectionLock = new object(); //Stop sending commands while attempting a connect.

		private SerialCommunicator serial = new SerialCommunicator();
		private Timer comTimer = new Timer(1000/20); //20 times per second

		private volatile bool manualControlEnabled = true;
		private bool? setMagnetState = null;
		private bool? setServoState = null;
		//private volatile byte? setSpeed = null;
		private volatile Rotation setRotation = Rotation.None; //Is volatile needed?
		private volatile Extension setExtension = Extension.None;
		private volatile bool wasMoving = false;

		private byte? setBasePrescale = null;
		private byte? setCarriagePrescale = null;

		#region Events and Handlers
		//Simply pass along event from SerialCommunicator
		public event SerialCommunicator.ConnectionChangedHandler OnConnectionChanged {
			add { serial.OnConnectionChanged += value; }
			remove { serial.OnConnectionChanged += value; }
		}
		#endregion

		public RobotInterface() {
			comTimer.Elapsed += onTimerTick;
			comTimer.AutoReset = true;
			comTimer.Start();
		}

		public bool ConnectToRobot() {
			lock (connectionLock) {
				DisableManualControl();
				if (serial.AutoConnect("CH340")) {
					lock (settingsLock) {
						//TODO send robot "onConnect" command, which enables fan, steppers, etc
						StopAll();
						PowerMagnetOff();
						RaiseServo();
						EnableManualControl();

						return true;
					}
				}
				return false;
			}
		}

		public void Disconnect() {
			lock (connectionLock) {
				DisableManualControl();
				StopAll();
				PowerMagnetOff();
				serial.Close(); ;
			}
		}

		private void resetManualMoveVars() {
			lock (settingsLock) {
				setMagnetState = null;
				setServoState = null;
				setRotation = Rotation.None;
				setExtension = Extension.None;
			}
		}

		public void DisableManualControl() {
			lock (connectionLock) {
				lock (timerLock) { //Wait for onTimerTick to finish if it hasn't
					if (manualControlEnabled == false) return; //Already disabled
					manualControlEnabled = false;
					StopAll(); //Stop all movement
				}
			}
		}

		public void EnableManualControl() {
			lock (connectionLock) {
				lock (timerLock) {
					if (manualControlEnabled) return; //Already enabled
					resetManualMoveVars();
					manualControlEnabled = true;
				}
			}
		}

		private void onTimerTick(object sender, EventArgs e) {
			lock (connectionLock) {
				lock (timerLock) {
					#region Manual Control
					if (manualControlEnabled) {
						SerialCommand moveCommand = null;
						SerialCommand magnetCommand = null;
						SerialCommand servoCommand = null;

						lock (settingsLock) {
							if ((setRotation != Rotation.None) || (setExtension != Extension.None)) {
								wasMoving = true;
								moveCommand = new ManualMoveCommand(setRotation, setExtension);
							} else if (wasMoving) {
								wasMoving = false;
								moveCommand = new EndMoveCommand();
							}

							if (setMagnetState != null) magnetCommand = new SetMagnetCommand((bool)setMagnetState);
							setMagnetState = null;

							if (setServoState != null) servoCommand = new MoveServoCommand((bool)setServoState);
							setServoState = null;
						}

						if (moveCommand != null) serial.SendCommand(moveCommand);
						if (magnetCommand != null) serial.SendCommand(magnetCommand);
						if (servoCommand != null) serial.SendCommand(servoCommand);
					}
					#endregion

					lock (settingsLock) {
						if (setBasePrescale != null || setCarriagePrescale != null) {
							serial.SendCommand(new SetPrescaleCommand(setBasePrescale, setCarriagePrescale));
							setBasePrescale = null;
							setCarriagePrescale = null;
						}
					}
				}
			}
		}

		public void SetManualControl(Rotation rot) {
			lock (settingsLock) {
				setRotation = rot;
			}
		}

		public void SetManualControl(Extension ext) {
			lock (settingsLock) {
				setExtension = ext;
			}
		}

		public void SetManualControl(Rotation rot, Extension ext) {
			lock (settingsLock) {
				setRotation = rot;
				setExtension = ext;
			}
		}

		public void SetManualMagnet(bool isOn) {
			lock (settingsLock) {
				setMagnetState = isOn;
			}
		}

		public void SetManualServo(bool isRaised) {
			lock (settingsLock) {
				setServoState = isRaised;
			}
		}

		public void SetBasePrescale(byte prescale) {
			if(prescale > 20) {
				Console.Error.WriteLine("Base prescale out of range, not sending: " + prescale);
				return;
			}
			lock (settingsLock) {
				setBasePrescale = prescale;
			}
		}
		//TODO send motor prescales on connect
		public void SetCarriagePrescale(byte prescale) {
			if(prescale > 20) {
				Console.Error.WriteLine("Carriage prescale out of range, not sending: " + prescale);
				return;
			}
			lock (settingsLock) {
				setCarriagePrescale = prescale;
			}
		}

		private bool sendBasicCommand(SerialCommand command) {
			lock (connectionLock) {
				object response = serial.SendCommand(command);
				if ((response != null) && (response is bool) && ((bool)response == true)) return true;
				else return false;
			}
		}

		public bool StopAll() {
			return sendBasicCommand(new EndMoveCommand());
		}

		public RobotPoint GetPosition() {
			lock (connectionLock) {
				object response = serial.SendCommand(new GetPositionCommand());
				if (response is RobotPoint) {
					return (RobotPoint)response;
				} else {
					return null;
				}
			}
		}

		public bool ReturnHome() {
			return sendBasicCommand(new GoToHomeCommand());
		}

		public bool MoveServo(bool raised) {
			return sendBasicCommand(new MoveServoCommand(raised));
		}

		public bool RaiseServo() { return MoveServo(true); }
		public bool LowerServo() { return MoveServo(false); }

		public bool PowerMagnet(bool IsOn) {
			return sendBasicCommand(new SetMagnetCommand(IsOn));
		}

		public bool PowerMagnetOn() { return PowerMagnet(true); }
		public bool PowerMagnetOff() { return PowerMagnet(false); }

		public bool MoveTo(RobotPoint position) {
			if (position == null) return false;
			return sendBasicCommand(new MoveToCommand(position));
		}

		public bool MoveToWait(RobotPoint position) {
			if (position == null) return false;
			return sendBasicCommand(new MoveToWaitCommand(position));
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
}
