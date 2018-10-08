using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using RobotHelpers.Serial;
using System.Windows.Forms;
using System.Collections.Concurrent;

namespace RobotArmUR2 {
	public class Robot {

		private static readonly object settingsLock = new object();
		private static readonly object homeLock = new object();
		private SerialCommunicator serial;
		private RobotUIListener listener;

		private Rotation manualRotate = Rotation.None;
		private Extension manualExtension = Extension.None;
		private bool up = false;
		private bool down = false;
		private bool left = false;
		private bool right = false;
		private bool wasMoving = false;
		private System.Windows.Forms.Timer RobotComTimer = new System.Windows.Forms.Timer(); //RobotComTimer = Robot Communiation Timer

		private int robotSpeed = 100;
		private bool robotSpeedDirty = false;
		//private float temp = Properties.Settings.Default.BLRobotRotation;

		
		public static float Angle1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotRotation"].DefaultValue);
		public static float Angle2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotRotation"].DefaultValue);
		public static float Angle3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotRotation"].DefaultValue);
		public static float Angle4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotRotation"].DefaultValue);

		public static float Distance1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotDistance"].DefaultValue);
		public static float Distance2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotDistance"].DefaultValue);
		public static float Distance3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotDistance"].DefaultValue);
		public static float Distance4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotDistance"].DefaultValue);
		
		public float Angle1 { get; set; }
		public float Angle2 { get; set; }
		public float Angle3 { get; set; }
		public float Angle4 { get; set; }

		public float Distance1 { get; set; }
		public float Distance2 { get; set; }
		public float Distance3 { get; set; }
		public float Distance4 { get; set; }

		private float distanceOffset = 0; //In mm

		public Robot() {
			serial = new SerialCommunicator();
			loadSettings();
			setTimerSettings();
		}

		public Robot(RobotUIListener listener) {
			this.listener = listener;
			serial = new SerialCommunicator(listener);
			loadSettings();
			setTimerSettings();
		}

		private void loadSettings() {
			Angle1 = Properties.Settings.Default.BLRobotRotation;
			Angle2 = Properties.Settings.Default.TLRobotRotation;
			Angle3 = Properties.Settings.Default.TRRobotRotation;
			Angle4 = Properties.Settings.Default.BRRobotRotation;

			Distance1 = Properties.Settings.Default.BLRobotDistance;
			Distance2 = Properties.Settings.Default.TLRobotDistance;
			Distance3 = Properties.Settings.Default.TRRobotDistance;
			Distance4 = Properties.Settings.Default.BRRobotDistance;
		}

		private void setTimerSettings() {
			RobotComTimer.Interval = 50;
			RobotComTimer.Tick += manualControlTick;
			RobotComTimer.Start();
		}

		public void saveSettings() {
			Properties.Settings.Default.BLRobotRotation = Angle1;
			Properties.Settings.Default.TLRobotRotation = Angle2;
			Properties.Settings.Default.TRRobotRotation = Angle3;
			Properties.Settings.Default.BRRobotRotation = Angle4;

			Properties.Settings.Default.BLRobotDistance = Distance1;
			Properties.Settings.Default.TLRobotDistance = Distance2;
			Properties.Settings.Default.TRRobotDistance = Distance3;
			Properties.Settings.Default.BRRobotDistance = Distance4;

			Properties.Settings.Default.Save();
		}

		private void markAllDirty() {
			lock (settingsLock) {
				robotSpeedDirty = true;

			}
		}

		private void manualControlTick(object sender, EventArgs e) {
			lock (settingsLock) {
				if (manualRotate == Rotation.None && manualExtension == Extension.None) {
					if (wasMoving) {
						wasMoving = false;
						SendCommand(new EndMoveCommand());
					}
				} else {
					wasMoving = true;
					SendCommand(new StartMoveCommand(manualRotate, manualExtension));
				}

				if (robotSpeedDirty) {
					SendCommand(new UpdateSpeedCommand(robotSpeed));
					robotSpeedDirty = false;
				}

			}
		}

		public void setUIListener(RobotUIListener listener) {
			this.listener = listener;
			serial.setSerialUIListener(listener);
		}

		private SerialResponse SendCommand(SerialCommand command) {
			if (serial.isOpen()) {
				return serial.sendCommand(command);
			} else {
				return null;
			}
		}

		public void ConnectToRobot() {
			if (serial.autoConnect()) {
				markAllDirty();
				manualControlTick(null, null);
				//GoToHome();
			} else {
				MessageBox.Show("Device not found.");
			}
		}

		public void GoToHome() {
			lock (homeLock) {
				lock (settingsLock) {
					RobotComTimer.Stop();
				}
				SerialResponse response = SendCommand(new GoToHomeCommand());
				while (response != null) {
					if (response.Data.Length < 1) break;
					if (response.Data[0] == 1) break;
					response = serial.readBytes(1);
				}
				RobotComTimer.Start();
			}
		}

