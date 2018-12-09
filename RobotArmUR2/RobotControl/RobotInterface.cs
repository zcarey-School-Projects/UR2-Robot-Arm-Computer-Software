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

	/// <summary>Handles communication with the robot by handling manual events, speed setting, and moving.</summary>
	public class RobotInterface {

		/// <summary>Allows easy stopping of the timer.</summary>
		private readonly object timerLock = new object(); 

		/// <summary>Prevents manual events mishaps</summary>
		private readonly object settingsLock = new object();

		/// <summary>Stop sending commands while attempting to connect.</summary>
		private readonly object connectionLock = new object(); 

		/// <summary>The serial communication class</summary>
		private SerialCommunicator serial = new SerialCommunicator();

		/// <summary>Used to send data, like manual move events, to the robot every so often.</summary>
		private Timer comTimer = new Timer(1000/20); //20 times per second

		/// <summary>Controls if manual events should be sent over serial</summary>
		private volatile bool manualControlEnabled = true;

		//The states that should be sent over serial
		private bool? setMagnetState = null;
		private bool? setServoState = null;
		private volatile Rotation setRotation = Rotation.None; //Is volatile needed?
		private volatile Extension setExtension = Extension.None;
		private volatile bool wasMoving = false;
		private byte? setBasePrescale = null;
		private byte? setCarriagePrescale = null;

		#region Events and Handlers
		/// <summary>Fired when the robot is connected/disconnected.</summary>
		public event SerialCommunicator.ConnectionChangedHandler OnConnectionChanged { 
			add { serial.OnConnectionChanged += value; }//Simply pass on events to the serial class.
			remove { serial.OnConnectionChanged += value; }
		}
		#endregion

		/// <summary>Sets up required resources to send manual events.</summary>
		public RobotInterface() {
			comTimer.Elapsed += onTimerTick;
			comTimer.AutoReset = true;
			comTimer.Start();
		}

		/// <summary>Attempt to automatically connect to the robot. Fires OnConnectionChanged event.</summary>
		/// <returns>true if connected.</returns>
		public bool ConnectToRobot() {
			lock (connectionLock) {
				DisableManualControl();
				if (serial.AutoConnect("CH340")) {
					lock (settingsLock) {
						//TODO send robot "onConnect" command, which enables fan, steppers, etc
						//TODO send prescale data on connect.
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


		/// <summary>Stops all robot movement and disconnects from the robot.</summary>
		public void Disconnect() {
			lock (connectionLock) {
				DisableManualControl();
				StopAll();
				PowerMagnetOff();
				serial.Close(); ;
			}
		}

		/// <summary>Sets manual moving variables to their defaults.</summary>
		private void resetManualMoveVars() {
			lock (settingsLock) {
				setMagnetState = null;
				setServoState = null;
				setRotation = Rotation.None;
				setExtension = Extension.None;
			}
		}

		/// <summary>Disables manual move events</summary>
		public void DisableManualControl() {
			lock (connectionLock) {
				lock (timerLock) { //Wait for onTimerTick to finish if it hasn't
					if (manualControlEnabled == false) return; //Already disabled
					manualControlEnabled = false;
					StopAll(); //Stop all movement
				}
			}
		}

		/// <summary>Enables manual move events</summary>
		public void EnableManualControl() {
			lock (connectionLock) {
				lock (timerLock) {
					if (manualControlEnabled) return; //Already enabled
					resetManualMoveVars();
					manualControlEnabled = true;
				}
			}
		}

		/// <summary>Fired when the manual move tiemr ticks.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void onTimerTick(object sender, EventArgs e) {
			lock (connectionLock) {
				lock (timerLock) {
					#region Manual Control
					//Checks manual move states
					if (manualControlEnabled) {
						ISerialCommand moveCommand = null;
						ISerialCommand magnetCommand = null;
						ISerialCommand servoCommand = null;

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

						//Send commands if needed.
						if (moveCommand != null) serial.SendCommand(moveCommand);
						if (magnetCommand != null) serial.SendCommand(magnetCommand);
						if (servoCommand != null) serial.SendCommand(servoCommand);
					}
					#endregion

					lock (settingsLock) {
						//Checks prescale and sends command if needed.
						if (setBasePrescale != null || setCarriagePrescale != null) {
							serial.SendCommand(new SetPrescaleCommand(setBasePrescale, setCarriagePrescale));
							setBasePrescale = null;
							setCarriagePrescale = null;
						}
					}
				}
			}
		}

		/// <summary>Manually move the rotation of the robot.</summary>
		/// <param name="rot"></param>
		public void SetManualControl(Rotation rot) {
			lock (settingsLock) {
				setRotation = rot;
			}
		}

		/// <summary>Manually move the extension of the robot.</summary>
		/// <param name="ext"></param>
		public void SetManualControl(Extension ext) {
			lock (settingsLock) {
				setExtension = ext;
			}
		}

		/// <summary>Manually control both rotation and extension of the robot.</summary>
		/// <param name="rot"></param>
		/// <param name="ext"></param>
		public void SetManualControl(Rotation rot, Extension ext) {
			lock (settingsLock) {
				setRotation = rot;
				setExtension = ext;
			}
		}

		/// <summary>Sets the state of the magnet for manual control.</summary>
		/// <param name="isOn"></param>
		public void SetManualMagnet(bool isOn) {
			lock (settingsLock) {
				setMagnetState = isOn;
			}
		}

		/// <summary>Sets the state of the servo for manual control.</summary>
		/// <param name="isRaised"></param>
		public void SetManualServo(bool isRaised) {
			lock (settingsLock) {
				setServoState = isRaised;
			}
		}

		/// <summary>Sets the prescale of the base. NOTE: does not change immediately, will send command on next timer tick.</summary>
		/// <param name="prescale"></param>
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

		/// <summary> Sets the prescale of the carriage. NOTE: does not change immediately, will send command on next timer tick. </summary>
		/// <param name="prescale"></param>
		public void SetCarriagePrescale(byte prescale) {
			if(prescale > 20) {
				Console.Error.WriteLine("Carriage prescale out of range, not sending: " + prescale);
				return;
			}
			lock (settingsLock) {
				setCarriagePrescale = prescale;
			}
		}

		/// <summary>Internal u=command for sending commands that return true/false for if they worked/failed.</summary>
		/// <param name="command"></param>
		/// <returns></returns>
		private bool sendBasicCommand(ISerialCommand command) {
			lock (connectionLock) {
				object response = serial.SendCommand(command);
				if ((response != null) && (response is bool) && ((bool)response == true)) return true;
				else return false;
			}
		}

		/// <summary>Stops all robot movement.</summary>
		/// <returns></returns>
		public bool StopAll() {
			return sendBasicCommand(new EndMoveCommand());
		}


		/// <summary>Retrieves the current position from the robot.</summary>
		/// <returns></returns>
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

		/// <summary>Sends robot to the home position and calibrates it. Will block until complete.</summary>
		/// <returns></returns>
		public bool ReturnHome() {
			return sendBasicCommand(new GoToHomeCommand());
		}

		/// <summary>Raises/lowers the servo.</summary>
		/// <param name="raised"></param>
		/// <returns></returns>
		public bool MoveServo(bool raised) {
			return sendBasicCommand(new MoveServoCommand(raised));
		}


		/// <summary>Raises the servo.</summary>
		/// <returns></returns>
		public bool RaiseServo() { return MoveServo(true); }

		/// <summary>Lowers the servo.</summary>
		/// <returns></returns>
		public bool LowerServo() { return MoveServo(false); }


		/// <summary>Either powers the magnet on or turns it off.</summary>
		/// <param name="IsOn"></param>
		/// <returns></returns>
		public bool PowerMagnet(bool IsOn) {
			return sendBasicCommand(new SetMagnetCommand(IsOn));
		}

		/// <summary>Turns the magnet on.</summary>
		/// <returns></returns>
		public bool PowerMagnetOn() { return PowerMagnet(true); }

		/// <summary>Turns the magnet off.</summary>
		/// <returns></returns>
		public bool PowerMagnetOff() { return PowerMagnet(false); }

		/// <summary>Moves the robot to the specified position.</summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool MoveTo(RobotPoint position) {
			if (position == null) return false;
			return sendBasicCommand(new MoveToCommand(position));
		}

		/// <summary>Moves the robot to the specified position and block until finished.</summary>
		/// <param name="position"></param>
		/// <returns></returns>
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
