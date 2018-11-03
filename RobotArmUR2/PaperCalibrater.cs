using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RobotHelpers;
using RobotArmUR2.VisionProcessing;
using Emgu.CV.Structure;

namespace RobotArmUR2 {
	public partial class PaperCalibrater : Form {

		//private Form1 UI;
		private Vision vision;
		///*private*/public /*PointF[]*/PaperCalibration paperPoints;
		private /*int*/ CalibrationPoint draggingPoint = CalibrationPoint.BottomLeft;
		private bool dragging = false;
		//private Cursor OpenHandCursor = new Cursor(Properties.Resources.OpenHand.Handle);
		//private Cursor ClosedHandCursor = new Cursor(Properties.Resources.ClosedHand.Handle);
		private EmguPictureBox<Bgr, byte> picture;

		public PaperCalibrater(/*Vision vision*/) {
			InitializeComponent();
			picture = new EmguPictureBox<Bgr, byte>(this, PaperPicture);
			//this.UI = UI;
			//this.vision = vision;
		}

		private void PaperCalibrater_Load(object sender, EventArgs e) {
			
		}

		private void PaperCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			//TODO
			/*
			Properties.Settings.Default.PaperPoint0X = paperPoints.BL.X;
			Properties.Settings.Default.PaperPoint0Y = paperPoints.BL.Y;
			Properties.Settings.Default.PaperPoint1X = paperPoints.TL.X;
			Properties.Settings.Default.PaperPoint1Y = paperPoints.TL.Y;
			Properties.Settings.Default.PaperPoint2X = paperPoints.TR.X;
			Properties.Settings.Default.PaperPoint2Y = paperPoints.TR.Y;
			Properties.Settings.Default.PaperPoint3X = paperPoints.BR.X;
			Properties.Settings.Default.PaperPoint3Y = paperPoints.BR.Y;
			Properties.Settings.Default.Save();*/
			//UI.defaultMode();
		}

		public void NewFrameFinished(Vision vision) {
			this.vision = vision;

			Image<Bgr, byte> img = vision.InputImage;
			Image<Bgr, byte> rect = img.CopyBlank();
			PointF[] points = vision.PaperCalibration.ToArray(rect.Size);
			Point[] paperPoints = new Point[points.Length];
			for(int i = 0; i < points.Length; i++) {
				paperPoints[i] = new Point((int)points[i].X, (int)points[i].Y);
			}
			rect.FillConvexPoly(paperPoints, new Bgr(42, 240, 247));
			CvInvoke.AddWeighted(img, 0.8, rect, 0.2, 0, img);

			foreach (PointF point in points) {
				img.Draw(new CircleF(point, 10), new Bgr(42, 240, 247), 3);
			}

			picture.Image = img;
		}

		public void displayImage<TColor>(Image<TColor, byte> image) where TColor : struct, IColor {
			//if (image == null) return;
			//PaperPicture.InvokeIfRequired(pictureBox => { pictureBox.Image = image.Resize(pictureBox.Width, pictureBox.Height, Emgu.CV.CvEnum.Inter.Linear).ToBitmap(); });
		}
		/*
		private void calculatePaperCoords(float px, float py) {
			double p1x = paperPoints.BL.X;// * PaperPicture.Width;
			double p1y = paperPoints.BL.Y;// * PaperPicture.Height;

			double x = px / PaperPicture.Width;//px * PaperPicture.Width;
			double y = py / PaperPicture.Height;//py * PaperPicture.Height;

			double dx1 = paperPoints.TL.X - p1x;
			double dx2 = paperPoints.TR.X - paperPoints.BR.X;
			double dy1 = paperPoints.BR.Y - p1y;
			double dy2 = paperPoints.TR.Y - paperPoints.TL.Y;

			double D = dx2 - dx1;
			double E = paperPoints.BR.X - p1x;
			double F = dy2 - dy1;
			double G = paperPoints.TL.Y - p1y;

			double alpha = (-E * F) + (D * dy1);
			double hat = (F * x) - (E * G) - (p1x * F) + (D * p1y) + (dx1 * dy1) - (y * D);
			double e = (G * x) - (p1x * G) + (dx1 * p1y) - (y * dx1);
			double b = (-hat + Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);
			//double b2 = (-hat - Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);

			double a = (x - E * b - p1x) / (D * b + dx1);
			//double a2 = (x - E * b2 - H) / (D * b2 + dx1);

			PaperCoords.InvokeIfRequired(label => { label.Text = "(" + a.ToString("N2") + ", " + b.ToString("N2") + ")"; });
		}*/

		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (/*paperPoints == null ||*/ vision == null) {
				PaperPicture.Cursor = Cursors.Default;
				return;
			}

