using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;

namespace RobotArmUR2.Util.Calibration.Paper {

	/// <summary>Used to calibrate the ROI of the paper.</summary>
	public partial class PaperCalibrater : Form {

		/// <summary>The current point being dragged.</summary>
		private PaperPoint draggingPoint = null;

		/// <summary>The wrapped picturebox</summary>
		private EmguPictureBox picture;

		/// <summary>Flag to detect the paper on the next image</summary>
		private volatile bool autoDetectPaper = false;

		/// <summary>Flag for if the form is open.</summary>
		private volatile bool isOpen = false;

		public PaperCalibrater(Vision vision) {
			InitializeComponent();
			picture = new EmguPictureBox(this, PaperPicture); //Wraps the picturebox
			vision.OnNewFrameProcessed += Vision_OnNewFrameFinished; //Listens for new images.
		}

		private void PaperCalibrater_Load(object sender, EventArgs e) {
			picture.Image = null; //Load to a blank image until a new one is available.
			isOpen = true; //Flag as open
		}

		private void PaperCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			isOpen = false; //Flag as closed
			ApplicationSettings.PaperCalibration.SaveSettings(); //Save any changed points.
		}

		/// <summary>Fired when a new frame is processed from the vision class.</summary>
		/// <param name="vision"></param>
		/// <param name="images"></param>
		private void Vision_OnNewFrameFinished(Vision vision, VisionImages images) { 
			if (!isOpen) return; //Only update image if window is open.

			if (images == null || images.Input == null) picture.Image = null;
			else { 
				if (autoDetectPaper) { //Check auto-detect paper flag
					autoDetectPaper = false;
					detectPaper(vision, images);
				}

				Image<Bgr, byte> img = images.Input.Copy(); //We don't want to edit the original, so make a copy just for us to display
				Image<Bgr, byte> rect = img.CopyBlank(); //Image the mask will be drawn to so we can weight it later.

				//Draw the mask on the blank image
				Point[] paperPoints = ApplicationSettings.PaperCalibration.ToArray(rect.Size); //{BottomLeft, TopLeft, TopRight, BottomRight}
				rect.FillConvexPoly(paperPoints, ApplicationSettings.PaperROIMaskColor);

				//Give it transparency on the input image
				CvInvoke.AddWeighted(img, 1 - ApplicationSettings.PaperROIMaskTransparency, rect, ApplicationSettings.PaperROIMaskTransparency, 0, img);

				//Draw circles on the corners
				foreach (PointF point in ApplicationSettings.PaperCalibration.ToArray(rect.Size.Width, rect.Size.Height)) {
					img.Draw(new CircleF(point, ApplicationSettings.PaperROICircleRadius), ApplicationSettings.PaperROICircleColor, ApplicationSettings.PaperROICircleThickness);
				}

				//Draw text next to the circles
				#region Draw Text next to circles
				PointF textPoint = ApplicationSettings.PaperCalibration.BottomLeft.GetScreenCoord(rect.Size);
				textPoint.X += ApplicationSettings.PaperROICircleRadius;
				textPoint.Y -= ApplicationSettings.PaperROICircleRadius;
				img.Draw("Bottom Left", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

				textPoint = ApplicationSettings.PaperCalibration.TopLeft.GetScreenCoord(rect.Size);
				textPoint.X += ApplicationSettings.PaperROICircleRadius;
				textPoint.Y += ApplicationSettings.PaperROICircleRadius;
				img.Draw("Top Left", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

				textPoint = ApplicationSettings.PaperCalibration.TopRight.GetScreenCoord(rect.Size);
				textPoint.X -= ApplicationSettings.PaperROICircleRadius;
				textPoint.Y += ApplicationSettings.PaperROICircleRadius;
				img.Draw("Top Right", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);

				textPoint = ApplicationSettings.PaperCalibration.BottomRight.GetScreenCoord(rect.Size);
				textPoint.X -= ApplicationSettings.PaperROICircleRadius;
				textPoint.Y -= ApplicationSettings.PaperROICircleRadius;
				img.Draw("Bottom Right", Point.Round(textPoint), ApplicationSettings.PaperROIFont, ApplicationSettings.PaperROIFontScale, ApplicationSettings.PaperROIFontColor, ApplicationSettings.PaperROIFontThickness);
				#endregion

				//Display the image
				picture.Image = img;
			}
		}

		/// <summary>Returns the closest paper point to the mouse that is within grabbing distance.</summary>
		/// <param name="mousePoint"></param>
		/// <returns></returns>
		private PaperPoint getClosestPaperPoint(Point mousePoint) {
			Point? hit = picture.GetImagePoint(mousePoint);
			if (hit == null) return null;
			double minimumDistance = ApplicationSettings.PaperROICircleRadius + (ApplicationSettings.PaperROICircleThickness / 2d);

			double closest = double.MaxValue;
			Point relative = (Point)hit;
			Size imgSize = picture.Image.Size;
			PaperPoint closestPoint = null;
			foreach (PaperPoint point in ApplicationSettings.PaperCalibration.ToArray()) {
				PointF pos = point.GetScreenCoord(imgSize);
				double distance = Math.Sqrt(Math.Pow(relative.X - pos.X, 2) + Math.Pow(relative.Y - pos.Y, 2));
				if ((distance < closest) && (distance <= minimumDistance)) {
					closest = distance;
					closestPoint = point;
				}
			}

			return closestPoint;
		}

		/// <summary>Fired when the mouse is moved over the picturebox.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (draggingPoint != null) { //We are dragging a point, so update it's position
				PointF? hit = picture.GetRelativeImagePoint(new Point(e.X, e.Y));
				if(hit != null) {
					draggingPoint.X = ((PointF)hit).X;
					draggingPoint.Y = ((PointF)hit).Y;
				}
			} else { //No point if being dragged. Look for candidates. If found, change mouse to give feedback to user.
				PaperPoint closestPoint = getClosestPaperPoint(new Point(e.X, e.Y));
				PaperPicture.Cursor = ((closestPoint != null) ? Cursors.Hand : Cursors.Default);
			}
		}

