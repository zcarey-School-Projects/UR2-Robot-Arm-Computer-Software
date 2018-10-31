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
	public class ImageInput : FileInput{

		private int width = 0;
		private int height = 0;
		private Image<Bgr, byte> imageBuffer;

		public ImageInput() : base() { }
		public ImageInput(String filename) : base(filename) { }

		protected override void onDispose() {
			width = 0;
			height = 0;
			imageBuffer = null;
		}

		protected override int getDelayMS() {
			return 67;
		}

		protected override bool isNextFrameAvailable() {
			return (imageBuffer != null);
		}

		protected override Image<Bgr, byte> readFrame() {
			if(imageBuffer != null) {
				return imageBuffer.Clone();
			} else {
				return null;
			}
		}

		public override int GetWidth() { return width; }

		public override int GetHeight() { return height; }

		protected override string getDialogFileExtensions() {
			return "Image Files (*.bmp, *.gif, *.jpeg, *.jpg, *.exif, *.png, *.tiff, *.rawcvimg)|*.bmp;*.gif;*.jpeg;*.jpg;*.exif;*.png;*.tiff;*.rawcvimg" +
				"|BMP (*.bmp)|*.bmp" +
				"|GIF (*.png)|*.png" +
				"|JPEG (*.jpeg, *.jpg)|*.jpeg;*.jpg" +
				"|EXIF (*.exif)|*.exif" +
				"|PNG (*.png)|*.png" +
				"|TIFF (*.tiff)|*.tiff" +
				"|RawCV Image (*.rawcvimg)|*.rawcvimg";
		}

		protected override bool setFile(String path) {
			if (File.Exists(path)) {
				String extension = Path.GetExtension(path);
				switch (extension) {
					case ".rawcvimg": return readRawCVImage(path);
					/*case ".bmp":
					case ".gif":
					case ".jpeg":
					case ".jpg":
					case ".exif":
					case ".png":
					case ".tiff":*/
					//return readGenericImage(path);
					//default: return false;
					default: return readGenericImage(path);
				}
			} else {
				base.printDebugMsg("Could not find file: " + path);
			}

			return false;
		}

		//This functions assumes that it is confirmed that the given path is a raw CV image type, and file exists.
		private bool readGenericImage(String path) {
			Bitmap img = null;

			try {
				img = new Bitmap(path);
				int fileWidth = img.Width;
				int fileHeight = img.Height;
				byte[,,] buffer = new byte[fileHeight, fileWidth, 3];

				for (int y = 0; y < fileHeight; y++) {
					for (int x = 0; x < fileWidth; x++) {
						Color pixel = img.GetPixel(x, y);
						buffer[y, x, 0] = pixel.B;
						buffer[y, x, 1] = pixel.G;
						buffer[y, x, 2] = pixel.R;
					}
				}

				imageBuffer = new Image<Bgr, byte>(buffer);
				width = fileWidth;
				height = fileHeight;

				return true;
			} catch {
				return false;
			} finally {
				if (img != null) img.Dispose();
			}
		}

		//This functions assumes that it is confirmed that the given path is a raw CV image type, and file exists.
		private bool readRawCVImage(String path) {
			BinaryReader reader = new BinaryReader(File.OpenRead(path));

			try {
				int fileWidth = reader.ReadInt32();
				int fileHeight = reader.ReadInt32();
				byte[,,] buffer = new byte[fileHeight, fileWidth, 3];

				for (int channel = 0; channel < 3; channel++) {
					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++) {
							buffer[y, x, channel] = reader.ReadByte();
						}
					}
				}

				imageBuffer = new Image<Bgr, byte>(buffer); 
				width = fileWidth;
				height = fileHeight;
				
				return true;
			} catch {
				return false;
			} finally {
				reader.Close();
				reader.Dispose();
			}
		}

	}
}