			if (dragging) {
				PointF? hit = picture.GetRelativeImagePoint(new Point(e.X, e.Y));
				if (hit == null) return;
				PointF relative = (PointF)hit;

				PaperCalibration calibration = vision.PaperCalibration;
				calibration.SetPoint(draggingPoint, relative);
				/*int position = (Bx - Ax) * (Y - Ay) - (By - Ay) * (X - Ax); <0 is below, >0 is above, 0 = on line
				
				if (draggingPoint == 0) {
					point.X = Math.Min(point.X, paperPoints[1].X);
					point.Y = Math.Min(point.Y, paperPoints[3].Y);
				}else if(draggingPoint == 1) {
					point.X = Math.Max(point.X, paperPoints[0].X);
					point.Y = Math.Min(point.Y, paperPoints[2].Y);
				}else if(draggingPoint == 2) {
					point.X = Math.Max(point.X, paperPoints[3].X);
					point.Y = Math.Max(point.Y, paperPoints[1].Y);
				}else if(draggingPoint == 3) {
					point.X = Math.Min(point.X, paperPoints[2].X);
					point.Y = Math.Max(point.Y, paperPoints[0].Y);
				}
				*/
				//TODO: constrain points
				//vision.setPaperMaskPoints(paperPoints.BL, paperPoints.TL, paperPoints.TR, paperPoints.BR);
				//vision.setPaperMaskPoints(paperPoints);
				vision.PaperCalibration = calibration;
			} else {
				bool showHand = false;
				PaperCalibration calibration = vision.PaperCalibration;
				Point? hit = picture.GetImagePoint(new Point(e.X, e.Y));
				if (hit != null) {
					Point relative = (Point)hit;
					foreach (PointF point in calibration.ToArray(picture.Image.Size)) {
						double distance = Math.Sqrt(Math.Pow(relative.X - point.X, 2) + Math.Pow(relative.Y - point.Y, 2));
						if (distance <= 11) {
							showHand = true;
							break;
						}
					}
				}
				//PaperPicture.Cursor = (showHand ? OpenHandCursor : Cursors.Default);
				PaperPicture.Cursor = (showHand ? Cursors.Hand : Cursors.Default);

				//calculatePaperCoords(e.X, e.Y);
			}
		}

		private void PaperPicture_MouseLeave(object sender, EventArgs e) {
			PaperPicture_MouseUp(null, null);
		}

		private void PaperPicture_MouseDown(object sender, MouseEventArgs e) {
			if (vision == null) return;
			if (!dragging) {
				if (picture.Image == null) return; //TODO With EmguPictureBox, is this needed?
				PaperCalibration calibration = vision.PaperCalibration;
				bool found = false;
				double closest = double.MaxValue;
				//foreach (PointF point in calibration.ToArray(PaperPicture.Size)) {
				Point? hit = picture.GetImagePoint(new Point(e.X, e.Y));
				if (hit == null) return; //TODO instead clip to closest point on image
				Point relative = (Point)hit;
				Size imgSize = picture.Image.Size;
				foreach (CalibrationPoint pos in (CalibrationPoint[]) Enum.GetValues(typeof(CalibrationPoint))) {
					PointF point = calibration.GetPoint(pos);
					double distance = Math.Sqrt(Math.Pow(relative.X - point.X * imgSize.Width, 2) + Math.Pow(relative.Y - point.Y * imgSize.Height, 2));
					if ((distance < closest) && (distance <= 11)) {
						found = true;
						closest = distance;
						draggingPoint = pos;
					}
				}
				
				if (found) {
					//PaperPicture.Cursor = ClosedHandCursor;
					PaperPicture.Cursor = Cursors.Default;
					dragging = true;
				}
			}
		}

		private void PaperPicture_MouseUp(object sender, MouseEventArgs e) {
			PaperPicture.Cursor = Cursors.Default;
			Cursor.Current = Cursors.Default;
			dragging = false;
		}

		private void ResetBounds_Click(object sender, EventArgs e) {
			/*paperPoints[0] = new PointF(0, 0);
			paperPoints[1] = new PointF(1, 0);
			paperPoints[2] = new PointF(1, 1);
			paperPoints[3] = new PointF(0, 1);
			*/
			//paperPoints = new PaperCalibration();
			//vision.setPaperMaskPoints(paperPoints[0], paperPoints[1], paperPoints[2], paperPoints[3]);
			//vision.setPaperMaskPoints(paperPoints);
			if (vision == null) return;
			vision.PaperCalibration = new PaperCalibration();
		}

		private void AutoDetect_Click(object sender, EventArgs e) {
			//if (!vision.AutoDetectPaper()) {
			//	MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			//}
			Console.WriteLine("Non-Implemented feature!");
		}
	}

}
