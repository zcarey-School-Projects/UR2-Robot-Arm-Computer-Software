﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using RobotArmUR2.RobotControl;
using RobotArmUR2.RobotControl.Programs;
using RobotArmUR2.Util.Calibration.Paper;
using RobotArmUR2.Util.Calibration.Robot;
using RobotArmUR2.VisionProcessing;

namespace RobotArmUR2
{
	/// <summary> This form deals with the main user interface that essentially controls the entire program. </summary>
	public partial class UIForm : Form
	{
		//Class that will be in charge of grabbing new images and processing them to find shapes.
		private Vision vision;

		//Contains all the necessary methods to control the robot.
		private Robot robot;

		//Pop-up forms that contain various settings
		private PaperCalibrater paperCalibrater;
		private RobotCalibrater robotCalibrater;
		private RobotSettings robotSettings;
		
		//Custom PictureBox wrappers that make using them simpler, and allow the user to select which image to be displayed.
		private SelectablePictureBox LeftPictureBox;
		private SelectablePictureBox MiddlePictureBox;
		private SelectablePictureBox RightPictureBox;

		public UIForm()
		{
			InitializeComponent();

			//Initialize picture boxes and their default displayed image.
			LeftPictureBox = new SelectablePictureBox(this, LeftImage, VisionImage.Input);
			MiddlePictureBox = new SelectablePictureBox(this, MiddleImage, VisionImage.Threshold);
			RightPictureBox = new SelectablePictureBox(this, RightImage, VisionImage.Shapes);

			//Initialize vision and assign it's events
			vision = new Vision();
			vision.OnNewFrameProcessed += VisionUI_NewFrameFinished;

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

		private void UIForm_Load(object sender, EventArgs e) {
			//Once the form is finished loading, open a test image.
			vision.InputStream.LoadLocalFile("DebugImages\\Test Table.jpg");
			vision.InputStream.Play();
		}

		private void UIForm_FormClosing(object sender, FormClosingEventArgs e) {
			//Stop grabbing new images and disconnect the robot.
			robot.Interface.Disconnect(); //Stops robot movement and such.
			vision.InputStream.Stop();
		}

		#region Vision Events
		/// <summary> Fires every time vision processes a new frame. </summary>
		/// <param name="vision"> Class that fired the event. </param>
		/// <param name="images"> Output images from vision. </param>
		private void VisionUI_NewFrameFinished(Vision vision, VisionImages images) {
			string resolutionText = "Native Resolution: " + vision.InputStream.Width + " x " + vision.InputStream.Height;
			float CurrentFPS = vision.InputStream.FPS;
			float TargetFPS = vision.InputStream.TargetFPS;

			
			if ((images != null) && (images.Raw != null)) {
				//resolutionText = "Native Resolution: " + images.Raw.Width + " x " + images.Raw.Height; //TODO remove if working
			}
			
			BeginInvoke(new Action(() => {
				ResolutionText.Text = resolutionText;
				FpsStatusLabel.Text = CurrentFPS.ToString("N2").PadLeft(6) + " FPS"; //Converts FPS to a string with 2 decimals, with at most 3 digits
				TargetFpsStatusLabel.Text = "Target FPS: " + TargetFPS.ToString("N2").PadLeft(6);
			}));

			//Draw a few images for the user
			LeftPictureBox.Images = images;
			MiddlePictureBox.Images = images;
			RightPictureBox.Images = images;
		}
		#endregion

		#region Robot Events
		/// <summary>Fired when the robot starts or stops a program. </summary>
		/// <param name="running">true id a program is running</param>
		private void Robot_OnProgramStateChanged(bool running) {
			BeginInvoke(new Action(() => {
				//Enable/disable certain elements depending if a program is running.
				AutoConnect.Enabled = !running;
				menuStrip1.Enabled = !running;
				Stack.Text = (running ? "Cancel" : "Stack!");
			}));
		}

		/// <summary> Event fired when a manual input is changed (i.e. Rotate Clock-wise key is pressed) </summary>
		/// <param name="rotation">State of rotation</param>
		/// <param name="extension">State of extension</param>
		private void Robot_OnManualControlChanged(Rotation rotation, Extension extension) {
			BeginInvoke(new Action(() => {
				//Change the arrow images depending on which direction the robot is moving.
				RotateLeftVisual.Image = ((rotation == Rotation.CCW) ? Properties.Resources.RotateLeft : Properties.Resources.RotateLeftOff);
				RotateRightVisual.Image = ((rotation == Rotation.CW) ? Properties.Resources.RotateRight : Properties.Resources.RotateRightOff);
				ExtendVisual.Image = ((extension == Extension.Outward) ? Properties.Resources.Extend : Properties.Resources.ExtendOff);
				RetractVisual.Image = ((extension == Extension.Inward) ? Properties.Resources.Retract : Properties.Resources.RetractOff);
			}));
		}

		/// <summary> Event fires when a robot is connected/disconnected </summary>
		/// <param name="isConnected">If the robot was connected, false if disconnected.</param>
		/// <param name="portName">The name of the port connected to.</param>
		private void RobotInterface_OnConnectionChanged(bool isConnected, string portName) {
			if (this.IsHandleCreated) { //Was getting called after the window closed, so added a check to prevent crashing.
				BeginInvoke(new Action(() => {
					RobotConnected.CheckState = (isConnected ? CheckState.Checked : CheckState.Unchecked);
					RobotPort.Text = "Port: " + portName;
				}));
			}
		}
		#endregion

		#region User Input

		#region Menu Strip Controls

		#region Pop-up Forms
		//Opens Paper Position Calibrater pop-up form. (Calibration > Paper)
		private void paperPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			paperCalibrater.ShowDialog();
		}

