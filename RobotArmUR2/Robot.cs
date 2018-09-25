using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using RobotHelpers.Serial;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public class Robot {

		private SerialCommunicator serial;
		private RobotUIListener listener;

		private Rotation manualRotate = Rotation.None;
		private Extension manualExtension = Extension.None;
		private bool up = false;
		private bool down = false;
		private bool left = false;
		private bool right = false;
		private bool wasMoving = false;
		private System.Windows.Forms.Timer manualControlTimer = new System.Windows.Forms.Timer();

		public Robot() {
			serial = new SerialCommunicator();
			setTimerSettings();
		}

		public Robot(RobotUIListener listener) {
			this.listener = listener;
			serial = new SerialCommunicator(listener);
			setTimerSettings();
		}

		private void setTimerSettings() {
			manualControlTimer.Interval = 50;
			manualControlTimer.Tick += manualControlTick;
			manualControlTimer.Start();
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
				Thread.Sleep(200);
				GoToHome();
			} else {
				MessageBox.Show("Device not found.");
			}
		}

		public void GoToHome() {
			SerialResponse response = SendCommand(new GoToHomeCommand());
			while (response.Data[0] != 1) {
				response = serial.readBytes(1);
			}
		}

		public void ManualControlKeyEvent(Key key, bool pressed) {
			if (key == Key.Left) {
				left = pressed;
				if (left) {
					manualRotate = Rotation.CCW;
				}else if (right) {
					manualRotate = Rotation.CW;
				}else{
					manualRotate = Rotation.None;
				}
			} else if (key == Key.Right) {
				right = pressed;
				if (right) {
					manualRotate = Rotation.CW;
				}else if (left) {
					manualRotate = Rotation.CCW;
				}else{
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
				}else if (up) {
					manualExtension = Extension.Outward;
				} else {
					manualExtension = Extension.None;
				}
			}
			/*
			if (manualRotate == Rotation.None && manualExtension == Extension.None) {
				SendCommand(new EndMoveCommand());
			} else {
				SendCommand(new StartMoveCommand(manualRotate, manualExtension));
			}*/
		}

		private void manualControlTick(object sender, EventArgs e) {
			if (manualRotate == Rotation.None && manualExtension == Extension.None) {
				if (wasMoving) {
					wasMoving = false;
					SendCommand(new EndMoveCommand());
				}
			} else {
				wasMoving = true;
				SendCommand(new StartMoveCommand(manualRotate, manualExtension));
			}
		}

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

	}

}
