
namespace RobotArmUR2.Util.Serial {

	/// <summary>A command to be sent using "The Very Special Format" format. 
	/// Also allows the chance to parse return data and return to command caller in the form of a data object (e.x. int)</summary>
	public interface ISerialCommand {

		/// <summary>Returns a string of the command to be sent over serial.</summary>
		/// <returns></returns>
		string GetCommand(); 

		/// <summary>Returns the arguments to be sent appended to the command.</summary>
		/// <returns></returns>
		string[] GetArguments();

		/// <summary>Returns the name of the command for debugging purposes.</summary>
		/// <returns></returns>
		string GetName();

		/// <summary> When a sucessfull response is recieved, this function is fired with the data contained in the response.</summary>
		/// <param name="serial">Serial class that caused the event.</param>
		/// <param name="parameters">Data contained in the response.</param>
		/// <returns>Any data to be returned to caller.</returns>
		object OnSerialResponse(SerialCommunicator serial, string[] parameters);

	}
}
