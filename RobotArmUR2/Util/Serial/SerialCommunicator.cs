using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace RobotArmUR2.Util.Serial {

	/// <summary> Deals with communicating over a serial connection to any compatible devices. 
	/// Serial commands are sent using the "The Very Special Format" format. All data sends in the form of "&lt;CommandName;Param1:Param2:..."&gt;
	/// Where CommandName is the name of the command to be sent, and Param1, Param2, etc are the parameters to be sent with the command.</summary>
	public class SerialCommunicator : IDisposable{

		/// <summary> Only allows one thread to be accessing the serial port at any time. </summary>
		private readonly object serialLock = new object();

		/// <summary>The serial port used to communicate using hardware. </summary>
		private SerialPort serial;

		/// <summary>Indicates if the serial port is opened.</summary>
		public bool IsOpen { get { lock (serialLock) { return serial.IsOpen; } } }

		#region Events and Handlers
		/// <summary>Fires when the serial port is either connected or disconnected.</summary>
		public event ConnectionChangedHandler OnConnectionChanged;
		public delegate void ConnectionChangedHandler(bool IsConnected, string PortName); 
		#endregion

		/// <summary>Initializes the settings that will be used to open a new connection.</summary>
		/// <param name="baud">The baud rate.</param>
		/// <param name="parity">The parity. Defaults to none.</param>
		/// <param name="stopBits">The number of stop bits. Defaults to one.</param>
		public SerialCommunicator(int baud = 57600, Parity parity = Parity.None, StopBits stopBits = StopBits.One) {
			serial = new SerialPort("null", baud, parity, 8, stopBits);
			serial.NewLine = ">";
			serial.ReadTimeout = 500;
			serial.WriteTimeout = 500;
			serial.Encoding = Encoding.ASCII;
			serial.Disposed += closeEvent;
			serial.ErrorReceived += closeEvent; //If any errors occur, close the port.
		}

		~SerialCommunicator() {
			Dispose();
		}

		public void Dispose() {
			if (serial != null) serial.Close();
		}

		//Fires when the serial connection encounters an error.
		private void closeEvent(object sender, EventArgs args) {
			lock (serialLock) {
				if (serial.IsOpen) Close(); OnConnectionChanged(false, null);
			}
		}

		/// <summary>Closes the serial port and fires OnConnectionChanged event.</summary>
		public void Close() {
			lock (serialLock) {
				serial.Close(); //Also throws a closeEvent?
				//OnConnectionChanged(false, null);
			}
		}

		/// <summary>Attempts to open a serial connection with the given port name. In general, e.x. COM27</summary>
		/// <param name="portName">The name of the port to open.</param>
		/// <returns>true if opened.</returns>
		public bool Open(String portName) {
			lock (serialLock) {
				if (serial.IsOpen) Close(); //If a connectioj is already open, close it first.
				serial.PortName = portName;
				try { //Attempt to open the port.
					serial.Open();
					if (!serial.IsOpen) return false;
					
					//Attempt to clear out bad data from the port.
					do {
						Thread.Sleep(50);
						serial.DiscardInBuffer();
						serial.DiscardOutBuffer();
					} while (serial.BytesToRead > 0 || serial.BytesToWrite > 0);
					
					OnConnectionChanged(true, portName);
					return true;
				} catch (Exception e) { //Could not connect to this specific port.
					Console.Error.WriteLine("\nCould not connect to device: " + e.Message);
					Console.Error.WriteLine(e.StackTrace);
					Console.Error.WriteLine();
					return false;
				}
			}
		}

		/// <summary>Attempts to find a device that contains the specified name and connect to it. If multiple devices contain this name, no gurantees on which order it will try to connect to them.
		/// This name should be based off the driver/device name found in DeviceManager.</summary>
		/// <param name="deviceName">The name to look for contained in available port names.</param>
		/// <returns>true if connected.</returns>
		public bool AutoConnect(string deviceName) {
			lock (serialLock) { //Not necessary, but will stop communication while looking for a connection.
				try {
					if (serial.IsOpen) Close(); //If a connection is already open, close it.
					ManagementObjectCollection manObjReturn = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0").Get();

					foreach (ManagementObject manObj in manObjReturn) {
						//string desc = manObj["Description"].ToString();
						//string deviceId = manObj["DeviceID"].ToString();
						object nameObject = manObj["Name"]; //Gets the name object of an available port.
						if (nameObject == null) continue;
						string name = nameObject.ToString(); //Converts the name to a string.
						if (name.Contains(deviceName) && name.Contains("(COM")) { //Ensures the device has a com port.
							int comStart = name.LastIndexOf("(COM") + 1; //Finds the start of the port name
							int comEnd = name.Substring(comStart).IndexOf(")"); //Tries to find the end of the port name
							if (comEnd < 0) continue; //Could not find other end of COM name
							comEnd += comStart; //Makes it an absolute position
							string portName = name.Substring(comStart, comEnd - comStart); //Stores the entire port name in a string

							return Open(portName); //Attemps to connect.
						}
					}

					return false;
				} catch(Exception e) {
					Console.Error.WriteLine("Error searching for available ports: " + e.Message);
					Console.Error.WriteLine(e.StackTrace);
					Console.Error.WriteLine();
					return false;
				}
			}
		}

		/// <summary>Sends a command over serial using "The Very Special Format" format.</summary>
		/// <param name="cmd">The command to send.</param>
		/// <returns>The object returned by the sent serial command. Method also returns null for errors.</returns>
		public object SendCommand(ISerialCommand cmd) {
			lock (serialLock) {
				if (!serial.IsOpen) return null;

				//Get the command name and check if it is valid.
				string command = cmd.GetCommand(); 
				if (command == null) throw new ArgumentNullException("Can not send a null command.");

				//Parses the arguments into the correct format at checks if they are valid.
				string commandArgs = ""; 
				string[] args = cmd.GetArguments();
				if (args != null) {
					foreach (string arg in args) {
						if (arg == null) throw new ArgumentNullException("Can not send a null argument.");
						commandArgs += arg + ",";
					}
				}

				//Send the command in its full "The Very Special Format" format.
				SendString("<" + command + ";" + commandArgs + ">");

				//Attempt to read the command response.
				string response = ReadLine();
				if (response == null) return null;

				//Check if response is in the correct format. If not, assume a bad serial connection and disconnect.
				if (!response.StartsWith("<" + command + ";")) {
					Close(); 
					return null;
				}

				//A response has been read, now parse the returned parameters into an array.
				string paramString = response.Substring(command.Length + 2);
				paramString = paramString.TrimEnd(',');
				string[] parameters = paramString.Split(',');

				//Send the parameters to the given serial command and return its response to the caller.
				return cmd.OnSerialResponse(this, parameters);
				
			}
		}

		/// <summary> Sends a string without "The Very Special Format". Used in some cases to prevent timeouts. </summary>
		/// <param name="str"> The string to send. </param>
		/// <returns> true if no errors occured. </returns>
		public bool SendString(string str) {
			lock (serialLock) {
				if (!serial.IsOpen) return false;
				try {
					serial.Write(str); 
					return true;
				} catch (ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return false;
				} catch (InvalidOperationException e) {
					Console.Error.WriteLine("\nTried writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return false;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return false;
				}
			}
		}

		/// <summary> Reads a single character from serial. Returns null if an error occured.</summary>
		/// <returns></returns>
		public byte? ReadChar() {
			lock (serialLock) {
				if (!serial.IsOpen) return null;
				try {
					int data = serial.ReadByte(); 
					if (data < 0) throw new EndOfStreamException("Data returned negative, probably null data.");
					return (byte)data;
				} catch (ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				} catch (InvalidOperationException e) {
					Console.Error.WriteLine("\nTried writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				}
			}
		}

		/// <summary>Reads a line of text from serial, seperated by the "The Very Special Format"'s terminator character (>)</summary>
		/// <returns>Returns null if an error occured, otherwise the string read.</returns>
		public string ReadLine() {
			lock (serialLock) {
				if (!serial.IsOpen) return null;
				try {
					string data = serial.ReadLine();
					return data;
				} catch (ArgumentNullException e) {
					Console.Error.WriteLine("\nError writing to serial port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				} catch (InvalidOperationException e) {
					Console.Error.WriteLine("\nTried writing to a closed port: " + e.Message);
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				} catch (TimeoutException e) {
					Console.Error.WriteLine("\nA timeout occured while writing/reading serial port: " + e.Message);
					Console.WriteLine("The serial port will be closed.");
					Console.Error.WriteLine(e.StackTrace + '\n');
					Close();
					return null;
				}
			}
		}

		/// <summary> Returns an array of all currently available serial ports. </summary>
		/// <returns></returns>
		public static String[] GetAvailablePorts() {
			return SerialPort.GetPortNames();
		}
	}
}
