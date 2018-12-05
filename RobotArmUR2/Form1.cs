﻿using System;
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

namespace RobotArmUR2
{
	public partial class Form1 : Form
	{
		//Class for grabbing images and finding shapes
		private Vision vision;

		//Contains controls for the robot
		private Robot robot;

		//Pop-up forms that contain various settings
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;
		private RobotSettings robotSettings;
		
		//Custom PictureBox wrappers that make using them simpler
		private EmguPictureBox<Bgr, byte> LeftPictureBox; //TODO can i put constructor here?
		private EmguPictureBox<Gray, byte> MiddlePictureBox;
		private EmguPictureBox<Bgr, byte> RightPictureBox;

		public Form1()
		{
			InitializeComponent();

			LeftPictureBox = new EmguPictureBox<Bgr, byte>(this, Image1);
			MiddlePictureBox = new EmguPictureBox<Gray, byte>(this, Image2);
			RightPictureBox = new EmguPictureBox<Bgr, byte>(this, Image3);

			Properties.Settings.Default.Reload(); //TODO do i need this?

			//Initialize vision and assign it's events
			vision = new Vision();
			vision.SetNativeResolutionText += VisionUI_SetNativeResolutionText;
			vision.SetFPSCounter += VisionUI_SetFPSCounter;
			vision.NewFrameFinished += VisionUI_NewFrameFinished;

			//Initialize robot and it's events
			robot = new Robot();
			robot.OnManualControlChanged += Robot_OnManualControlChanged;
			robot.OnProgramStateChanged += Robot_OnProgramStateChanged;
			robot.Interface.OnConnectionChanged += RobotInterface_OnConnectionChanged;

			//Initialize pop-up forms
			robotCalibrater = new RobotCalibrater(robot);
			paperCalibrater = new PaperCalibrater(vision);
			robotSettings = new RobotSettings(robot);
		}

		private void Form1_Load(object sender, EventArgs e) {
			//Once the form is finished loading, open a test image.
			vision.InputStream.LoadLocalFile("DebugImages\\Test Table.jpg");
			vision.InputStream.Play();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			//Stop grabbing new images and disconnect the robot.
			vision.InputStream.Stop();
			//TODO disconnect robot.
		}

		#region Vision Events
		//Event fires every time a new image is grabbed.
		private void VisionUI_NewFrameFinished(Vision vision) {
			VisionImages images = vision.Images;

			//Draw a few images for the user
			LeftPictureBox.Image = images.Input; 
			MiddlePictureBox.Image = images.Threshold;

			//Draw detected shapes onto the image then display it.
			if (images.Warped == null) RightPictureBox.Image = null;
			else {
				Image<Bgr, byte> warped = images.Warped.Convert<Bgr, byte>();
				vision.DrawShapes(warped, ApplicationSettings.TriangleHighlightColor, ApplicationSettings.SquareHighlightColor, ApplicationSettings.ShapeHighlightThickness);
				RightPictureBox.Image = warped;
			}
		}

		//Event fires when a new FPS is calculated 
		//TODO do we REALLY need to put this here? Does it need its own event?
		private void VisionUI_SetFPSCounter(float CurrentFPS, float TargetFPS) {
			BeginInvoke(new Action(() => { //Thread safe, baby
				FpsStatusLabel.Text = CurrentFPS.ToString("N2").PadLeft(6) + " FPS"; //Converts FPS to a string with 2 decimals, with at most 3 digits
				TargetFpsStatusLabel.Text = "Target FPS: " + TargetFPS.ToString("N2").PadLeft(6);
			}));
		}

		//Fires with the current size of the input image.
		private void VisionUI_SetNativeResolutionText(Size resolution) {
			BeginInvoke(new Action(() => { //Thread safety!
				ResolutionText.Text = "Native Resolution: " + resolution.Width + " x " + resolution.Height;
			}));
		}
		#endregion

		#region Robot Events
		//Event fired when the robot starts or stops a program.
		private void Robot_OnProgramStateChanged(bool running) {
			BeginInvoke(new Action(() => {
				//Enable/disable certain elements depending if a program is running.
				AutoConnect.Enabled = !running;
				menuStrip1.Enabled = !running;
				Stack.Text = (running ? "Cancel" : "Stack!");
			}));
		}

		//Event fired when a manual input is changed (i.e. Rotate Clock-wise key is pressed)
		private void Robot_OnManualControlChanged(Rotation rotation, Extension extension) {
			BeginInvoke(new Action(() => {
				//Change the arrow images depending on which direction the robot is moving.
				RotateLeftVisual.Image = ((rotation == Rotation.CCW) ? Properties.Resources.RotateLeft : Properties.Resources.RotateLeftOff);
				RotateRightVisual.Image = ((rotation == Rotation.CW) ? Properties.Resources.RotateRight : Properties.Resources.RotateRightOff);
				ExtendVisual.Image = ((extension == Extension.Outward) ? Properties.Resources.Extend : Properties.Resources.ExtendOff);
				RetractVisual.Image = ((extension == Extension.Inward) ? Properties.Resources.Retract : Properties.Resources.RetractOff);
			}));
		}

