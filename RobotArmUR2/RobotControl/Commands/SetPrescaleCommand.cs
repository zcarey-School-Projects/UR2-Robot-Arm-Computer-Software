using System;
using System.Collections.Generic;
using RobotArmUR2.Util.Serial;

namespace RobotArmUR2.RobotControl.Commands {

	/// <summary>Sets the speed of a motor by changing its prescale value.</summary>
	class SetPrescaleCommand : ISerialCommand {

		private byte? basePrescale;
		private byte? carriagePrescale;

		/// <summary>Prescales can't exceed 20. Give null if don;t wish to change.</summary>
		/// <param name="BasePrescale"></param>
		/// <param name="CarriagePrescale"></param>
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

		/// <summary>Prescales can't exceed 20. Give null if don;t wish to change.</summary>
		/// <param name="BasePrescale"></param>
		/// <param name="CarriagePrescale"></param>
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
