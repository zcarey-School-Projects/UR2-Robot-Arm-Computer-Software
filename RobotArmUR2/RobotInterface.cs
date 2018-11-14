using RobotArmUR2.Robot_Commands;
using RobotHelpers.Serial;

namespace RobotArmUR2 {
	public class RobotInterface {

		private SerialCommunicator serial;

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

		public void MoveTo(float angle, float distance) {
			serial.sendCommand(new MoveToCommand(angle, distance));
		}

		public void MoveToAndWait(float angle, float distance) {
			//TODO put inside "MoveTo" command, take bool as parameter, bool WaitForFinish = false
			serial.sendCommand(new MoveToWaitCommand(angle, distance));
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

		public void Lab(float x, float y) {
			serial.sendCommand(new LabCommand(x, y));
		}

	}
}
