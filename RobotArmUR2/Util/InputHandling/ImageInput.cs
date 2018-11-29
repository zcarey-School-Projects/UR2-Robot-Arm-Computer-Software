using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace RobotHelpers.InputHandling {
	public class ImageInput : FileInput{

		private Image<Bgr, byte> imageBuffer;

		public ImageInput() : base() { }
		public ImageInput(String filename) : base(filename) { }

		protected override void onDispose() {
			if (imageBuffer != null) imageBuffer.Dispose();
			imageBuffer = null;
		}

		protected override int getDelayMS() {
			return 67; //TODO add to property
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

		public override int GetWidth() {
			if (imageBuffer == null) return 0;
			return imageBuffer.Width;
		}

		public override int GetHeight() {
			if (imageBuffer == null) return 0;
			return imageBuffer.Height;
		}

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
				if (extension == ".rawcvimg") return readRawCVImage(path);
				else return readGenericImage(path);
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
				int width = img.Width;
				int height = img.Height;
				byte[,,] buffer = new byte[height, width, 3];

				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						Color pixel = img.GetPixel(x, y);
						buffer[y, x, 0] = pixel.B;
						buffer[y, x, 1] = pixel.G;
						buffer[y, x, 2] = pixel.R;
					}
				}

				imageBuffer = new Image<Bgr, byte>(buffer);

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
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				byte[,,] buffer = new byte[height, width, 3];

				for (int channel = 0; channel < 3; channel++) {
					for (int y = 0; y < height; y++) {
						for (int x = 0; x < width; x++) {
							buffer[y, x, channel] = reader.ReadByte();
						}
					}
				}

				imageBuffer = new Image<Bgr, byte>(buffer); 
				
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
