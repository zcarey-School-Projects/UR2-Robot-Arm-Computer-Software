using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using RobotArmUR2.Robot_Programs;
using RobotArmUR2.VisionProcessing;
using RobotHelpers.InputHandling;

namespace RobotArmUR2
{
	public partial class Form1 : Form, IRobotUI, IVisionUI
	{

		private Vision vision;

		private Robot robot;
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;
		
		//Custom PictureBox wrappers that make using them simpler
		private EmguPictureBox<Bgr, byte> origImage;
		private EmguPictureBox<Gray, byte> threshImage;
		private EmguPictureBox<Bgr, byte> warpedImage;

		//private volatile bool manualMoveEnabled = true;

		public Form1()
		{
			InitializeComponent();
			origImage = new EmguPictureBox<Bgr, byte>(this, Image1);
			threshImage = new EmguPictureBox<Gray, byte>(this, Image2);
			warpedImage = new EmguPictureBox<Bgr, byte>(this, Image3);

			Properties.Settings.Default.Reload();
			this.robot = new Robot();
			robot.UIListener = this;
			robotCalibrater = new RobotCalibrater(robot);
			vision = new Vision(/*paperCalibrater*/);
			//vision.setCamera(Default_Camera_Index);
			vision.InputStream = new ImageInput("DebugImages\\Test Table.jpg");
			//vision.SetUIListener(this);
			vision.UIListener = this;

			paperCalibrater = new PaperCalibrater(/*this, vision*/);

			vision.PaperCalibration = new PaperCalibration(
				new PointF(Properties.Settings.Default.PaperPoint0X, Properties.Settings.Default.PaperPoint0Y),
				new PointF(Properties.Settings.Default.PaperPoint1X, Properties.Settings.Default.PaperPoint1Y),
				new PointF(Properties.Settings.Default.PaperPoint2X, Properties.Settings.Default.PaperPoint2Y),
				new PointF(Properties.Settings.Default.PaperPoint3X, Properties.Settings.Default.PaperPoint3Y)
			);

			RobotSpeedSlider.Value = Properties.Settings.Default.RobotSpeed;
			RobotSpeedSlider_Scroll(null, null);
			/*
			PrescaleSlider.Value = Properties.Settings.Default.RobotPrescale;
			PrescaleSlider_Scroll(null, null);*/
		}

		private void Form1_Load(object sender, EventArgs e) {
			vision.start();

		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			Properties.Settings.Default.RobotSpeed = RobotSpeedSlider.Value;
			//Properties.Settings.Default.RobotPrescale = PrescaleSlider.Value;
			Properties.Settings.Default.Save();
			vision.stop();

			//TODO do save stuff
			//Properties.Settings.Default.Save();
		}

		public void VisionUI_NewFrameFinished(Vision vision) {
			origImage.Image = vision.InputImage; //grabs image before continuing, therefore should be thread safe.
			//threshImage.Image = vision.ThresholdImage;
			threshImage.Image = vision.CannyImage;

			Image<Bgr, byte> warped = vision.WarpedImage.Convert<Bgr, byte>();
			List<Triangle2DF> trigs = vision.Triangles;
			List<RotatedRect> squares = vision.Squares;
			vision.DrawTriangles(warped, vision.Triangles, ApplicationSettings.TriangleHighlightColor, ApplicationSettings.TriangleHighlightThickness);
			vision.DrawSquares(warped, vision.Squares, ApplicationSettings.SquareHighlightColor, ApplicationSettings.SquareHighlightThickness);
			warpedImage.Image = warped;

			PointF? robotCoords = null;
			if(trigs.Count > 0) {
				PointF pt = trigs[0].Centeroid;
				PointF rel = new PointF(pt.X / warped.Width, pt.Y / warped.Height);
				robotCoords = RobotProgram.CalculateRobotCoordinates(robot.Calibration, rel);
			}else if(squares.Count > 0) {
				PointF pt = squares[0].Center;
				PointF rel = new PointF(pt.X / warped.Width, pt.Y / warped.Height);
				robotCoords = RobotProgram.CalculateRobotCoordinates(robot.Calibration, rel);
			}

			BeginInvoke(new Action(() => {
				TriangleCount.Text = "Triangles: " + trigs.Count;
				SquareCount.Text = "Squares: " + squares.Count;
				if (robotCoords != null) {
					PointF coord = (PointF)robotCoords;
					TargetCoords.Text = "Target: (" + coord.X.ToString("N2").PadLeft(4) + "°, " + coord.Y.ToString("N2").PadLeft(5) +" mm)";
				}
			}));

			paperCalibrater.NewFrameFinished(vision);
		}

		private void screenshotToolStripMenuItem_Click(object sender, EventArgs e) {
			InputHandler input = vision.InputStream;
			if(input != null) {
				input.UserPromptSaveScreenshot();
			}
		}

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

		private void AutoConnect_Click(object sender, EventArgs e) {
			robot.ConnectToRobot();
		}

		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			robot.RunProgram(new ReturnHomeProgram(robot));
		}

		private void paperPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			paperCalibrater.ShowDialog();
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
			robot.ManualControlKeyEvent(e.KeyCode, true);
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e) {
			robot.ManualControlKeyEvent(e.KeyCode, false);
		}
		
		public void ChangeManualRotateImage(Robot.Rotation state) {
			BeginInvoke(new Action(() => {
				RotateLeftVisual.Image = ((state == Robot.Rotation.CCW) ? Properties.Resources.RotateLeft : Properties.Resources.RotateLeftOff);
				RotateRightVisual.Image = ((state == Robot.Rotation.CW) ? Properties.Resources.RotateRight : Properties.Resources.RotateRightOff);
			}));
		}

		public void ChangeManualExtensionImage(Robot.Extension state) {
			BeginInvoke(new Action(() => {
				ExtendVisual.Image = ((state == Robot.Extension.Outward) ? Properties.Resources.Extend : Properties.Resources.ExtendOff);
				RetractVisual.Image = ((state == Robot.Extension.Inward) ? Properties.Resources.Retract : Properties.Resources.RetractOff);
			}));
		}

		private void RobotSpeedSlider_Scroll(object sender, EventArgs e) {
			float ms = RobotSpeedSlider.Value / 10f;
			RobotSpeedLabel.Text = "Carriage Speed: " + ms + "ms";
			robot.SetSpeed((byte)RobotSpeedSlider.Value);
		}

		public void SerialOnConnectionChanged(bool isConnected, string portName) {
			BeginInvoke(new Action(() => {
				RobotConnected.CheckState = (isConnected ? CheckState.Checked : CheckState.Unchecked);
				RobotPort.Text = "Port: " + portName;
			}));
		}

		private void robotPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			robotCalibrater.ShowDialog();
		}

		private void GotoHomePos_Click(object sender, EventArgs e) {
			robot.RunProgram(new ReturnHomeProgram(robot));
		}

		public void ProgramStateChanged(bool running) {
			BeginInvoke(new Action(() => {
				RobotSpeedSlider.Enabled = !running;
				AutoConnect.Enabled = !running;
				menuStrip1.Enabled = !running;
				//manualMoveEnabled = !running; //TODO
				Stack.Text = (running ? "Cancel" : "Stack!");
			}));
		}
		
		private void Stack_Click(object sender, EventArgs e) {
			if(!robot.RunProgram(new StackingProgram(robot, vision, paperCalibrater))) {
				robot.CancelProgram();
			}
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

		private void Rotate180Checkbox_CheckedChanged(object sender, EventArgs e) {
			vision.RotateImage180 = Rotate180Checkbox.Checked;
		}

	}

}
