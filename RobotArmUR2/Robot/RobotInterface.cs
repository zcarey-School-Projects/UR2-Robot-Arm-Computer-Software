using RobotArmUR2.Robot_Commands;
using RobotHelpers.Serial;
using System;
using System.Timers;

/*
 To add new manual control command, add a variable (at top of class),
 add line in "resetManualMoveVars" to reset to default,
 and add to onTimerTick to actually send command.
 */

namespace RobotArmUR2 {
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

		public SerialUIListener UIListener { set => serial.setSerialUIListener(value); } //TODO upgrade serial listener

		public RobotInterface() {
			manualTimer.Elapsed += onTimerTick;
			manualTimer.AutoReset = true;
			//manualTimer.Start();
		}

		public bool ConnectToRobot() {
			//TODO add locks
			DisableManualControl();
			if (serial.autoConnect()) {
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
				serial.sendCommand(new EndMoveCommand()); //Stop all movement
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

				if (moveCommand != null) serial.sendCommand(moveCommand);
				if (magnetCommand != null) serial.sendCommand(magnetCommand);
				if (servoCommand != null) serial.sendCommand(servoCommand);
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
			serial.sendCommand(new EndMoveCommand());
		}

		public float? GetRotation() {
			object response = serial.sendCommand(new GetRotationCommand());
			if((response != null) && (response is float)) {
				return (float)response;
			} else {
				return null;
			}
		}

		public float? GetExtension() {
			object response = serial.sendCommand(new GetExtensionCommand());
			if ((response != null) && (response is float)) {
				return (float)response;
			} else {
				return null;
			}
		}

		public void ReturnHome() {
			serial.sendCommand(new GoToHomeCommand());
		}

		public void MoveServo(bool raised) {
			serial.sendCommand(new MoveServoCommand(raised));
		}

		public void RaiseServo() { MoveServo(true); }
		public void LowerServo() { MoveServo(false); }

		public void PowerMagnet(bool IsOn) {
			serial.sendCommand(new SetMagnetCommand(IsOn));
		}

		public void PowerMagnetOn() { PowerMagnet(true); }
		public void PowerMagnetOff() { PowerMagnet(false); }

		public void MoveTo(RobotPoint position) {
			if (position == null) return;
			serial.sendCommand(new MoveToCommand(position));
		}

		public void MoveToWait(RobotPoint position) {//TODO command doesnt work
			if (position == null) return;
			serial.sendCommand(new MoveToWaitCommand(position));
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