		//Opens RobotSettings pop-up  (Settings > Robot)
		private void robotToolStripMenuItem_Click(object sender, EventArgs e) {
			robotSettings.ShowDialog();
		}

		//Opens Robot Position Pop-up form. (Calibration > Robot)
		private void robotPositionToolStripMenuItem_Click(object sender, EventArgs e) {
			robotCalibrater.ShowDialog();
		}
		#endregion

		//Runs a program to return the robot to home. (Calibration > Go To Home)
		private void goToHomeToolStripMenuItem_Click(object sender, EventArgs e) {
			robot.RunProgram(new ReturnHomeProgram());
		}

		//Save a screenshot (File > Save > Screenshot)
		private void screenshotToolStripMenuItem_Click(object sender, EventArgs e) {
			vision.InputStream.PromptUserSaveScreenshot();
		}

		//Load an image from files (File > Load)
		private void imageToolStripMenuItem_Click(object sender, EventArgs e) {
			vision.InputStream.PromptUserLoadFile();
			vision.InputStream.Play();
		}

		#region Camera Input Selection
		/// <summary> Changes the input source to a camera and plays it. </summary>
		/// <param name="cameraIndex">Camera index to select. </param>
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

		//When the left-most picture box is clicked, display the mouse's click position and image color
		private void LeftImage_MouseClick(object sender, MouseEventArgs e) {
			string rVal = "   ";
			string gVal = "   ";
			string bVal = "   ";
			string x = "    ";
			string y = "    ";

			Image<Bgr, byte> img = LeftPictureBox.Image;
			Point? hit = LeftPictureBox.GetImagePoint(new Point(e.X, e.Y));
			if (hit != null) {
				Point pos = (Point)hit;
				x = pos.X.ToString().PadLeft(4);
				y = pos.Y.ToString().PadLeft(4);
				try {
					rVal = img.Data[pos.Y, pos.X, 2].ToString().PadLeft(3);
					gVal = img.Data[pos.Y, pos.X, 1].ToString().PadLeft(3);
					bVal = img.Data[pos.Y, pos.X, 0].ToString().PadLeft(3);
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

		//When Stack! button is clicked, run a robot program to pick up  the shapes.
		private void Stack_Click(object sender, EventArgs e) { //Button serves as both Stack and Cancel button
			if (!robot.RunProgram(new StackingProgram(vision))) { //Returns false if a program is already running
				robot.CancelProgram(); 
			}
		}

		//When RotateImage checkbox is changed, set the property in the input class
		private void Rotate180Checkbox_CheckedChanged(object sender, EventArgs e) {
			vision.InputStream.Rotate180 = Rotate180Checkbox.Checked;
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
