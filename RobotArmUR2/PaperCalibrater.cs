using Emgu.CV;
using System;
using System.Drawing;
using System.Windows.Forms;
using RobotArmUR2.VisionProcessing;
using Emgu.CV.Structure;

namespace RobotArmUR2 {
	public partial class PaperCalibrater : Form {

		private static readonly Bgr MaskColor = new Bgr(42, 240, 247);
		private static readonly float MaskTransparency = 0.2f;

		private static readonly Bgr CircleColor = new Bgr(42, 240, 247);
		private static readonly int CircleThickness = 3;
		private static readonly int CircleRadius = 10;

		private Vision vision;
		private PaperPoint draggingPoint = null;
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
			
			Point[] paperPoints = new Point[4];
			paperPoints[0] = vision.PaperCalibration.BottomLeft.GetScreenCoord(rect.Size);
			paperPoints[1] = vision.PaperCalibration.TopLeft.GetScreenCoord(rect.Size);
			paperPoints[2] = vision.PaperCalibration.TopRight.GetScreenCoord(rect.Size);
			paperPoints[3] = vision.PaperCalibration.BottomRight.GetScreenCoord(rect.Size);
			rect.FillConvexPoly(paperPoints, MaskColor);
			CvInvoke.AddWeighted(img, 1 - MaskTransparency, rect, MaskTransparency, 0, img);

			foreach (PaperPoint point in vision.PaperCalibration.ToArray()) {
				img.Draw(new CircleF(point.GetScreenCoord(rect.Size), CircleRadius), CircleColor, CircleThickness);
			}

			picture.Image = img;
		}

		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (vision == null) {
				PaperPicture.Cursor = Cursors.Default;
				return;
			}

			if (draggingPoint != null) {
				PointF? hit = picture.GetRelativeImagePoint(new Point(e.X, e.Y));
				if(hit != null) {
					draggingPoint.X = ((PointF)hit).X; //TODO put inside PaperPoint class?
					draggingPoint.Y = ((PointF)hit).Y;
				}
			} else {
				bool showHand = false;
				PaperCalibration calibration = vision.PaperCalibration; //TODO for adaptive circle, use relative points instead
				Point? hit = picture.GetImagePoint(new Point(e.X, e.Y)); //TODO since other method uses similar code, put in a method?
				if (hit != null) {
					Point relative = (Point)hit;
					foreach (PaperPoint point in calibration.ToArray()) {
						Point imgPos = point.GetScreenCoord(picture.Image.Size);
						double distance = Math.Sqrt(Math.Pow(relative.X - imgPos.X, 2) + Math.Pow(relative.Y - imgPos.Y, 2));
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
			if (draggingPoint == null) {
				if (picture.Image == null) return; //TODO With EmguPictureBox, is this needed?
				PaperCalibration calibration = vision.PaperCalibration;
				double closest = double.MaxValue;
				//foreach (PointF point in calibration.ToArray(PaperPicture.Size)) {
				Point? hit = picture.GetImagePoint(new Point(e.X, e.Y));
				if (hit == null) return; //TODO instead clip to closest point on image
				Point relative = (Point)hit;
				Size imgSize = picture.Image.Size;
				foreach(PaperPoint point in vision.PaperCalibration.ToArray()) {
					Point pos = point.GetScreenCoord(imgSize);
					double distance = Math.Sqrt(Math.Pow(relative.X - pos.X, 2) + Math.Pow(relative.Y - pos.Y, 2));
					if((distance < closest) && (distance <= 11)) {
						closest = distance;
						draggingPoint = point;
					}
				}
				
				if (draggingPoint != null) PaperPicture.Cursor = Cursors.Default;
			}
		}

		private void PaperPicture_MouseUp(object sender, MouseEventArgs e) {
			PaperPicture.Cursor = Cursors.Default;
			Cursor.Current = Cursors.Default;
			draggingPoint = null;
		}

		private void ResetBounds_Click(object sender, EventArgs e) {
			if (vision == null) return;
			vision.PaperCalibration.ResetToDefault(); //TODO make thread safe
		}

		private void AutoDetect_Click(object sender, EventArgs e) { //TODO auto-detect
			RotatedRect? auto = vision.AutoDetectPaper();
			if (auto == null) {
				MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			} else {
				RotatedRect bounds = (RotatedRect)auto;
				double d = Math.Sqrt(Math.Pow(bounds.Size.Width / 2, 2) + Math.Pow(bounds.Size.Height / 2, 2));
				double radAngle = bounds.Angle * Math.PI / 180;
				double ratio = Math.Atan(bounds.Size.Width / bounds.Size.Height);

				double deltaX = d * Math.Sin(ratio - radAngle);
				double deltaY = d * Math.Cos(ratio - radAngle);
				vision.PaperCalibration.TopRight.X = (float)(bounds.Center.X + deltaX); //TODO create a thread-safe method for setting full points
				vision.PaperCalibration.TopRight.Y = (float)(bounds.Center.Y - deltaY);
				vision.PaperCalibration.BottomLeft.X = (float)(bounds.Center.X - deltaX);
				vision.PaperCalibration.BottomLeft.Y = (float)(bounds.Center.Y + deltaY);

				deltaX = d * Math.Sin(ratio + radAngle);
				deltaY = d * Math.Cos(ratio + radAngle);
				vision.PaperCalibration.TopLeft.X = (float)(bounds.Center.X - deltaX);
				vision.PaperCalibration.TopLeft.Y = (float)(bounds.Center.Y - deltaY);
				vision.PaperCalibration.BottomRight.X = (float)(bounds.Center.X + deltaX);
				vision.PaperCalibration.BottomRight.Y = (float)(bounds.Center.Y + deltaY);
			}
		}
	}

}
