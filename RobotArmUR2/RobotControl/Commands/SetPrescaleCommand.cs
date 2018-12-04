using System;
using System.Collections.Generic;
using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {
	class SetPrescaleCommand : SerialCommand {

		private byte? basePrescale;
		private byte? carriagePrescale;

		public SetPrescaleCommand(byte? BasePrescale, byte? CarriagePrescale) {
			this.basePrescale = BasePrescale;
			this.carriagePrescale = CarriagePrescale;

			if(basePrescale > 20) {
				basePrescale = null;
				Console.Error.WriteLine(GetName() + ": Base Prescale exceeded limit of 20, not sending: " + BasePrescale);
			}
			if(carriagePrescale > 20) {
				carriagePrescale = null;
				Console.Error.WriteLine(GetName() + ": Carriage Prescale exceeded limit of 20, not sending: " + CarriagePrescale);
			}
		}

		public SetPrescaleCommand(byte BasePrescale, byte CarriagePrescale) : this((byte?)BasePrescale, (byte?)CarriagePrescale) { }

		public string GetCommand() {
			return "Prescale";
		}

		public string[] GetArguments() {
			List<string> args = new List<string>();
			if (basePrescale != null) args.Add("B" + ((byte)basePrescale).ToString());
			if (carriagePrescale != null) args.Add("C" + ((byte)carriagePrescale).ToString());
			return args.ToArray();
		}

		public string GetName() {
			return "Set Prescale Command";
		}

		public object OnSerialResponse(SerialCommunicator serial, string[] parameters) {
			return true;
		}
	}
}
