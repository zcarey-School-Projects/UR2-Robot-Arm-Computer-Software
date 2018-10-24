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

namespace RobotArmUR2 {
	public partial class PaperCalibrater : Form {

		private Form1 UI;
		private Vision vision;
		/*private*/public PointF[] paperPoints;
		private int draggingPoint;
		private bool dragging = false;
		//private Cursor OpenHandCursor = new Cursor(Properties.Resources.OpenHand.Handle);
		//private Cursor ClosedHandCursor = new Cursor(Properties.Resources.ClosedHand.Handle);

		public PaperCalibrater(Form1 UI) {
			InitializeComponent();
			this.UI = UI;
		}

		private void PaperCalibrater_Load(object sender, EventArgs e) {
			
		}

		private void PaperCalibrater_FormClosing(object sender, FormClosingEventArgs e) {
			Properties.Settings.Default.PaperPoint0X = paperPoints[0].X;
			Properties.Settings.Default.PaperPoint0Y = paperPoints[0].Y;
			Properties.Settings.Default.PaperPoint1X = paperPoints[1].X;
			Properties.Settings.Default.PaperPoint1Y = paperPoints[1].Y;
			Properties.Settings.Default.PaperPoint2X = paperPoints[2].X;
			Properties.Settings.Default.PaperPoint2Y = paperPoints[2].Y;
			Properties.Settings.Default.PaperPoint3X = paperPoints[3].X;
			Properties.Settings.Default.PaperPoint3Y = paperPoints[3].Y;
			Properties.Settings.Default.Save();
			UI.defaultMode();
		}

		public void refresh(Vision vision) {
			this.vision = vision;
			this.paperPoints = vision.getPaperMaskPoints();
		}

		public void displayImage<TColor>(Image<TColor, byte> image) where TColor : struct, IColor {
			if (image == null) return;
			PaperPicture.InvokeIfRequired(pictureBox => { pictureBox.Image = image.Resize(pictureBox.Width, pictureBox.Height, Emgu.CV.CvEnum.Inter.Linear).ToBitmap(); });
		}

		private void calculatePaperCoords(float px, float py) {
			double p1x = paperPoints[0].X;// * PaperPicture.Width;
			double p1y = paperPoints[0].Y;// * PaperPicture.Height;

			double x = px / PaperPicture.Width;//px * PaperPicture.Width;
			double y = py / PaperPicture.Height;//py * PaperPicture.Height;

			double dx1 = paperPoints[1].X - p1x;
			double dx2 = paperPoints[2].X - paperPoints[3].X;
			double dy1 = paperPoints[3].Y - p1y;
			double dy2 = paperPoints[2].Y - paperPoints[1].Y;

			double D = dx2 - dx1;
			double E = paperPoints[3].X - p1x;
			double F = dy2 - dy1;
			double G = paperPoints[1].Y - p1y;

			double alpha = (-E * F) + (D * dy1);
			double hat = (F * x) - (E * G) - (p1x * F) + (D * p1y) + (dx1 * dy1) - (y * D);
			double e = (G * x) - (p1x * G) + (dx1 * p1y) - (y * dx1);
			double b = (-hat + Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);
			//double b2 = (-hat - Math.Sqrt(Math.Pow(hat, 2) - 4 * alpha * e)) / (2 * alpha);

			double a = (x - E * b - p1x) / (D * b + dx1);
			//double a2 = (x - E * b2 - H) / (D * b2 + dx1);

			PaperCoords.InvokeIfRequired(label => { label.Text = "(" + a.ToString("N2") + ", " + b.ToString("N2") + ")"; });
		}

		private void PaperPicture_MouseMove(object sender, MouseEventArgs e) {
			if (paperPoints == null || vision == null) {
				PaperPicture.Cursor = Cursors.Default;
				return;
			}

			if (dragging) {
				ref PointF point = ref paperPoints[draggingPoint];
				point.X = (float)e.X / PaperPicture.Width;
				point.Y = (float)e.Y / PaperPicture.Height;
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
				vision.setPaperMaskPoints(paperPoints[0], paperPoints[1], paperPoints[2], paperPoints[3]);
			} else {
				bool showHand = false;
				foreach (PointF point in paperPoints) {
					int x = (int)(point.X * PaperPicture.Width);
					int y = (int)(point.Y * PaperPicture.Height);
					double distance = Math.Sqrt(Math.Pow(e.X - x, 2) + Math.Pow(e.Y - y, 2));
					if (distance <= 11) {
						showHand = true;
						break;
					}
				}

				//PaperPicture.Cursor = (showHand ? OpenHandCursor : Cursors.Default);
				PaperPicture.Cursor = (showHand ? Cursors.Hand : Cursors.Default);

				calculatePaperCoords(e.X, e.Y);
			}
		}

		private void PaperPicture_MouseLeave(object sender, EventArgs e) {
			PaperPicture_MouseUp(null, null);
		}

		private void PaperPicture_MouseDown(object sender, MouseEventArgs e) {
			if (!dragging) {
				bool found = false;
				double closest = double.MaxValue;
				for (int i = 0; i < paperPoints.Length; i++) {
					PointF point = paperPoints[i];
					int x = (int)(point.X * PaperPicture.Width);
					int y = (int)(point.Y * PaperPicture.Height);
					double distance = Math.Sqrt(Math.Pow(e.X - x, 2) + Math.Pow(e.Y - y, 2));
					if ((distance < closest) && (distance <= 11)) {
						found = true;
						closest = distance;
						draggingPoint = i;
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
			paperPoints[0] = new PointF(0, 0);
			paperPoints[1] = new PointF(1, 0);
			paperPoints[2] = new PointF(1, 1);
			paperPoints[3] = new PointF(0, 1);

			vision.setPaperMaskPoints(paperPoints[0], paperPoints[1], paperPoints[2], paperPoints[3]);
		}

		private void AutoDetect_Click(object sender, EventArgs e) {
			if (!vision.AutoDetectPaper()) {
				MessageBox.Show("Could not find the paper.", "Error", MessageBoxButtons.OK);
			}
		}
	}

}