		public void ManualControlKeyEvent(Key key, bool pressed) {
			lock (settingsLock) {
				if (key == Key.Left) {
					left = pressed;
					if (left) {
						manualRotate = Rotation.CCW;
					} else if (right) {
						manualRotate = Rotation.CW;
					} else {
						manualRotate = Rotation.None;
					}
				} else if (key == Key.Right) {
					right = pressed;
					if (right) {
						manualRotate = Rotation.CW;
					} else if (left) {
						manualRotate = Rotation.CCW;
					} else {
						manualRotate = Rotation.None;
					}
				} else if (key == Key.Up) {
					up = pressed;
					if (up) {
						manualExtension = Extension.Outward;
					} else if (down) {
						manualExtension = Extension.Inward;
					} else {
						manualExtension = Extension.None;
					}
				} else if (key == Key.Down) {
					down = pressed;
					if (down) {
						manualExtension = Extension.Inward;
					} else if (up) {
						manualExtension = Extension.Outward;
					} else {
						manualExtension = Extension.None;
					}
				} else {
					return;
				}
			}

			if (listener != null) {
				listener.ChangeManualRotateImage(manualRotate);
				listener.ChangeManualExtensionImage(manualExtension);
			}
		}

		public void changeRobotSpeed(int newSpeed) {
			if (newSpeed < 5 || newSpeed > 235) newSpeed = 100;
			lock (settingsLock) {
				if (newSpeed != robotSpeed) {
					robotSpeed = newSpeed;
					robotSpeedDirty = true;
				}
			}
		}

		public bool requestRotation(ref float rotationResponse) {
			lock (homeLock) {
				SerialResponse response = serial.sendCommand(new GetRotation());
				if (response == null) {
					return false;
				} else {
					return response.ParseFloat(out rotationResponse);
				}
			}
		}

		public bool requestExtension(ref float extensionResponse) {
			lock (homeLock) {
				SerialResponse response = serial.sendCommand(new GetExtension());
				if (response == null) {
					return false;
				} else {
					return response.ParseFloat(out extensionResponse);
				}
			}
		}

		public void moveTo(float angle, float distance) {
			lock (homeLock) {
				SendCommand(new SetTargetAngle(angle));
				SendCommand(new SetTargetDistance(distance));
			}
		}

		#region Serial Commands

		private class GoToHomeCommand : SerialCommand {
			public override string GetName() {
				return "Go To Home";
			}

			public override string getCommand() {
				return "ReturnHome";
			}

			public override byte[] GetData() {
				return null;
			}
		}

		private class StartMoveCommand : SerialCommand {
			private Rotation rotationMove;
			private Extension extensionMove;

			public StartMoveCommand(Rotation rotation, Extension extension) {
				rotationMove = rotation;
				extensionMove = extension;
			}

			public override string GetName() {
				return "Start Move Command";
			}

			public override string getCommand() {
				return "ManualMove";
			}

			public override byte[] GetData() {
				return GetBytes(getRotationValue() + "" + getExtensionValue());
			}

			private string getRotationValue() {
				switch (rotationMove) {
					case Rotation.CCW: return "L";
					case Rotation.CW: return "R";
					case Rotation.None:
					default: return "N";
				}
			}

			private string getExtensionValue() {
				switch (extensionMove) {
					case Extension.Inward: return "I";
					case Extension.Outward: return "O";
					case Extension.None:
					default: return "N";
				}
			}
		}

		private class EndMoveCommand : SerialCommand {
			public override string GetName() {
				return "End Move Command";
			}

			public override string getCommand() {
				return "Stop";
			}

			public override byte[] GetData() {
				return null;
			}
		}

		private class UpdateSpeedCommand : SerialCommand {

			private int baseSpeedMS;

			public UpdateSpeedCommand(int speedMS) {
				if (speedMS < 1 || speedMS > 500) {
					baseSpeedMS = 10;
				} else {
					baseSpeedMS = speedMS;
				}
			}

			public override string getCommand() {
				return "SetSpeed";
			}

			public override byte[] GetData() {
				return ToAscii(baseSpeedMS);
			}

			public override string GetName() {
				return "Update Robot Speed";
			}
		}

		private class GetRotation : SerialCommand {
			public override string getCommand() {
				return "GetRot";
			}

			public override byte[] GetData() {
				return null;
			}

			public override string GetName() {
				return "Get Rotation";
			}
		}

		private class GetExtension : SerialCommand {
			public override string getCommand() {
				return "GetDist";
			}

			public override byte[] GetData() {
				return null;
			}

			public override string GetName() {
				return "Get Extension";
			}
		}

		private class SetTargetAngle : SerialCommand {
			float angle;

			public SetTargetAngle(float angle) {
				this.angle = angle;
			}

			public override string getCommand() {
				return "SetAngle";
			}

			public override byte[] GetData() {
				return ToAscii(angle.ToString("N2"));
			}

			public override string GetName() {
				return "Set Target Angle";
			}
		}

		private class SetTargetDistance : SerialCommand {
			float distance;

			public SetTargetDistance(float distance) {
				this.distance = distance;
			}

			public override string getCommand() {
				return "SetRad";
			}

			public override byte[] GetData() {
				return ToAscii(distance.ToString("N2"));
			}

			public override string GetName() {
				return "Set Target Distance";
			}
		}

		#endregion

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
			Left,
			Right, 
			Up,
			Down
		}

	}

	public interface RobotUIListener : SerialUIListener {

		void ChangeManualRotateImage(Robot.Rotation state);

		void ChangeManualExtensionImage(Robot.Extension state);

	}

}
