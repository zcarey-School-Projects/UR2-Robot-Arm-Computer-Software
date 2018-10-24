using RobotHelpers.InputHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotHelpers.InputHandling {
	public abstract class FileInput : InputHandler{

		protected static OpenFileDialog dialog;

		static FileInput() {
			dialog = new OpenFileDialog();
			dialog.RestoreDirectory = true;
		}

		protected FileInput() : base() {}

		protected FileInput(String filename) : base() {
			LoadFromFile(filename);
		}

		///<summary>
		///<para>Sets the input to a local file.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		public bool LoadFromFile(String path) {
			lock (inputLock) {
				return setFile(path);
			}
		}

		///<summary>
		///<para>Sets the input to a local file.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		public abstract bool setFile(String path);

		///<summary>
		///<para>Returns the possible file extensions for an OpenFileDialog.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		protected abstract String getDialogFileExtensions();

		///<summary>
		///<para>Opens a dialog for the user to select an input file to load.</para>
		///</summary>
		///<returns>Input was loaded. If not, previous input is kept.</returns>
		public bool PromptUserToLoadFile() {//TODO make thread safe
			dialog.Filter = getDialogFileExtensions();
			if (dialog.ShowDialog() == DialogResult.OK) {
				String path = dialog.FileName;
				return LoadFromFile(path);
			}

			return false;
		}

	}
}
