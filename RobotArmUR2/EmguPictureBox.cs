using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2 {
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
			return GetRelativeImagePoint(image, MousePoint);
		}

		public PointF? GetRelativeImagePoint(Image<TColor, TDepth> img, Point MousePoint) {
			//TODO clean if works
			//lock (pictureLock) {
			//Image<TColor, TDepth> img = image;
				if (img == null) return null;
				if (picture.Width == 0 || picture.Height == 0 || img.Width == 0 || img.Height == 0) return null;

				float PictureAspect = (float)picture.Width / picture.Height;
				float ImgAspect = (float)img.Width / img.Height;
			/*if (ImgAspect > PictureAspect) {
				int scaledHeight = (int)(picture.Width / ImgAspect);
				int yPos = (picture.Height - scaledHeight) / 2;
				Point pos = new Point(MousePoint.X, MousePoint.Y - yPos);
				if ((pos.X < 0) || (pos.Y < 0) || (pos.X >= picture.Width) || (pos.Y >= scaledHeight)) return null;
				return new PointF((float)pos.X / picture.Width, (float)pos.Y / scaledHeight);
			} else {
				int scaledWidth = (int)(ImgAspect * picture.Height);
				int xPos = (picture.Width - scaledWidth) / 2;
				Point pos = new Point(MousePoint.X - xPos, MousePoint.Y);
				if ((pos.X < 0) || (pos.Y < 0) || (pos.X >= scaledWidth) || (pos.Y >= picture.Height)) return null;
				return new PointF((float)pos.X /scaledWidth, (float)pos.Y / picture.Height); ;
			}*/
			//}
			//does not check if point is out of bounds
			lock (pictureLock) { //make sure sizes dont change while we are doing the calculation
				int scaledWidth = picture.Width;
				int scaledHeight = picture.Height;

				if (ImgAspect > PictureAspect) scaledHeight = (int)(picture.Width / ImgAspect);
				else scaledWidth = (int)(picture.Height * ImgAspect);

				Size relativePos = new Size((picture.Width - scaledWidth) / 2, (picture.Height - scaledHeight) / 2);
				Point pos = Point.Subtract(MousePoint, relativePos);

				return new PointF((float)pos.X / scaledWidth, (float)pos.Y / scaledHeight);
			}

		}

		public Point? GetImagePoint(Point MousePoint) {
			//lock (pictureLock) {
			Image<TColor, TDepth> img = image;
				PointF? hit = GetRelativeImagePoint(img, MousePoint);
				if (hit == null) return null;
				PointF pos = (PointF)hit;
				return new Point((int)(pos.X * img.Width), (int)(pos.Y * img.Height));
			//}
		}

	}
}
