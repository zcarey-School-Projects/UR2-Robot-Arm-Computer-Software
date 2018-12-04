using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2.Util {
	public class EmguPictureBox<TColor, TDepth> where TColor : struct, IColor where TDepth : new() {

		private static readonly object pictureLock = new object();
		private Form invoker;
		private PictureBox picture;

		private Image<TColor, TDepth> image;
		public Image<TColor, TDepth> Image {
			get => image;
			set {
				//lock (pictureLock) {
					image = value;

					if (invoker.IsHandleCreated) {
						invoker.BeginInvoke(new Action(() => {
							if (value == null) picture.Image = null;
							else picture.Image = value.Bitmap;
						}));
					} else {
						if (value == null) picture.Image = null;
						else picture.Image = value.Bitmap;
					}
				///}
			}
		}

		public EmguPictureBox(Form form, PictureBox picture) {
			this.invoker = form;
			this.picture = picture;
			picture.SizeMode = PictureBoxSizeMode.Zoom;
		}
		
		public PointF? GetRelativeImagePoint(Point MousePoint) {
			//return GetRelativeImagePoint(image, MousePoint);
			lock (pictureLock) { //make sure sizes dont change while we are doing the calculation
				if (image == null) return null;
				if (picture.Width == 0 || picture.Height == 0 || image.Width == 0 || image.Height == 0) return null;

				float PictureAspect = (float)picture.Width / picture.Height;
				float ImgAspect = (float)image.Width / image.Height;

				int scaledWidth = picture.Width;
				int scaledHeight = picture.Height;

				if (ImgAspect > PictureAspect) scaledHeight = (int)(picture.Width / ImgAspect);
				else scaledWidth = (int)(picture.Height * ImgAspect);

				Size relativePos = new Size((picture.Width - scaledWidth) / 2, (picture.Height - scaledHeight) / 2);
				Point pos = Point.Subtract(MousePoint, relativePos);

				return new PointF((float)pos.X / (scaledWidth - 1), (float)pos.Y / (scaledHeight - 1));
			}
		}
		/*
		public PointF? GetRelativeImagePoint(Image<TColor, TDepth> img, Point MousePoint) {
			lock (pictureLock) { //make sure sizes dont change while we are doing the calculation
				if (img == null) return null;
				if (picture.Width == 0 || picture.Height == 0 || img.Width == 0 || img.Height == 0) return null;

				float PictureAspect = (float)picture.Width / picture.Height;
				float ImgAspect = (float)img.Width / img.Height;

				int scaledWidth = picture.Width;
				int scaledHeight = picture.Height;

				if (ImgAspect > PictureAspect) scaledHeight = (int)(picture.Width / ImgAspect);
				else scaledWidth = (int)(picture.Height * ImgAspect);

				Size relativePos = new Size((picture.Width - scaledWidth) / 2, (picture.Height - scaledHeight) / 2);
				Point pos = Point.Subtract(MousePoint, relativePos);

				return new PointF((float)pos.X / scaledWidth, (float)pos.Y / scaledHeight);
			}
		}*/

		public Point? GetImagePoint(Point MousePoint) {
			lock (pictureLock) {
			//Image<TColor, TDepth> img = image;
			//PointF? hit = GetRelativeImagePoint(img, MousePoint);//TODO dirty class
			PointF? hit = GetRelativeImagePoint(MousePoint);
				if (hit == null) return null;
				PointF pos = (PointF)hit;
				return new Point((int)(pos.X * (image.Width - 1)), (int)(pos.Y * (image.Height - 1)));
			}
		}

	}
}
