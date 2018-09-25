using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotHelpers.Serial {
	public class SerialResponse {

		private byte[] bytes;

		public byte[] Data { get { return bytes; } }

		public SerialResponse(ref byte[] data) {
			this.bytes = data;
		}

		#region Convert Bytes to Data Types Helpful Methods
		/// <summary>
		/// Converts one byte into a boolean expression.
		/// </summary>
		/// <returns></returns>
		public bool ToBool() { return BitConverter.ToBoolean(bytes, bytes.Length - 1); }

		/// <summary>
		/// Returns one byte.
		/// </summary>
		/// <returns></returns>
		public byte ToByte() { return bytes[bytes.Length - 1]; }

		/// <summary>
		/// Returns a signed 16-bit number from two bytes.
		/// </summary>
		/// <returns></returns>
		public short ToInt16() { return BitConverter.ToInt16(bytes, bytes.Length - 2); }

		/// <summary>
		/// Returns a signed 32-bit number from four bytes.
		/// </summary>
		/// <returns></returns>
		public int ToInt32() { return BitConverter.ToInt32(bytes, bytes.Length - 4); }

		/// <summary>
		/// Returns a signed 64-bit number from eight bytes.
		/// </summary>
		/// <returns></returns>
		public long ToInt64() { return BitConverter.ToInt64(bytes, bytes.Length - 8); }

		/// <summary>
		/// Returns an unsigned 16-bit number from two bytes.
		/// </summary>
		/// <returns></returns>
		public ushort ToUInt16() { return BitConverter.ToUInt16(bytes, bytes.Length - 2); }

		/// <summary>
		/// Returns an unsigned 32-bit number from four bytes.
		/// </summary>
		/// <returns></returns>
		public uint ToUInt32() { return BitConverter.ToUInt32(bytes, bytes.Length - 4); }

		/// <summary>
		/// Returns an unsigned 64-bit number from eight bytes.
		/// </summary>
		/// <returns></returns>
		public ulong ToUInt64() { return BitConverter.ToUInt64(bytes, bytes.Length - 8); }

		/// <summary>
		/// Returns a single precision float from four bytes.
		/// </summary>
		/// <returns></returns>
		public float ToFloat() { return BitConverter.ToSingle(bytes, bytes.Length - 4); }

		/// <summary>
		/// Returns a double precision float from eight bytes.
		/// </summary>
		/// <returns></returns>
		public double ToDouble() { return BitConverter.ToDouble(bytes, bytes.Length - 8); }

		/// <summary>
		/// Returns a single ASCII (8-bit) character from a single byte.
		/// </summary>
		/// <returns></returns>
		public char ToChar() { return (char)ToByte(); }

		/// <summary>
		/// Converts the bytes into an ASCII string.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			string str = "";
			foreach (byte b in bytes) {
				str += (char)b;
			}

			return str;
		}
		#endregion

	}
}
