using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2.Util {

	/// <summary>Wraps a standard PictureBox to allow easy thread-safe setting of its image, and also being able to calculate click coordinates.
	/// NOTE: Automatically sets SizeMode to zoom to fit image in picturebox. Changing this after initialize this class will cause click position calculations to be wrong.</summary>
	public class EmguPictureBox { 

		/// <summary>The form the PictureBox is located in to ensure image setting is thread safe.</summary>
		private Form invoker;

		/// <summary>The PictureBox that is being wrapped.</summary>
		private PictureBox picture;

		/// <summary>The image the PictureBox is currently displaying.</summary>
		public Image<Bgr, byte> Image{
			get => image;
			set {
				image = value;

				if (invoker.IsHandleCreated) { //If the form hasn't been created, don't try and invoke the image because we will get an error.
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
		private Image<Bgr, byte> image;

		/// <summary>Wrap the PictureBox with the Form it is created from.</summary>
		/// <param name="form">Form where the PictureBox is created.</param>
		/// <param name="picture">The PictureBox to be wrapped.</param>
		public EmguPictureBox(Form form, PictureBox picture) {
			this.invoker = form ?? throw new ArgumentNullException("Can't have a null Form as an argument.");
			this.picture = picture ?? throw new ArgumentNullException("Can't have a null PictureBox as an argument.");
			picture.SizeMode = PictureBoxSizeMode.Zoom;
		}
		

		/// <summary>Returns the relative coordinates (i.e. all coordinates between [0, 1]) of where the mouse clicked the picturebox.</summary>
		/// <param name="MousePoint">Point where the mouse clicked.</param>
		/// <returns></returns>
		public PointF? GetRelativeImagePoint(Point MousePoint) { //TODO create event that essentially "overloads" wrapped picturebox onClick event
			Image<Bgr, byte> image = this.image; //Thread-safe grab of the iamge.
			if (image == null) return null; //Error checks
			if (picture.Width == 0 || picture.Height == 0 || image.Width == 0 || image.Height == 0) return null;

			//Calculate the aspect ratio of both the image and the picturebox
			float PictureAspect = (float)picture.Width / picture.Height;
			float ImgAspect = (float)image.Width / image.Height;

			//Calculate the scaled size of the picturebox based on the aspect ratio.
			//Since we are using zoom mode, if the picturebox is longer than the iamge you get the "black bars" on the left and right of the image
			//Here, we are calculating the "length" of the picturebox that touches the image on the axis that has the black bars.
			int scaledWidth = picture.Width;
			int scaledHeight = picture.Height;
			if (ImgAspect > PictureAspect) scaledHeight = (int)(picture.Width / ImgAspect);
			else scaledWidth = (int)(picture.Height * ImgAspect);

			//Calculate the relative position compared to the image using the scaled size.
			Size relativePos = new Size((picture.Width - scaledWidth) / 2, (picture.Height - scaledHeight) / 2);
			Point pos = Point.Subtract(MousePoint, relativePos);

			return new PointF((float)pos.X / (scaledWidth - 1), (float)pos.Y / (scaledHeight - 1));
		}

		/// <summary>Returns the pixel coordinate on the image where the mouse clicked. </summary>
		/// <param name="MousePoint">Where the mouse clicked.</param>
		/// <returns></returns>
		public Point? GetImagePoint(Point MousePoint) { //TODO this isn't actually thread safe, is it?
			PointF? hit = GetRelativeImagePoint(MousePoint);
			if (hit == null) return null;
			PointF pos = (PointF)hit;
			return new Point((int)(pos.X * (image.Width - 1)), (int)(pos.Y * (image.Height - 1)));
		}

	}
}
