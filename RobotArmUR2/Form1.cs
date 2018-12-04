using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using RobotArmUR2.RobotControl;
using RobotArmUR2.RobotControl.Programs;
using RobotArmUR2.Util;
using RobotArmUR2.Util.Calibration;
using RobotArmUR2.Util.Calibration.Paper;
using RobotArmUR2.Util.Calibration.Robot;
using RobotArmUR2.VisionProcessing;
using RobotHelpers.InputHandling;

namespace RobotArmUR2
{
	public partial class Form1 : Form
	{

		private Vision vision;

		private Robot robot;
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;
		private RobotSettings robotSettings;
		
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
			this.robot = new Robot(); //TODO make vision event driven
			robot.OnManualControlChanged += Robot_OnManualControlChanged;
			robot.OnProgramStateChanged += Robot_OnProgramStateChanged;
			robot.Interface.OnConnectionChanged += RobotInterface_OnConnectionChanged;
			robotCalibrater = new RobotCalibrater(robot);
			vision = new Vision(/*paperCalibrater*/);
			//vision.setCamera(Default_Camera_Index);
			vision.InputStream = new ImageInput("DebugImages\\Test Table.jpg");

			vision.SetNativeResolutionText += VisionUI_SetNativeResolutionText;
			vision.SetFPSCounter += VisionUI_SetFPSCounter;
			vision.NewFrameFinished += VisionUI_NewFrameFinished;

			paperCalibrater = new PaperCalibrater(/*this, vision*/);
			robotSettings = new RobotSettings(robot);
		}

		private void Form1_Load(object sender, EventArgs e) {
			vision.start();

		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			Properties.Settings.Default.Save();
			vision.stop();
		}
		
		private void VisionUI_NewFrameFinished(Vision vision) {
			origImage.Image = vision.InputImage; //grabs image before continuing, therefore should be thread safe.
			threshImage.Image = vision.ThresholdImage;
			//threshImage.Image = vision.CannyImage;

			Image<Bgr, byte> warped = vision.WarpedImage.Convert<Bgr, byte>();
			vision.DrawShapes(warped, ApplicationSettings.TriangleHighlightColor, ApplicationSettings.SquareHighlightColor, ApplicationSettings.ShapeHighlightThickness);
			warpedImage.Image = warped;

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
			if (!robot.Interface.ConnectToRobot()) {
				MessageBox.Show("Could not find device.");
			}
		}

		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			robot.RunProgram(new ReturnHomeProgram());
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
		
		private void Robot_OnManualControlChanged(Rotation rotation, Extension extension) {
			BeginInvoke(new Action(() => {
				RotateLeftVisual.Image = ((rotation == Rotation.CCW) ? Properties.Resources.RotateLeft : Properties.Resources.RotateLeftOff);
				RotateRightVisual.Image = ((rotation == Rotation.CW) ? Properties.Resources.RotateRight : Properties.Resources.RotateRightOff);
				ExtendVisual.Image = ((extension == Extension.Outward) ? Properties.Resources.Extend : Properties.Resources.ExtendOff);
				RetractVisual.Image = ((extension == Extension.Inward) ? Properties.Resources.Retract : Properties.Resources.RetractOff);
			}));
		}

		private void RobotInterface_OnConnectionChanged(bool isConnected, string portName) {
			BeginInvoke(new Action(() => {
				RobotConnected.CheckState = (isConnected ? CheckState.Checked : CheckState.Unchecked);
				RobotPort.Text = "Port: " + portName;
			}));
		}

		private void robotPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			robotCalibrater.ShowDialog();
		}

		private void Robot_OnProgramStateChanged(bool running) {
			BeginInvoke(new Action(() => {
				AutoConnect.Enabled = !running;
				menuStrip1.Enabled = !running;
				Stack.Text = (running ? "Cancel" : "Stack!");
			}));
		}
		
		private void Stack_Click(object sender, EventArgs e) {
			if(!robot.RunProgram(new StackingProgram(vision))) {
				robot.CancelProgram();
			}
		}

		private void VisionUI_SetFPSCounter(float fps) {
			BeginInvoke(new Action(() => { //Thread safe, baby
				FpsStatusLabel.Text = fps.ToString("N2").PadLeft(6); //Converts FPS to a string with 2 decimals, with at most 3 digits
			}));
		}

		private void VisionUI_SetNativeResolutionText(Size resolution) {
			BeginInvoke(new Action(() => { //Thread safety!
				ResolutionText.Text = "Native Resolution: " + resolution.Width + " x " + resolution.Height;
			}));
		}

		private void Rotate180Checkbox_CheckedChanged(object sender, EventArgs e) {
			vision.RotateImage180 = Rotate180Checkbox.Checked;
		}

		private void ThresholdValue_Scroll(object sender, EventArgs e) {
			byte val = (byte)ThresholdValue.Value;
			vision.GrayscaleThreshold = val;
			ThresholdValueLabel.Text = "Threshold: " + val.ToString().PadLeft(3);
		}

		private void robotToolStripMenuItem_Click(object sender, EventArgs e) {
			robotSettings.ShowDialog();
		}
	}

}
