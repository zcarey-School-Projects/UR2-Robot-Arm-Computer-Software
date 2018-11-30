using RobotArmUR2.RobotControl.Commands;
using System;
using System.Timers;
using Util.Serial;

/*
 To add new manual control command, add a variable (at top of class),
 add line in "resetManualMoveVars" to reset to default,
 and add to onTimerTick to actually send command.
 */

namespace RobotArmUR2 { //TODO fix namespaces
	public class RobotInterface {
		private static readonly object timerLock = new object();
		private static readonly object settingsLock = new object();

		private SerialCommunicator serial = new SerialCommunicator();
		private Timer manualTimer = new Timer(1000/20); //20 times per second

		private bool? setMagnetState = null;
		private bool? setServoState = null;
		//private volatile byte? setSpeed = null;
		private volatile Rotation setRotation = Rotation.None; //Is volatile needed?
		private volatile Extension setExtension = Extension.None;

		private volatile bool wasMoving = false;

		#region Events and Handlers
		//Simply pass along event from SerialCommunicator
		public event Util.Serial.SerialCommunicator.ConnectionChangedHandler OnConnectionChanged {
			add { serial.OnConnectionChanged += value; }
			remove { serial.OnConnectionChanged += value; }
		}
		#endregion

		public RobotInterface() {
			manualTimer.Elapsed += onTimerTick;
			manualTimer.AutoReset = true;
			//manualTimer.Start();
		}

		public bool ConnectToRobot() {
			//TODO add locks
			DisableManualControl();
			if (serial.AutoConnect("CH340")) {
				//Console.WriteLine("Connected Device: " + name); //TODO move to form
				lock (settingsLock) {
					//TODO send robot "onConnect" command, which enables fan, steppers, etc
					StopAll();
					PowerMagnetOff();
					RaiseServo();
					//TODO speed settings
					//GoToHome()
					EnableManualControl();
					
					return true;
				}
			} 
			return false;
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
			lock (timerLock) { //Wait for onTimerTick to finish if it hasn't
				if (!manualTimer.Enabled) return; //Already disabled
				manualTimer.Stop(); //Stop throwing tick events.
				StopAll(); //Stop all movement
			}
		}

		public void EnableManualControl() {
			lock (timerLock) {
				if (manualTimer.Enabled) return; //Already enabled
				resetManualMoveVars();
				manualTimer.Start(); //Allow event to fire
			}
		}

		private void onTimerTick(object sender, EventArgs e) {
			lock (timerLock) {
				SerialCommand moveCommand = null;
				SerialCommand magnetCommand = null;
				SerialCommand servoCommand = null;

				lock (settingsLock) {
					if((setRotation != Rotation.None) || (setExtension != Extension.None)) {
						wasMoving = true;
						moveCommand = new ManualMoveCommand(setRotation, setExtension);
					}else if (wasMoving) {
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

		public void StopAll() {
			serial.SendCommand(new EndMoveCommand());
		}


		public RobotPoint GetPosition() {
			object response = serial.SendCommand(new GetPositionCommand());
			if(response is RobotPoint) {
				return (RobotPoint)response;
			} else {
				return null;
			}
		}

		public void ReturnHome() {
			serial.SendCommand(new GoToHomeCommand());
		}

		public void MoveServo(bool raised) {
			serial.SendCommand(new MoveServoCommand(raised));
		}

		public void RaiseServo() { MoveServo(true); }
		public void LowerServo() { MoveServo(false); }

		public void PowerMagnet(bool IsOn) {
			serial.SendCommand(new SetMagnetCommand(IsOn));
		}

		public void PowerMagnetOn() { PowerMagnet(true); }
		public void PowerMagnetOff() { PowerMagnet(false); }

		public void MoveTo(RobotPoint position) {
			if (position == null) return;
			serial.SendCommand(new MoveToCommand(position));
		}

		public void MoveToWait(RobotPoint position) {//TODO command doesnt work
			if (position == null) return;
			serial.SendCommand(new MoveToWaitCommand(position));
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
