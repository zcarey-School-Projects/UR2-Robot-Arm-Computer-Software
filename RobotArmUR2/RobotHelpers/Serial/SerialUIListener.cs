using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotHelpers.Serial {
	public interface SerialUIListener {

		void SerialOnConnectionChanged(bool isConnected, string portName);

	}
}
