using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;

namespace RobotHelpers.InputHandling {
	public class VideoInput : FileInput{

		private BinaryReader reader;
		private int width = 0;
		private int height = 0;
		private byte[,,] buffer = null;
		private bool frameAvailable = false;

		public VideoInput() : base() { }
		public VideoInput(String filename) : base(filename) { }

		protected override void onDispose() {
			if(reader != null) {
				reader.Close();
				reader.Dispose();
			}

			reader = null;
			buffer = null;
		}

		protected override int getDelayMS() {
			return 33;
		}

		protected override bool isNextFrameAvailable() {
			return frameAvailable;
		}

		protected override Image<Bgr, byte> readFrame() {
			if (frameAvailable) {
				reader.ReadDouble(); //Dispose of timing info.

				for(int channel = 0; channel < 3; channel++) {
					for(int y = 0; y < height; y++) {
						for(int x = 0; x < width; x++) {
							buffer[y, x, channel] = reader.ReadByte();
						}
					}
				}

				frameAvailable = reader.ReadBoolean();
				if (!frameAvailable) {
					reader.Close();
					reader.Dispose();
					reader = null;
				}

				return new Image<Bgr, byte>(buffer);
			}

			return null;
		}

		public override int GetWidth() {
			return width;
		}

		public override int GetHeight() {
			return height;
		}

		protected override string getDialogFileExtensions() {
			return "RawCV Video (*.rawcv)|*.rawcv";
		}

		protected override bool setFile(String path) {
			if (File.Exists(path)) {
				String extension = Path.GetExtension(path);
				if (extension == ".rawcv") {
					readRawCVFile(path);
				}
			} else {
				base.printDebugMsg("Could not find file: " + path);
			}

			return false;
		}

		private bool readRawCVFile(String path) {
			BinaryReader fileReader = null;

			try {
				fileReader = new BinaryReader(File.OpenRead(path));
				int fileWidth = reader.ReadInt32();
				int fileHeight = reader.ReadInt32();
				byte[,,] fileBuffer = new byte[height, width, 3];
				bool fileFrameAvailable = reader.ReadBoolean();

				if (fileFrameAvailable) {
					width = fileWidth;
					height = fileHeight;
					buffer = fileBuffer;
					frameAvailable = fileFrameAvailable;

					return true;
				}

				return false;
			} catch {
				if (fileReader != null) fileReader.Dispose();
				return false;
			}
		}

	}
}
