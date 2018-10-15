using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Threading;
using Emgu.CV.CvEnum;
using System.IO.Ports;
using System.IO;
using System.Management;
using RobotHelpers;

namespace RobotArmUR2
{
	public partial class Form1 : Form, RobotUIListener
	{

		private Vision vision;
		private String originalTitle;
		private SaveFileDialog saveDialog;

		private const int Default_Camera_Index = 0;
		private Robot robot;
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;

		private volatile bool manualMoveEnabled = true;

		public Form1()
		{
			InitializeComponent();
			Properties.Settings.Default.Reload();
			this.robot = new Robot(this);
			paperCalibrater = new PaperCalibrater(this);
			robotCalibrater = new RobotCalibrater(robot);
			vision = new Vision(this, paperCalibrater, Default_Camera_Index);
			//vision.setCamera(Default_Camera_Index);
			vision.setInternalImage("DebugImages\\Test Table.jpg");
			originalTitle = this.Text;

			saveDialog = new SaveFileDialog();
			saveDialog.RestoreDirectory = true;

			vision.setPaperMaskPoints(
				new PointF(Properties.Settings.Default.PaperPoint0X, Properties.Settings.Default.PaperPoint0Y),
				new PointF(Properties.Settings.Default.PaperPoint1X, Properties.Settings.Default.PaperPoint1Y),
				new PointF(Properties.Settings.Default.PaperPoint2X, Properties.Settings.Default.PaperPoint2Y),
				new PointF(Properties.Settings.Default.PaperPoint3X, Properties.Settings.Default.PaperPoint3Y)
			);

			RobotSpeedSlider.Value = Properties.Settings.Default.RobotSpeed;
			RobotSpeedSlider_Scroll(null, null);

			PrescaleSlider.Value = Properties.Settings.Default.RobotPrescale;
			PrescaleSlider_Scroll(null, null);
		}

		private void Form1_Load(object sender, EventArgs e) {
			vision.start();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			Properties.Settings.Default.RobotSpeed = RobotSpeedSlider.Value;
			Properties.Settings.Default.RobotPrescale = PrescaleSlider.Value;
			Properties.Settings.Default.Save();
			vision.stop();
		}

		private void screenshotToolStripMenuItem_Click(object sender, EventArgs e) {
			byte[,,] rawData = vision.getRawData();
			int width = vision.getStreamWidth();
			int height = vision.getStreamHeight();

			saveDialog.Filter = "OpenCV Image (*.rawcvimg)|*.rawcvimg";
			if(saveDialog.ShowDialog() == DialogResult.OK) {
				BinaryWriter writer = new BinaryWriter(File.OpenWrite(saveDialog.FileName));
				writer.Write(width);
				writer.Write(height);

				byte[] buffer = new byte[width * height];

				for(int channel = 0; channel < 3; channel++) {
					for(int y = 0; y < height; y++) {
						for(int x = 0; x < width; x++) {
							buffer[x + y * width] = rawData[y, x, channel];
						}
					}

					writer.Write(buffer, 0, buffer.Length);
				}

				writer.Close();
				writer.Close();
				writer.Dispose();
			}

		}

		#region Input Method Changers
		private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
			if(vision.loadImage() == false) {
				vision.setCamera(Default_Camera_Index);
			}
		}

		private void Camera0Menu_Click(object sender, EventArgs e) {
			vision.setCamera(0);
		}

		private void CameraMenu1_Click(object sender, EventArgs e) {
			vision.setCamera(1);
		}

		private void Camera2Menu_Click(object sender, EventArgs e) {
			vision.setCamera(2);
		}
		#endregion

		private void AutoConnect_Click(object sender, EventArgs e) {
			robot.ConnectToRobot();
		}

		#region UI Multi-Thread Update Methods
		public enum PictureId {
			//If you add a picturebox, don't forget to update "getPictureBoxFromId" method!
			Original,
			Gray,
			Canny
		}

		private PictureBox getPictureBoxFromId(PictureId id) {
			switch (id) {
				case PictureId.Original: return OriginalImage;
				case PictureId.Gray: return GrayImage;
				case PictureId.Canny: return CannyImage;
				default: return null;
			}
		}

		public void displayImage<TColor>(Image<TColor, byte> image, PictureId target) where TColor : struct, IColor {
			if (image == null) return;
			getPictureBoxFromId(target).InvokeIfRequired(pictureBox => { pictureBox.Image = image.Resize(pictureBox.Width, pictureBox.Height, Emgu.CV.CvEnum.Inter.Linear).ToBitmap(); });
		}

		public void setNativeResolutionText(int sizeX, int sizeY) {
			ResolutionText.InvokeIfRequired(label => { label.Text = "Native Resolution: " + sizeX + " x " + sizeY; });
		}

