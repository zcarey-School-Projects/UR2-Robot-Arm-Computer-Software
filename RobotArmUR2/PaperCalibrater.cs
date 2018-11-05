using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;
using RobotArmUR2.VisionProcessing;
using Emgu.CV.Structure;

namespace RobotArmUR2 {
	public partial class PaperCalibrater : Form {

		private Vision vision;
		private CalibrationPoint draggingPoint = CalibrationPoint.BottomLeft;
		private bool dragging = false; //TODO can combine in DraggingPoint by making it a nullable type
		private EmguPictureBox<Bgr, byte> picture;

		public PaperCalibrater() {
			InitializeComponent();
			picture = new EmguPictureBox<Bgr, byte>(this, PaperPicture);
		}

		private void PaperCalibrater_Load(object sender, EventArgs e) {
			
		}

		private void PaperCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			vision.PaperCalibration.SaveSettings();
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

		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (vision == null) {
				PaperPicture.Cursor = Cursors.Default;
				return;
			}

			if (dragging) {
				PointF? hit = picture.GetRelativeImagePoint(new Point(e.X, e.Y));
				if (hit == null) return;
				PointF relative = (PointF)hit;

				PaperCalibration calibration = vision.PaperCalibration;
				calibration.SetPoint(draggingPoint, relative);
				//vision.PaperCalibration = calibration;
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

				PaperPicture.Cursor = (showHand ? Cursors.Hand : Cursors.Default);
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
			if (vision == null) return;
			vision.PaperCalibration = new PaperCalibration();
		}

		private void AutoDetect_Click(object sender, EventArgs e) {
			RotatedRect? auto = vision.AutoDetectPaper();
			if (auto == null) {
				MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			} else {
				RotatedRect bounds = (RotatedRect)auto;
				PaperCalibration calibrate = new PaperCalibration();
				PointF[] verts = bounds.GetVertices();
				calibrate.TL = verts[0];
				calibrate.TR = verts[1];
				calibrate.BL = verts[2];
				calibrate.BR = verts[3];
				vision.PaperCalibration = calibrate;
			}
		}
	}

}
