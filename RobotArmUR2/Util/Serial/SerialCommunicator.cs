using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace Util.Serial {
	public class SerialCommunicator {
		private static readonly object serialLock = new object();
		private SerialPort serial;

		public bool IsOpen { get { lock (serialLock) { return serial.IsOpen; } } }

		#region Events and Handlers
		public delegate void ConnectionChangedHandler(bool IsConnected, string PortName); 
		public event ConnectionChangedHandler OnConnectionChanged;
		#endregion

		public SerialCommunicator(int baud = 57600, Parity parity = Parity.None, StopBits stopBits = StopBits.One) {
			serial = new SerialPort("null", baud, parity, 8, stopBits);
			serial.NewLine = ">";
			serial.ReadTimeout = 500;
			serial.WriteTimeout = 500;
			serial.Encoding = Encoding.ASCII;
			serial.Disposed += closeEvent;
			serial.ErrorReceived += closeEvent;
		}

		private void closeEvent(object sender, EventArgs args) { if(serial.IsOpen) close(); OnConnectionChanged(false, null); }

		public void close() {
			lock (serialLock) {
				Console.WriteLine("SERIAL CLOSED");//TODO REMOVE after testing
				serial.Close(); //Also throws a closeEvent?
				//OnConnectionChanged(false, null);
			}
		}

		public bool Open(String portName) {
			lock (serialLock) {
				if (serial.IsOpen) close();
				serial.PortName = portName;
				try {
					serial.Open();
					if (!serial.IsOpen) return false;
					
					do {
						Thread.Sleep(50);
						serial.DiscardInBuffer();
						serial.DiscardOutBuffer();
					} while (serial.BytesToRead > 0 || serial.BytesToWrite > 0);
					
					OnConnectionChanged(true, portName);
					return true;
				} catch (Exception e) {
					Console.Error.WriteLine("\nCould not connect to device: " + e.Message);
					Console.Error.WriteLine(e.StackTrace);
					Console.Error.WriteLine();
					return false;
				}
			}
		}

		public bool AutoConnect(string deviceName) {
			lock (serialLock) { //Not necessary, but will stop communication while looking for a connection.
				close();
				ManagementObjectCollection manObjReturn = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0").Get();

				foreach (ManagementObject manObj in manObjReturn) {
					//string desc = manObj["Description"].ToString();
					//string deviceId = manObj["DeviceID"].ToString();
					object nameObject = manObj["Name"];
					if (nameObject == null) continue;
					string name = nameObject.ToString();
					if (name.Contains(deviceName) && name.Contains("(COM")) {
						int comStart = name.LastIndexOf("(COM") + 1;
						int comEnd = name.Substring(comStart).IndexOf(")") + comStart;
						if (comEnd < 0) continue; //Could not find other end of COM name
						string portName = name.Substring(comStart, comEnd - comStart);

						return Open(portName);
					}
				}

				return false;
			}
		}

		public object SendCommand(SerialCommand cmd) {
			lock (serialLock) {
				try {
					if (!serial.IsOpen) return null;
					string command = cmd.GetCommand();
					if (command == null) throw new ArgumentNullException("Can not send a null command.");
					string commandArgs = "";
					string[] args = cmd.GetArguments();
					if (args != null) {
						foreach (string arg in args) {
							if (arg == null) throw new ArgumentNullException("Can not send a null argument.");
							commandArgs += arg + ",";
						}
					}
					serial.WriteLine("<" + command + ";" + commandArgs); //Since we are using WriteLine, the ">" character gets written automatically
					string response = serial.ReadLine();
					if (!response.StartsWith("<" + command + ";")) {
						//TODO implement. close serial, cancel command
						throw new NotImplementedException();
					}
					string paramString = response.Substring(command.Length + 2);
					paramString = paramString.TrimEnd(',');
					string[] parameters = paramString.Split(',');

					return cmd.OnSerialResponse(this, parameters);
				}catch(ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return null;
				}catch(InvalidOperationException e) {
					Console.Error.WriteLine("\nTried reading/writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return null;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					close();
					return null;
				}
			}
		}

		public bool SendString(string str) {
			lock (serialLock) {
				if (!serial.IsOpen) return false;
				try {
					serial.Write(str);
					return true;
				} catch (ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return false;
				} catch (InvalidOperationException e) {
					Console.Error.WriteLine("\nTried writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return false;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					close();
					return false;
				}
			}
		}

		public byte? ReadChar() {
			lock (serialLock) {
				if (!serial.IsOpen) return null;
				try {
					int data = serial.ReadByte(); //TODO remove if there are no errors //TODO System.UnauthorizedAccessException
					if (data < 0) throw new EndOfStreamException("Well, not sure what to do now.");
					return (byte)data;
				} catch (ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return null;
				} catch (InvalidOperationException e) {
					Console.Error.WriteLine("\nTried writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					return null;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					close();
					return null;
				}
			}
		}

		public static String[] GetAvailablePorts() {
			return SerialPort.GetPortNames();
		}
	}
}
