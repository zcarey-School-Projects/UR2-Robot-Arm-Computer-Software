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
using RobotArmUR2.Robot_Commands;
using Emgu.CV.Structure;
using RobotArmUR2.Robot_Programs;

namespace RobotArmUR2 {
	public class Robot {
		private static readonly object stackingProgramLock = new object();
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

		private byte robotPrescale = 0;
		private bool robotPrescaleDirty = false;


		private volatile Thread programThread;
		private volatile bool endProgram = false;
		//private float temp = Properties.Settings.Default.BLRobotRotation;


		public static float Angle1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotRotation"].DefaultValue);
		public static float Angle2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotRotation"].DefaultValue);
		public static float Angle3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotRotation"].DefaultValue);
		public static float Angle4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotRotation"].DefaultValue);

		public static float Distance1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotDistance"].DefaultValue);
		public static float Distance2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotDistance"].DefaultValue);
		public static float Distance3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotDistance"].DefaultValue);
		public static float Distance4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotDistance"].DefaultValue);

		public static float TriangleStackAngleDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TrianglePileAngle"].DefaultValue);
		public static float TriangleStackDistanceDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TrianglePileDistance"].DefaultValue);

		public static float SquareStackAngleDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["SquarePileAngle"].DefaultValue);
		public static float SquareStackDistanceDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["SquarePileDistance"].DefaultValue);

		public float Angle1 { get; set; }
		public float Angle2 { get; set; }
		public float Angle3 { get; set; }
		public float Angle4 { get; set; }

		public float Distance1 { get; set; }
		public float Distance2 { get; set; }
		public float Distance3 { get; set; }
		public float Distance4 { get; set; }

		public float TriangleStackAngle { get; set; }
		public float TriangleStackDistance { get; set; }

		public float SquareStackAngle { get; set; }
		public float SquareStackDistance { get; set; }

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

			TriangleStackAngle = Properties.Settings.Default.TrianglePileAngle;
			TriangleStackDistance = Properties.Settings.Default.TrianglePileDistance;

			SquareStackAngle = Properties.Settings.Default.SquarePileAngle;
			SquareStackDistance = Properties.Settings.Default.SquarePileDistance;
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
				robotPrescaleDirty = true;
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

				if (robotPrescaleDirty) {
					SendCommand(new SetPrescaleCommand(robotPrescale));
					robotPrescaleDirty = false;
				}

			}
		}

		public bool runStackingProgram(Vision vision) { //Returns true if a new thread was started
			lock (stackingProgramLock) {
				if (programThread != null /*|| !serial.isOpen()*/) return false;
				endProgram = false;
				programThread = new Thread(() => StackingProgram(vision));
				programThread.IsBackground = true;
				programThread.Name = "Stacking Program";
				programThread.Start();

				return true;
			}
		}

		public void cancelStackingProgram() {
			lock (stackingProgramLock) {
				if(programThread != null) {
					endProgram = true;
					programThread.Join();
					endProgram = false;
					if (listener != null) listener.ProgramStateChanged(false);
					programThread = null;
				}
			}
		}

		private void StackingProgram(Vision vision) {
			//Before we start, inform the listener that the program is running
			if (listener != null) listener.ProgramStateChanged(true);
			//Disable timer wait for finish
			RobotComTimer.Enabled = false;
			lock (settingsLock) {
				//Timer is finished, reset manual move stuff
				up = false;
				down = false;
				left = false;
				right = false;
				manualRotate = Rotation.None;
				manualExtension = Extension.None;
				wasMoving = false;
			}

			//Reset manual control, stop all moves
			SendCommand(new EndMoveCommand());
			Thread.Sleep(500);

			StackingProgram program = new StackingProgram(this, vision);
			program.runProgram();


			//Before we end the thread, let the listener know we are done
			if (listener != null) listener.ProgramStateChanged(false);

			lock (stackingProgramLock) {
				programThread = null; //Let the program know we are finished.
			}
		}

		public void raiseServo() {
			SendCommand(new MoveServoCommand(true));
		}

		public void lowerServo() {
			SendCommand(new MoveServoCommand(false));
		}

		public void setUIListener(RobotUIListener listener) {
			this.listener = listener;
			serial.setSerialUIListener(listener);
		}

		private object SendCommand(SerialCommand command) {
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
				SendCommand(new GoToHomeCommand()); //Blocks until an error occurs or the robot has reached the home position.
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
				object response = serial.sendCommand(new GetRotationCommand());
				if (response == null) {
					return false;
				} else {
					if (response is float) {
						rotationResponse = (float)response;
						return true;
					} else {
						return false;
					}
				}
			}
		}

		public bool requestExtension(ref float extensionResponse) {
			lock (homeLock) {
				object response = SendCommand(new GetExtensionCommand());
				if (response == null) {
					return false;
				} else {
					if (response is float) {
						extensionResponse = (float)response;
						return true;
					} else {
						return false;
					}
				}
			}
		}

		public void changeRobotPrescale(int prescale) {
			if (prescale < 0) prescale = 0;
			if (prescale > 255) prescale = 255;
			robotPrescale = (byte)prescale;
			robotPrescaleDirty = true;
		}

		public void moveTo(float angle, float distance) {
			lock (homeLock) {
				SendCommand(new MoveToCommand(angle, distance));
			}
		}

		public void moveToAndWait(float angle, float distance) {
			lock (homeLock) {
				SendCommand(new MoveToWaitCommand(angle, distance));
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

		void ChangeManualRotateImage(Robot.Rotation state);

		void ChangeManualExtensionImage(Robot.Extension state);

		void ProgramStateChanged(bool running);

	}

}
