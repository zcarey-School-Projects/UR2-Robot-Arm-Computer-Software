using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotHelpers.Serial {
	public abstract class SerialCommand {

		public abstract object OnSerialResponse(SerialCommunicator serial, SerialResponse response);

		protected static byte[] ToAscii(string str) {
			byte[] data = new byte[str.Length];
			for(int i = 0; i < str.Length; i++) {
				data[i] = (byte)str[i];
			}

			return data;
		}

		/// <summary>
		/// Returns a new array that combines the two given byte arrays.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		protected static byte[] CombineByteArrays(byte[] first, byte[] second) {
			int len = first.Length + second.Length;
			/*if (len > 255) {
				Console.WriteLine("WARNING: Command length exceeds 255 bytes, clipping data: " + GetName());
				len = 255;
			}*/
			int len1 = Math.Min(first.Length, len);
			int len2 = Math.Min(second.Length, 255 - first.Length);
			if (len2 < 0) len2 = 0;

			byte[] newArray = new byte[len];
			Array.Copy(first, 0, newArray, 0, len1);
			Array.Copy(second, 0, newArray, len1, len2);
			return newArray;
		}

		/// <summary>
		/// Returns a new array that is a copy of the given array, with the given byte appended.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="append"></param>
		/// <returns></returns>
		protected static byte[] AppendByte(byte[] array, byte append) {
			/*if (array.Length >= 255) {
				Console.WriteLine("WARNING: Command length exceeds 255 bytes, clipping data: " + GetName());
				return array;
			}*/
			byte[] newArray = new byte[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = append;
			return newArray;
		}

		/// <summary>
		/// Returns a string of the command to be sent over serial.
		/// </summary>
		/// <returns></returns>
		public abstract string getCommand(); 

		/// <summary>
		/// Returns the data to be sent appended to the command.
		/// </summary>
		/// <returns></returns>
		public abstract byte[] GetData();

		/// <summary>
		/// Returns the name of the command for debugging purposes.
		/// </summary>
		/// <returns></returns>
		public abstract string GetName();

		#region Data Types to Byte Array Helping Converter Methods
		/// <summary>
		/// Converts the string into an array of bytes (ASCII format).
		/// </summary>
		/// <param name="str"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(string str, int startIndex, int endIndex) {
			if ((endIndex < startIndex) || (startIndex >= str.Length) || endIndex <= 0) return new byte[0];
			if (startIndex < 0) startIndex = 0;
			if (endIndex > str.Length) endIndex = str.Length;
			char[] chars = str.ToCharArray();
			byte[] bytes = new byte[endIndex - startIndex];
			uint byteIndex = 0;
			for (int i = startIndex; i < endIndex; i++) {
				bytes[byteIndex++] = (byte)chars[i];
			}

			return bytes;
		}

		/// <summary>
		/// Converts the string into an array of bytes (ASCII format).
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(string str) {
			return GetBytes(str, 0, str.Length);
		}

		/// <summary>
		/// Converts the given boolean into a single byte (either 0 or 1)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(bool value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given unicode character into one byte (an ASCII character)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(char value) { return new byte[1] { (byte)value }; }

		/// <summary>
		/// Converts the given signed short into two bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(short value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given signed integer into four bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(int value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given signed long into eight bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(long value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given unsigned short into two bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(ushort value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given unsigned integer into four bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(uint value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given unsigned long into eight bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(ulong value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given float into its four byte equivalent.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(float value) { return BitConverter.GetBytes(value); }

		/// <summary>
		/// Converts the given double into its eight byte equivalent.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static byte[] GetBytes(double value) { return BitConverter.GetBytes(value); }

		protected static byte[] ToAscii(sbyte value) { return ToAscii((long)value); }
		protected static byte[] ToAscii(byte value) { return ToAscii((ulong)value); }
		protected static byte[] ToAscii(short value) { return ToAscii((long)value); }
		protected static byte[] ToAscii(ushort value) { return ToAscii((ulong)value); }
		protected static byte[] ToAscii(int value) { return ToAscii((long)value); }
		protected static byte[] ToAscii(uint value) { return ToAscii((ulong)value); }

		protected static byte[] ToAscii(long value) {
			if(value < 0) {
				return CombineByteArrays(new byte[] { (byte)'-' }, ToAscii((ulong)(-value)));
			} else {
				return ToAscii((ulong)value);
			}
		}

		protected static byte[] ToAscii(ulong value) {
			string ascii = "";

			do {
				byte c = (byte)(value % 10);
				value /= 10;
				ascii = c + ascii;
			} while (value > 0);

			return GetBytes(ascii);
		}
		#endregion

	}
}
