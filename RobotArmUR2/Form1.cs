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
using RobotArmUR2.VisionProcessing;
using RobotHelpers.InputHandling;

namespace RobotArmUR2
{
	public partial class Form1 : Form, RobotUIListener, IVisionUI
	{

		private Vision vision;
		private SaveFileDialog saveDialog;

		private const int Default_Camera_Index = 0;
		private Robot robot;
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;

		private EmguPictureBox<Bgr, byte> origImage;
		private EmguPictureBox<Gray, byte> threshImage;
		private EmguPictureBox<Gray, byte> warpedImage;

		private volatile bool manualMoveEnabled = true;

		public Form1()
		{
			InitializeComponent();
			origImage = new EmguPictureBox<Bgr, byte>(this, Image1);
			threshImage = new EmguPictureBox<Gray, byte>(this, Image2);
			warpedImage = new EmguPictureBox<Gray, byte>(this, Image3);

			Properties.Settings.Default.Reload();
			this.robot = new Robot(this);
			robotCalibrater = new RobotCalibrater(robot);
			vision = new Vision(/*paperCalibrater*/);
			//vision.setCamera(Default_Camera_Index);
			vision.InputStream = new ImageInput("DebugImages\\Test Table.jpg");
			//vision.SetUIListener(this);
			vision.UIListener = this;

			paperCalibrater = new PaperCalibrater(/*this, vision*/);

			saveDialog = new SaveFileDialog();
			saveDialog.RestoreDirectory = true;

			vision.PaperCalibration = new PaperCalibration(
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

		public void VisionUI_NewFrameFinished(Vision vision) {
			origImage.Image = vision.InputImage; //grabs image before continuing, therefore should be thread safe.
			threshImage.Image = vision.ThresholdImage;
			warpedImage.Image = vision.WarpedImage;

			paperCalibrater.NewFrameFinished(vision);
		}

		private void screenshotToolStripMenuItem_Click(object sender, EventArgs e) {
			InputHandler input = vision.InputStream;
			if(input != null) {
				input.UserPromptSaveScreenshot();
			}
			/*
			byte[,,] rawData = vision.InputStream.ReadRawData(); //TODO make ReadRawData() a property of inputHanlder
			int width = vision.InputStream.GetWidth(); //TODO make getWidth a property of input handler
			int height = vision.InputStream.GetHeight(); //TODO make getHeight a property of input handler

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
			*/
		}

		#region Input Method Changers
		private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
			ImageInput newInput = new ImageInput();
			if (newInput.PromptUserToLoadFile()) {
				vision.InputStream = newInput;
			}
		}

		private void changeCamera(int cameraIndex) { vision.InputStream = new CameraInput(cameraIndex); }

		private void Camera0Menu_Click(object sender, EventArgs e) { changeCamera(0); }

		private void CameraMenu1_Click(object sender, EventArgs e) { changeCamera(1); }

		private void Camera2Menu_Click(object sender, EventArgs e) { changeCamera(2); }
		#endregion

		private void AutoConnect_Click(object sender, EventArgs e) {
			robot.ConnectToRobot();
		}

		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			throw new MissingMethodException("Unimplemented Method");
		}

		private void paperPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			//vision.setMode(Vision.VisionMode.CalibratePaper);
			//paperCalibrater.refresh(vision);
			paperCalibrater.ShowDialog();
		}

		public void defaultMode() {
			//vision.setMode(Vision.VisionMode.Default);
		}

		private void OriginalImage_MouseClick(object sender, MouseEventArgs e) {
			
			string rVal = "   ";
			string gVal = "   ";
			string bVal = "   ";
			string x = "    ";
			string y = "    ";

			Point? hit = origImage.GetImagePoint(new Point(e.X, e.Y));
			if(hit != null) {
				Point pos = (Point)hit;
				x = pos.X.ToString().PadLeft(4);
				y = pos.Y.ToString().PadLeft(4);
				try {
					rVal = origImage.Image.Data[pos.Y, pos.X, 2].ToString().PadLeft(3);
					gVal = origImage.Image.Data[pos.Y, pos.X, 1].ToString().PadLeft(3);
					bVal = origImage.Image.Data[pos.Y, pos.X, 0].ToString().PadLeft(3);
				} catch (IndexOutOfRangeException) {
					rVal = "   ";
					gVal = "   ";
					bVal = "   ";
					x = "    ";
					y = "    ";
				}
			}
			
			ClickLocation.Text = "X: " + x + " Y: " + y;
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
			}else if((key == Keys.E)) {
				robot.raiseServo();
			}else if((key == Keys.Q)) {
				robot.lowerServo();
			}else if((key == Keys.M)) {
				robot.magnetOn();
			}else if((key == Keys.N)) {
				robot.magnetOff();
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
			if (!robot.runStackingProgram(vision, paperCalibrater)) {
				robot.cancelStackingProgram();
			}
		}

		private void PrescaleSlider_Scroll(object sender, EventArgs e) {
			PrescaleLabel.Text = "Prescale: " + PrescaleSlider.Value;
			robot.changeRobotPrescale(PrescaleSlider.Value);
		}

		public void VisionUI_SetFPSCounter(float fps) {
			BeginInvoke(new Action(() => { //Thread safe, baby
				FpsStatusLabel.Text = fps.ToString("N2").PadLeft(6); //Converts FPS to a string with 2 decimals, with at most 3 digits
			}));
		}

		public void VisionUI_SetNativeResolutionText(Size resolution) {
			BeginInvoke(new Action(() => { //Thread safety!
				ResolutionText.Text = "Native Resolution: " + resolution.Width + " x " + resolution.Height;
			}));
		}

		/*private PictureBox getPictureBoxFromId(PictureId id) {
			switch (id) {
				case PictureId.Original: return OriginalImage;
				case PictureId.Gray: return GrayImage;
				case PictureId.Canny: return CannyImage;
				default: return null;
			}
		}*/

		/*public void VisionUI_DisplayImage<TColor, TDepth>(Image<TColor, TDepth> image, PictureId pictureId) where TColor : struct, IColor where TDepth : new() {
			PictureBox box = getPictureBoxFromId(pictureId);
			if (box == null) return;
			BeginInvoke(new Action(() => { //Always be thread safe, kids!
				box.Image = image.Bitmap;
			}));

		}*/
	}

}
