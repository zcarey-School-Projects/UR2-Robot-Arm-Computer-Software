using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotHelpers {
	public static class ControlHelper {
		public static void InvokeIfRequired<T>(this T control, Action<T> action) where T : ISynchronizeInvoke {
			if (control.InvokeRequired) {
				//control.Invoke(new Action(() => action(control)), null);
				control.BeginInvoke(new Action(() => action(control)), null);
			} else {
				action(control);
			}
		}

		public static void InvokeIfRequired<T>(this T control, Form invokeThread, Action<T> action) where T : Component {
			if (invokeThread.InvokeRequired) {
				invokeThread.BeginInvoke(new Action(() => action(control)), null);
			} else {
				action(control);
			}
		}
	}
}