		/// <summary>Fired when the mouse exits outside of the picturebox.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PaperPicture_MouseLeave(object sender, EventArgs e) {
			PaperPicture_MouseUp(null, null);
		}

		/// <summary>Fired when the mouse clicks the picturebox</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PaperPicture_MouseDown(object sender, MouseEventArgs e) {
			if (draggingPoint == null) { //If no point is being dragged, check for one and start dragging it.
				draggingPoint = getClosestPaperPoint(new Point(e.X, e.Y));
				if (draggingPoint != null) PaperPicture.Cursor = Cursors.Default;
			}
		}

		/// <summary>Fired when the mouse is released over the picturebox.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PaperPicture_MouseUp(object sender, MouseEventArgs e) {
			PaperPicture.Cursor = Cursors.Default; //Stop dragging and reset the mouse cursor.
			Cursor.Current = Cursors.Default;
			draggingPoint = null;
		}

		/// <summary>Resets the mask to its default values.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ResetBounds_Click(object sender, EventArgs e) {
			ApplicationSettings.PaperCalibration.ResetToDefault();
		}

		/// <summary>Set flag to attempt to auto detect the paper.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AutoDetect_Click(object sender, EventArgs e) {
			autoDetectPaper = true;
		}

		/// <summary>Attempts to auto-detect paper and set paper calibration accordingly.</summary>
		/// <param name="vision"></param>
		/// <param name="images"></param>
		private void detectPaper(Vision vision, VisionImages images) {
			RotatedRect? auto = vision.AutoDetectPaper(images); //Auto-detect the paper
			Size imgSize = vision.Images.Input.Size;
				
			if(auto != null){ //If paper was found, set the calibration points.
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
			}else{ 
				MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			}
		}
	}

}
