using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Serial {
	public interface SerialCommand {

		/// <summary>
		/// Returns a string of the command to be sent over serial.
		/// </summary>
		/// <returns></returns>
		string GetCommand(); 

		/// <summary>
		/// Returns the data to be sent appended to the command.
		/// </summary>
		/// <returns></returns>
		string[] GetArguments();

		/// <summary>
		/// Returns the name of the command for debugging purposes.
		/// </summary>
		/// <returns></returns>
		string GetName();

		/// <summary>
		/// When a sucessfull response is recieved, this function is fired with the data contained in the response.
		/// </summary>
		/// <param name="serial">Serial class that caused the event.</param>
		/// <param name="parameters">Data contained in the response.</param>
		/// <returns>Any data to be returned to caller.</returns>
		object OnSerialResponse(SerialCommunicator serial, string[] parameters);

	}
}