		//Event fires when a robot is connected/disconnected
		private void RobotInterface_OnConnectionChanged(bool isConnected, string portName) {
			BeginInvoke(new Action(() => {
				//Show it's connected and display the port name
				RobotConnected.CheckState = (isConnected ? CheckState.Checked : CheckState.Unchecked);
				RobotPort.Text = "Port: " + portName;
			}));
		}
		#endregion

		#region User Input

		#region Menu Strip Controls

		#region Pop-up Forms
		//Opens Paper Position Calibrater pop-up form.
		private void paperPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			paperCalibrater.ShowDialog();
		}

		//Opens RobotSettings pop-up form
		private void robotToolStripMenuItem_Click(object sender, EventArgs e) {
			robotSettings.ShowDialog();
		}

		//Opens Robot Position Pop-up form.
		private void robotPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			robotCalibrater.ShowDialog();
		}
		#endregion

		//Rruns a program to return the robot to home.
		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			robot.RunProgram(new ReturnHomeProgram());
		}

		//Save a screenshot
		private void screenshotToolStripMenuItem_Click(object sender, EventArgs e) {
			vision.InputStream.PromptUserSaveScreenshot();
		}

		//Load an image from files
		private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
			vision.InputStream.PromptUserLoadFile();
			vision.InputStream.Play();
		}

		#region Camera Input Selection
		//Changes the input source to a camera
		private void changeCamera(int cameraIndex) {
			vision.InputStream.SelectCamera(cameraIndex);
			vision.InputStream.Play();
		}

		private void Camera0Menu_Click(object sender, EventArgs e) { changeCamera(0); } //Menu strip select camera #0
		private void CameraMenu1_Click(object sender, EventArgs e) { changeCamera(1); }//Menu strip select camera #1
		private void Camera2Menu_Click(object sender, EventArgs e) { changeCamera(2); }//Menu strip select camera #2
		#endregion

		#endregion

		//Auto connect button, attempts to connect to the robot
		private void AutoConnect_Click(object sender, EventArgs e) {
			if (!robot.Interface.ConnectToRobot()) {
				MessageBox.Show("Could not find device.");
			}
		}

		//TODO rename PictureBoxes and events to left/middle/right
		//When the left-most picture box is clicked, display the mouse's click position and image color
		//TODO make thread safe
		private void OriginalImage_MouseClick(object sender, MouseEventArgs e) {

			string rVal = "   ";
			string gVal = "   ";
			string bVal = "   ";
			string x = "    ";
			string y = "    ";

			Point? hit = LeftPictureBox.GetImagePoint(new Point(e.X, e.Y));
			if (hit != null) {
				Point pos = (Point)hit;
				x = pos.X.ToString().PadLeft(4);
				y = pos.Y.ToString().PadLeft(4);
				try {
					rVal = LeftPictureBox.Image.Data[pos.Y, pos.X, 2].ToString().PadLeft(3);
					gVal = LeftPictureBox.Image.Data[pos.Y, pos.X, 1].ToString().PadLeft(3);
					bVal = LeftPictureBox.Image.Data[pos.Y, pos.X, 0].ToString().PadLeft(3);
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

		//When Stakc! button is clicked, run a robot program to pick up  the shapes.
		private void Stack_Click(object sender, EventArgs e) {
			if (!robot.RunProgram(new StackingProgram(vision))) {
				robot.CancelProgram();
			}
		}

		//When RotateImage checkbox is changed, set the property in the input class
		private void Rotate180Checkbox_CheckedChanged(object sender, EventArgs e) {
			vision.RotateImage180 = Rotate180Checkbox.Checked;//TODO ImageStream?
		}

		//When user changes threshold, apply the new value to the image.
		private void ThresholdValue_Scroll(object sender, EventArgs e) {
			byte val = (byte)ThresholdValue.Value;
			vision.GrayscaleThreshold = val;
			ThresholdValueLabel.Text = "Threshold: " + val.ToString().PadLeft(3);
		}

		//Key event for manual control
		private void Form1_KeyDown(object sender, KeyEventArgs e) { robot.ManualControlKeyEvent(e.KeyCode, true); }

		//Key event for manual control
		private void Form1_KeyUp(object sender, KeyEventArgs e) { robot.ManualControlKeyEvent(e.KeyCode, false); }

		#endregion

	}

}
