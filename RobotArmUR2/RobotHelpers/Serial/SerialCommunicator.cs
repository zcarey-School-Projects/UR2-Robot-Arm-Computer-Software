using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace RobotHelpers.Serial {
	public class SerialCommunicator {
		//TODO: Make baud rate changeable
		private static readonly object serialLock = new object();
		private SerialPort serial;
		private SerialUIListener UIListener = null;

		public SerialCommunicator() {
			serial = new SerialPort("null", 57600, Parity.None, 8, StopBits.One);
			serial.NewLine = "\n";
			serial.ReadTimeout = 1000;
			serial.WriteTimeout = 1000;
		}

		public SerialCommunicator(SerialUIListener UI) : this() {
			this.UIListener = UI;
		}

		public void setSerialUIListener(SerialUIListener listener) {
			UIListener = listener;
		}

		public String[] getAvailablePorts() {
			return SerialPort.GetPortNames();
		}

		public bool isOpen() {
			lock (serialLock) {
				return serial.IsOpen;
			}
		}

		public void close() {
			lock (serialLock) {
				serial.Close();
				invokeConnected(false, "");
			}
		}

		public bool open(String portName) {
			lock (serialLock) {
				if (serial.IsOpen) close();
				serial.PortName = portName;
				try {
					serial.Open();
					if (!serial.IsOpen) {
						return false;
					}
					serial.WriteLine(""); //Let the robot know to cancel whatever it's doing
					Thread.Sleep(500);
					while (serial.BytesToRead > 0 || serial.BytesToWrite > 0) {
						serial.DiscardInBuffer();
						serial.DiscardOutBuffer();
						Thread.Sleep(100);
					}
					serial.WriteLine("");
					invokeConnected(true, portName);
					return true;
				} catch (Exception) {
					//Console.WriteLine("Could not connect.");
					return false;
				}
			}
		}

		public bool autoConnect() {
			return autoConnect("CH340");
		}

		public bool autoConnect(string deviceName) {
			close();
			//lock (serialLock) {
				ManagementObjectSearcher manObjSearch = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");//"Select * from Win32_SerialPort");
				ManagementObjectCollection manObjReturn = manObjSearch.Get();

				foreach (ManagementObject manObj in manObjReturn) {
					//string desc = manObj["Description"].ToString();
					//string deviceId = manObj["DeviceID"].ToString();
					object nameObject = manObj["Name"];
					if (nameObject == null) continue;
					string name = nameObject.ToString();
					if (name.Contains(deviceName) && name.Contains("(COM")) {
						int comStart = name.LastIndexOf("(COM") + 1;
						int comEnd = name.Substring(comStart).IndexOf(")") + comStart;
						string port = name.Substring(comStart, comEnd - comStart);

						if (open(port)) {
							Console.WriteLine("Connected Device: " + name);
							return true;
						}

					}
					//String portName = manObj["Name"].ToString();
					//sp.PortName = portName;
				}
			//}
			return false;
		}

		public object sendCommand(SerialCommand cmd) {
			if (!serial.IsOpen) return null;
			string command = cmd.getCommand();
			string data = cmd.GetData();
			string message = (command != null ? command : "") + (data != null ? data : "");
/*			if (message.Length > 255) {
				Console.WriteLine("WARNING: Command length exceeds 255 bytes, not sending command: " + cmd.GetName());
				message = message.Substring(0, 255);
				return null;
			}*/

			lock (serialLock) {
				try {
					Console.WriteLine("Writing: " + "/n" + message + "!");
					serial.Write("\n" + message + "!");
					//serial.Write(new byte[] { (byte)'\n' }, 0, 1);

					//byte[] messageSize = new byte[1] { (byte)message.Length };
					//serial.Write(messageSize, 0, 1);
					/*
					int numBytes = serial.ReadByte();
					byte[] bytes = new byte[numBytes];
					if (numBytes > 0) {
						serial.Read(bytes, 0, numBytes);
					}*/ //TODO return a string
					Console.WriteLine("Reading...");
					string response = serial.ReadLine();
					char[] chars = response.ToCharArray();
					byte[] bytes = new byte[chars.Length];
					for(int i = 0; i < chars.Length; i++) {
						bytes[i] = (byte)chars[i];
					}
					Console.WriteLine("Responding");
					return cmd.OnSerialResponse(this, new SerialResponse(ref bytes));
				} catch (Exception) {
					Console.WriteLine("Serial Error: " + cmd.GetName());
					Console.WriteLine("Remaining bytes: " + serial.BytesToRead);
					Console.Write("Remaining Data: ");
					while(serial.BytesToRead > 0) {
						Console.Write((char)serial.ReadChar());
					}
					Console.WriteLine();
					close();
					return null;
				}
			}
		}

		public bool SendBytes(byte[] bytes) {
			return SendBytes(bytes, 0, bytes.Length);
		}

		public bool SendBytes(byte[] bytes, int start, int length) {
			if (!serial.IsOpen) return false;
			try {
				lock (serialLock) {
					serial.Write(bytes, start, length);
				}
				return true;
			} catch (Exception e) {
				Console.WriteLine("Serial Send Error: " + e.Message);
				close();
				return false;
			}
		}

		public SerialResponse ReadBytes(int numBytes) {
			if (!serial.IsOpen) return null;
			try {
				byte[] bytes = new byte[numBytes];
				if (numBytes > 0) {
					lock (serialLock) {
						serial.Read(bytes, 0, numBytes);
					}
				}

				return new SerialResponse(ref bytes);
			} catch (Exception e) {
				Console.WriteLine("Serial Read Error: " + e.Message);
				close();
				return null;
			}
		}

		public SerialResponse ReadLine() {
			if (!serial.IsOpen) return null;
			try {
				string response = null;
				lock (serialLock) {
					response = serial.ReadLine();
				}
				if (response == null) return null;
				char[] chars = response.ToCharArray();
				byte[] bytes = new byte[chars.Length];
				for(int i = 0; i < chars.Length; i++) {
					bytes[i] = (byte)chars[i];
				}

				return new SerialResponse(ref bytes);
			} catch (Exception e) {
				Console.WriteLine("Serial Read Error: " + e.Message);
				close();
				return null;
			}
		}

		private void invokeConnected(bool isConnected, string portName) {
			if (UIListener != null) {
				UIListener.SerialOnConnectionChanged(isConnected, portName);
			}
		}

	}
}
