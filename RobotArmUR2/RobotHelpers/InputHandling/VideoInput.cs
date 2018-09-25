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
	public class VideoInput : InputHandler{

		private BinaryReader reader;
		private int width = 0;
		private int height = 0;
		private byte[,,] buffer = null;
		private bool frameAvailable = false;

		public VideoInput(String path) : base() {
			setFile(path);
		}

		public override void Dispose() {
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

		public override int getWidth() {
			return width;
		}

		public override int getHeight() {
			return height;
		}

		public override bool requestLoadInput() {
			dialog.Filter = "RawCV Video (*.rawcv)|*.rawcv";
			if(dialog.ShowDialog() == DialogResult.OK) {
				String path = dialog.FileName;
				return setFile(path);
			}

			return false;
		}

		public override bool setFile(string path) {
			Dispose();
			if (File.Exists(path)) {
				reader = new BinaryReader(File.OpenRead(path));
				width = reader.ReadInt32();
				height = reader.ReadInt32();
				buffer = new byte[height, width, 3];
				frameAvailable = reader.ReadBoolean();
				return true;
			} else {
				return false;
			}
		}

	}
}
