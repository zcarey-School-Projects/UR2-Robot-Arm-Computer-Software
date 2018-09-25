using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;
using System.Drawing;

namespace RobotHelpers.InputHandling {
	public class ImageInput : InputHandler{

		private int width = 0;
		private int height = 0;
		private byte[,,] buffer;
		private Image<Bgr, byte> imageBuffer;

		public ImageInput(String path) : base() {
			setFile(path);
		}

		public override void Dispose() {
			width = 0;
			height = 0;
			imageBuffer = null;
			buffer = null;
		}

		protected override int getDelayMS() {
			return 67;
		}

		protected override bool isNextFrameAvailable() {
			return (imageBuffer != null);
		}

		protected override Image<Bgr, byte> readFrame() {
			if(imageBuffer != null) {
				return imageBuffer;
			} else {
				return null;
			}
		}

		public override int getWidth() { return width; }

		public override int getHeight() { return height; }

		public override bool requestLoadInput() {
			dialog.Filter = "Image Files (*.png, *.jpg, *.rawcvimg)|*.png;*.jpg;*.rawcvimg|RawCV Image (*.rawcvimg)|*.rawcvimg|PNG (*.png)|*.png|JPG (*.jpg)|*.jpg";
			if (dialog.ShowDialog() == DialogResult.OK) {
				String path = dialog.FileName;
				return setFile(path);
			}

			return false;
		}

		public override bool setFile(String path) {
			if (File.Exists(path)) {
				String extension = Path.GetExtension(path);
				if(extension == ".rawcvimg") {
					BinaryReader reader = new BinaryReader(File.OpenRead(path));
					width = reader.ReadInt32();
					height = reader.ReadInt32();
					buffer = new byte[height, width, 3];

					for (int channel = 0; channel < 3; channel++) {
						for (int y = 0; y < height; y++) {
							for (int x = 0; x < width; x++) {
								buffer[y, x, channel] = reader.ReadByte();
							}
						}
					}

					imageBuffer = new Image<Bgr, byte>(buffer);

					reader.Close();
					reader.Dispose();
				}else if (extension == ".png" || extension == ".jpg") {
					Bitmap img = new Bitmap(path);
					width = img.Width;
					height = img.Height;
					buffer = new byte[height, width, 3];
					for(int y = 0; y < img.Height; y++) {
						for(int x = 0; x < img.Width; x++) {
							Color pixel = img.GetPixel(x, y);
							buffer[y, x, 0] = pixel.B;
							buffer[y, x, 1] = pixel.G;
							buffer[y, x, 2] = pixel.R;
						}
					}

					imageBuffer = new Image<Bgr, byte>(buffer);

					img.Dispose();
				} else {
					base.printDebugMsg("Unknown file type: " + extension);
				}

				return true;
			} else {
				base.printDebugMsg("Could not find file: " + path);
			}

			return false;
		}

	}
}