		public void setFPS(float fps) {
			this.InvokeIfRequired(form => { form.Text = originalTitle + "; FPS: " + fps.ToString("0.00"); });
		}
		#endregion

		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			throw new MissingMethodException("Unimplemented Method");
		}

		private void paperPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			vision.setMode(Vision.VisionMode.CalibratePaper);
			paperCalibrater.refresh(vision);
			paperCalibrater.ShowDialog();
		}

		public void defaultMode() {
			vision.setMode(Vision.VisionMode.Default);
		}

		private void OriginalImage_MouseClick(object sender, MouseEventArgs e) {
			string rVal = "   ";
			string gVal = "   ";
			string bVal = "   ";

			byte[,,] img = vision.getRawData(); //[y, x, channel] as bgr
			int width = vision.getStreamWidth();
			int height = vision.getStreamHeight();
			int x = e.X * width / OriginalImage.Width;
			int y = e.Y * height / OriginalImage.Height;
			if (img != null) {
				if ((x >= 0) && (x < width) && (y >= 0) && (y < height)) {
					try {
						rVal = img[y, x, 2].ToString().PadLeft(3);
						gVal = img[y, x, 1].ToString().PadLeft(3);
						bVal = img[y, x, 0].ToString().PadLeft(3);
					}catch(IndexOutOfRangeException) {
						rVal = "   ";
						gVal = "   ";
						bVal = "   ";
					}
				}
			}

			ClickLocation.Text = "X: " + ((e.X + 1) * width / OriginalImage.Width).ToString().PadLeft(4) + " Y: " + ((e.Y + 1) * height / OriginalImage.Height).ToString().PadLeft(4);
			RGBValues.Text = "R: " + rVal + " G: " + gVal + " B: " + bVal;
			
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, true);
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e) {
			formKeyEvent(e.KeyCode, false);
		}

		public void formKeyEvent(Keys key, bool pressed) {
			if (!manualMoveEnabled) return;
			if ((key == Keys.A)/* || (key == Keys.Left)*/) {
				robot.ManualControlKeyEvent(Robot.Key.Left, pressed);
			}else if ((key == Keys.D)/* || (key == Keys.Right)*/) {
				robot.ManualControlKeyEvent(Robot.Key.Right, pressed);
			}else if((key == Keys.W)/* || (key == Keys.Up)*/) {
				robot.ManualControlKeyEvent(Robot.Key.Up, pressed);
			}else if ((key == Keys.S)/* || (key == Keys.Down)*/) {
				robot.ManualControlKeyEvent(Robot.Key.Down, pressed);
			}
		}

		public void ChangeManualRotateImage(Robot.Rotation state) {
			RotateLeftVisual.InvokeIfRequired(img => { img.Image = ((state == Robot.Rotation.CCW) ? Properties.Resources.RotateLeft : Properties.Resources.RotateLeftOff); });
			RotateRightVisual.InvokeIfRequired(img => { img.Image = ((state == Robot.Rotation.CW) ? Properties.Resources.RotateRight : Properties.Resources.RotateRightOff); });
		}

		public void ChangeManualExtensionImage(Robot.Extension state) {
			ExtendVisual.InvokeIfRequired(img => { img.Image = ((state == Robot.Extension.Outward) ? Properties.Resources.Extend : Properties.Resources.ExtendOff); });
			RetractVisual.InvokeIfRequired(img => { img.Image = ((state == Robot.Extension.Inward) ? Properties.Resources.Retract : Properties.Resources.RetractOff); });
		}

		private void button1_MouseDown(object sender, MouseEventArgs e) {
			formKeyEvent(Keys.Left, true);
		}

		private void button1_MouseUp(object sender, MouseEventArgs e) {
			formKeyEvent(Keys.Left, false);
		}

		private void button1_MouseLeave(object sender, EventArgs e) {
			formKeyEvent(Keys.Left, false);
		}

		private void button2_MouseDown(object sender, MouseEventArgs e) {
			formKeyEvent(Keys.Right, true);
		}

		private void button2_MouseUp(object sender, MouseEventArgs e) {
			formKeyEvent(Keys.Right, false);
		}

		private void button2_MouseLeave(object sender, EventArgs e) {
			formKeyEvent(Keys.Right, false);
		}

		private void RobotSpeedSlider_Scroll(object sender, EventArgs e) {
			float ms = RobotSpeedSlider.Value / 10f;
			RobotSpeedLabel.Text = "Carriage Speed: " + ms + "ms";
			robot.changeRobotSpeed(RobotSpeedSlider.Value);
		}

		public void SerialOnConnectionChanged(bool isConnected, string portName) {
			RobotConnected.InvokeIfRequired(checkBox => { checkBox.CheckState = (isConnected ? CheckState.Checked : CheckState.Unchecked); });
			Console.WriteLine(isConnected);
			RobotPort.InvokeIfRequired(textLabel => { textLabel.Text = "Port: " + portName; });
		}

		private void robotPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			robotCalibrater.ShowDialog();
		}

		private void GotoHomePos_Click(object sender, EventArgs e) {
			robot.GoToHome();
		}

		public void ProgramStateChanged(bool running) {
			GotoHomePos.InvokeIfRequired(button => { button.Enabled = !running; });
			RobotSpeedSlider.InvokeIfRequired(slider => { slider.Enabled = !running; });
			AutoConnect.InvokeIfRequired(button => { button.Enabled = !running; });
			menuStrip1.InvokeIfRequired(menu => { menu.Enabled = !running; });
			manualMoveEnabled = !running;
			Stack.InvokeIfRequired(button => { button.Text = (running ? "Cancel" : "Stack!"); });
		}

		private void Stack_Click(object sender, EventArgs e) {
			if (!robot.runStackingProgram()) {
				robot.cancelStackingProgram();
			}
		}

		private void PrescaleSlider_Scroll(object sender, EventArgs e) {
			PrescaleLabel.Text = "Prescale: " + PrescaleSlider.Value;
			robot.changeRobotPrescale(PrescaleSlider.Value);
		}
	}

}
