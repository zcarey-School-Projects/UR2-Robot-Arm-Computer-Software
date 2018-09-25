using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Management;

namespace RobotHelpers.Serial {
	public class SerialCommunicator {

		private static readonly object serialLock = new object();
		private SerialPort serial;
		private SerialUIListener UIListener = null;

		public SerialCommunicator() {
			serial = new SerialPort("null", 115200, Parity.None, 8, StopBits.One);
			serial.ReadTimeout = 5000;
			serial.WriteTimeout = 5000;
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

		public SerialResponse sendCommand(SerialCommand cmd) {
			if (!serial.IsOpen) return null;
			string command = cmd.getCommand();
			byte[] message = cmd.GetData();
			if (message == null) message = new byte[0];
			if (message.Length > 255 || command.Length > 255) {
				Console.WriteLine("WARNING: Command length exceeds 255 bytes, not sending command: " + cmd.GetName());
				return null;
			}

			//lock (serialLock) {
			try {
				byte[] cmdBytes = new byte[command.Length];
				for(int i = 0; i < command.Length; i++) {
					cmdBytes[i] = (byte)command[i];
				}
				serial.Write(cmdBytes, 0, cmdBytes.Length);
				//serial.Write(new byte[] { (byte)'\n' }, 0, 1);

				//byte[] messageSize = new byte[1] { (byte)message.Length };
				//serial.Write(messageSize, 0, 1);
				serial.Write(message, 0, message.Length);
				serial.Write(new byte[] { (byte)'\n' }, 0, 1);
				int numBytes = serial.ReadByte();
				
				return readBytes(numBytes);
			} catch (Exception) {
				Console.WriteLine("Serial Error: " + cmd.GetName());
				return null;
			}
			//}
		}

		public SerialResponse readBytes(int numBytes) {
			try {
				byte[] bytes = new byte[numBytes];
				if(numBytes > 0) {
					serial.Read(bytes, 0, numBytes);
				}

				return new SerialResponse(ref bytes);
			} catch (Exception) {
				Console.WriteLine("Serial Read Error.");
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
