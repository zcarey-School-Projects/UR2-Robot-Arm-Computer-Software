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

		public FileInput() {

		}

		public FileInput(String filename) {
			LoadFromFile(filename);
		}

		///<summary>
		///<para>Sets the input to a local file.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		public bool LoadFromFile(String path) {
			lock (inputLock) {
				bool result = setFile(path);
				printDebugMsg((result ? "Successfully loaded file: " : "Could not load file: ") + path);
				return result;
			}
		}

		///<summary>
		///<para>Sets the input to a local file.</para>
		///<param name="filename">File name of the local file to load.</param>
		///</summary>
		///<returns>File was loaded.</returns>
		protected abstract bool setFile(String path);

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
