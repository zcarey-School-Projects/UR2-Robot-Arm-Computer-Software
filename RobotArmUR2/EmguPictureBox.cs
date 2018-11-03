using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public class EmguPictureBox<TColor, TDepth> where TColor : struct, IColor where TDepth : new() {

		private static readonly object pictureLock = new object();
		private Form invoker;
		private PictureBox picture;

		private Image<TColor, TDepth> image;
		public Image<TColor, TDepth> Image {
			get {
				lock (pictureLock) { return image; }
			}
			set {
				lock (pictureLock) {
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
				}
			}
		}

		public EmguPictureBox(Form form, PictureBox picture) {
			this.invoker = form;
			this.picture = picture;
			picture.SizeMode = PictureBoxSizeMode.Zoom;
		}

		public PointF? GetRelativeImagePoint(Point MousePoint) {
			lock (pictureLock) {
				if (image == null) return null;

				float PictureAspect = (float)picture.Width / picture.Height;
				float ImgAspect = (float)image.Width / image.Height;
				if (ImgAspect > PictureAspect) {
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
				}
			}
		}

		public Point? GetImagePoint(Point MousePoint) {
			lock (pictureLock) {
				PointF? hit = GetRelativeImagePoint(MousePoint);
				if (hit == null) return null;
				PointF pos = (PointF)hit;
				return new Point((int)(pos.X * image.Width), (int)(pos.Y * image.Height));
			}
		}

	}
}
