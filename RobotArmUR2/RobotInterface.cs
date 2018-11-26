using RobotHelpers.Serial;
using System.Timers;

namespace RobotArmUR2 {
	public class RobotInterface {

		private SerialCommunicator serial = new SerialCommunicator();
		private Timer manualTimer = new Timer(1000/20); //20 times per second

		private bool? setMagnetState = null;
		private bool? setServoState = null;
		//private volatile byte? setSpeed = null;
		private volatile Rotation setRotation = Rotation.None; //Is volatile needed?
		private volatile Extension setExtension = Extension.None;

		private volatile bool wasMoving = false;

		


		/*
		public RobotInterface(SerialCommunicator serial) {
			this.serial = serial;
		}

		public void GoToHome() {
			serial.sendCommand(new GoToHomeCommand());
		}


		public void SetMagnetState(bool IsOn) {
			serial.sendCommand(new SetMagnetCommand(IsOn));
		}

		public void MagnetOn() { SetMagnetState(true); }
		public void MagnetOff() { SetMagnetState(false); }


		public void SetServoState(bool IsRaised) {
			serial.sendCommand(new MoveServoCommand(IsRaised));
		}

		public void RaiseServo() { SetServoState(true); }
		public void LowerServo() { SetServoState(false); }


		public void EndMove() {
			serial.sendCommand(new EndMoveCommand());
		}

		public void MoveTo(RobotPoint pos) {
			serial.sendCommand(new MoveToCommand(pos)); //TODO remove robotinterface
		}

		public void MoveToAndWait(RobotPoint pos) {
			//TODO put inside "MoveTo" command, take bool as parameter, bool WaitForFinish = false
			serial.sendCommand(new MoveToWaitCommand(pos));
		}

		public bool RequestRotation(ref float rotationResponse) {
			object response = serial.sendCommand(new GetRotationCommand());
			if (response == null) return false;
			else {
				if (response is float) {
					rotationResponse = (float)response;
					return true;
				} else {
					return false;
				}
			}
		}

		public bool RequestDistance(ref float distanceResponse) {
			object response = serial.sendCommand(new GetExtensionCommand());
			if (response == null) return false;
			else {
				if (response is float) {
					distanceResponse = (float)response;
					return true;
				} else {
					return false;
				}
			}
		}
		*/
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
