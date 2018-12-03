using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;
using RobotArmUR2.VisionProcessing;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace RobotArmUR2 {
	public partial class PaperCalibrater : Form {

		private PaperPoint draggingPoint = null;
		private EmguPictureBox<Bgr, byte> picture;
		private volatile bool autoDetectPaper = false;
		private volatile bool isOpen = false;

		public PaperCalibrater() {
			InitializeComponent();
			picture = new EmguPictureBox<Bgr, byte>(this, PaperPicture);
		}

		private void PaperCalibrater_Load(object sender, EventArgs e) {
			picture.Image = null;
			isOpen = true;
		}

		private void PaperCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			isOpen = false;
			ApplicationSettings.PaperCalibration.SaveSettings();
		}

		public void NewFrameFinished(Vision vision) {
			if (!isOpen) return; //Only update image if window is open.
			if (autoDetectPaper) {
				autoDetectPaper = false;
				detectPaper(vision);
			}

			Image<Bgr, byte> img = vision.InputImage;
			Image<Bgr, byte> rect = img.CopyBlank();

			Point[] paperPoints = ApplicationSettings.PaperCalibration.ToArray(rect.Size); //{BottomLeft, TopLeft, TopRight, BottomRight}
			rect.FillConvexPoly(paperPoints, ApplicationSettings.PaperROIMaskColor);
			CvInvoke.AddWeighted(img, 1 - ApplicationSettings.PaperROIMaskTransparency, rect, ApplicationSettings.PaperROIMaskTransparency, 0, img);

			foreach (PointF point in ApplicationSettings.PaperCalibration.ToArray(rect.Size.Width, rect.Size.Height)) {
				img.Draw(new CircleF(point, ApplicationSettings.PaperROICircleRadius), ApplicationSettings.PaperROICircleColor, ApplicationSettings.PaperROICircleThickness);
			}

			#region Draw Text next to circles
			PointF textPoint = ApplicationSettings.PaperCalibration.BottomLeft.GetAdjustedCoord(rect.Size.Width, rect.Size.Height);
			textPoint.X += ApplicationSettings.PaperROICircleRadius;
			textPoint.Y -= ApplicationSettings.PaperROICircleRadius;
			img.Draw("Bottom Left", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

			textPoint = ApplicationSettings.PaperCalibration.TopLeft.GetAdjustedCoord(rect.Size.Width, rect.Size.Height);
			textPoint.X += ApplicationSettings.PaperROICircleRadius;
			textPoint.Y += ApplicationSettings.PaperROICircleRadius;
			img.Draw("Top Left", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

			textPoint = ApplicationSettings.PaperCalibration.TopRight.GetAdjustedCoord(rect.Size.Width, rect.Size.Height);
			textPoint.X -= ApplicationSettings.PaperROICircleRadius;
			textPoint.Y += ApplicationSettings.PaperROICircleRadius;
			img.Draw("Top Right", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

			textPoint = ApplicationSettings.PaperCalibration.BottomRight.GetAdjustedCoord(rect.Size.Width, rect.Size.Height);
			textPoint.X -= ApplicationSettings.PaperROICircleRadius;
			textPoint.Y -= ApplicationSettings.PaperROICircleRadius;
			img.Draw("Bottom Right", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);
			#endregion

			picture.Image = img;
		}

		private PaperPoint getClosestPaperPoint(Point mousePoint, double minimumDistance = double.MaxValue) {
			Point? hit = picture.GetImagePoint(mousePoint);
			if (hit == null) return null;

			double closest = double.MaxValue;
			Point relative = (Point)hit;
			Size imgSize = picture.Image.Size;
			PaperPoint closestPoint = null;
			foreach (PaperPoint point in ApplicationSettings.PaperCalibration.ToArray()) {
				Point pos = point.GetScreenCoord(imgSize);
				double distance = Math.Sqrt(Math.Pow(relative.X - pos.X, 2) + Math.Pow(relative.Y - pos.Y, 2));
				if ((distance < closest) && (distance <= minimumDistance)) {
					closest = distance;
					closestPoint = point;
				}
			}

			return closestPoint;
		}

		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (draggingPoint != null) {
				PointF? hit = picture.GetRelativeImagePoint(new Point(e.X, e.Y));
				if(hit != null) {
					draggingPoint.X = ((PointF)hit).X;
					draggingPoint.Y = ((PointF)hit).Y;
				}
			} else {
				PaperPoint closestPoint = getClosestPaperPoint(new Point(e.X, e.Y), 11);
				PaperPicture.Cursor = ((closestPoint != null) ? Cursors.Hand : Cursors.Default);
			}
		}

		private void PaperPicture_MouseLeave(object sender, EventArgs e) {
			PaperPicture_MouseUp(null, null);
		}

		private void PaperPicture_MouseDown(object sender, MouseEventArgs e) {
			if (draggingPoint == null) {
				draggingPoint = getClosestPaperPoint(new Point(e.X, e.Y), 11);
				if (draggingPoint != null) PaperPicture.Cursor = Cursors.Default;
			}
		}

		private void PaperPicture_MouseUp(object sender, MouseEventArgs e) {
			PaperPicture.Cursor = Cursors.Default;
			Cursor.Current = Cursors.Default;
			draggingPoint = null;
		}

		private void ResetBounds_Click(object sender, EventArgs e) {
			ApplicationSettings.PaperCalibration.ResetToDefault();
		}

		private void AutoDetect_Click(object sender, EventArgs e) {
			autoDetectPaper = true;
		}

		private void detectPaper(Vision vision) {
			RotatedRect? auto = vision.AutoDetectPaper();
			Size imgSize = vision.GrayscaleImage.Size;
			if (auto == null) {
				MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			} else {
				RotatedRect bounds = (RotatedRect)auto;
				double d = Math.Sqrt(Math.Pow(bounds.Size.Width / 2, 2) + Math.Pow(bounds.Size.Height / 2, 2));
				double radAngle = Math.Abs(bounds.Angle * Math.PI / 180);
				double ratio = Math.Atan(bounds.Size.Width / bounds.Size.Height);

				float deltaX = (float)(d * Math.Sin(ratio - radAngle));
				float deltaY = (float)(d * Math.Cos(ratio - radAngle));

				ApplicationSettings.PaperCalibration.TopRight.SetPoint(new PointF(bounds.Center.X + deltaX, bounds.Center.Y - deltaY), imgSize);
				ApplicationSettings.PaperCalibration.BottomLeft.SetPoint(new PointF(bounds.Center.X - deltaX, bounds.Center.Y + deltaY), imgSize);

				deltaX = (float)(d * Math.Sin(ratio + radAngle));
				deltaY = (float)(d * Math.Cos(ratio + radAngle));

				ApplicationSettings.PaperCalibration.TopLeft.SetPoint(new PointF(bounds.Center.X - deltaX, bounds.Center.Y - deltaY), imgSize);
				ApplicationSettings.PaperCalibration.BottomRight.SetPoint(new PointF(bounds.Center.X + deltaX, bounds.Center.Y + deltaY), imgSize);
			}
		}
	}

}
