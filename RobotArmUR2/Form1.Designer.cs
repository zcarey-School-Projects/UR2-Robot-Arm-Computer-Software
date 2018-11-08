namespace RobotArmUR2
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ResolutionText = new System.Windows.Forms.Label();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.screenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Camera0Menu = new System.Windows.Forms.ToolStripMenuItem();
			this.Camera1Menu = new System.Windows.Forms.ToolStripMenuItem();
			this.Camera2Menu = new System.Windows.Forms.ToolStripMenuItem();
			this.calibrateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.goToHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.paperPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.robotPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AutoConnect = new System.Windows.Forms.Button();
			this.RGBValues = new System.Windows.Forms.Label();
			this.ClickLocation = new System.Windows.Forms.Label();
			this.RobotSpeedSlider = new System.Windows.Forms.TrackBar();
			this.RobotSpeedLabel = new System.Windows.Forms.Label();
			this.RobotConnected = new System.Windows.Forms.CheckBox();
			this.RobotPort = new System.Windows.Forms.Label();
			this.Stack = new System.Windows.Forms.Button();
			this.PrescaleSlider = new System.Windows.Forms.TrackBar();
			this.PrescaleLabel = new System.Windows.Forms.Label();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.FpsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.RotateRightVisual = new System.Windows.Forms.PictureBox();
			this.RotateLeftVisual = new System.Windows.Forms.PictureBox();
			this.RetractVisual = new System.Windows.Forms.PictureBox();
			this.ExtendVisual = new System.Windows.Forms.PictureBox();
			this.Image3 = new System.Windows.Forms.PictureBox();
			this.Image2 = new System.Windows.Forms.PictureBox();
			this.Image1 = new System.Windows.Forms.PictureBox();
			this.TriangleCount = new System.Windows.Forms.Label();
			this.SquareCount = new System.Windows.Forms.Label();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RobotSpeedSlider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrescaleSlider)).BeginInit();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RotateRightVisual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RotateLeftVisual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RetractVisual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExtendVisual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Image3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Image2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Image1)).BeginInit();
			this.SuspendLayout();
			// 
			// ResolutionText
			// 
			this.ResolutionText.AutoSize = true;
			this.ResolutionText.Location = new System.Drawing.Point(12, 274);
			this.ResolutionText.Name = "ResolutionText";
			this.ResolutionText.Size = new System.Drawing.Size(167, 17);
			this.ResolutionText.TabIndex = 1;
			this.ResolutionText.Text = "Native Resolution --- x ---";
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.calibrateToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1005, 28);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.cameraToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.screenshotToolStripMenuItem});
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(135, 26);
			this.saveToolStripMenuItem.Text = "Save";
			// 
			// screenshotToolStripMenuItem
			// 
			this.screenshotToolStripMenuItem.Name = "screenshotToolStripMenuItem";
			this.screenshotToolStripMenuItem.Size = new System.Drawing.Size(156, 26);
			this.screenshotToolStripMenuItem.Text = "Screenshot";
			this.screenshotToolStripMenuItem.Click += new System.EventHandler(this.screenshotToolStripMenuItem_Click);
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageToolStripMenuItem});
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(135, 26);
			this.loadToolStripMenuItem.Text = "Load";
			// 
			// imageToolStripMenuItem
			// 
			this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
			this.imageToolStripMenuItem.Size = new System.Drawing.Size(126, 26);
			this.imageToolStripMenuItem.Text = "Image";
			this.imageToolStripMenuItem.Click += new System.EventHandler(this.imageToolStripMenuItem_Click);
			// 
			// cameraToolStripMenuItem
			// 
			this.cameraToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Camera0Menu,
            this.Camera1Menu,
            this.Camera2Menu});
			this.cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
			this.cameraToolStripMenuItem.Size = new System.Drawing.Size(135, 26);
			this.cameraToolStripMenuItem.Text = "Camera";
			// 
			// Camera0Menu
			// 
			this.Camera0Menu.Name = "Camera0Menu";
			this.Camera0Menu.Size = new System.Drawing.Size(92, 26);
			this.Camera0Menu.Text = "0";
			this.Camera0Menu.Click += new System.EventHandler(this.Camera0Menu_Click);
			// 
			// Camera1Menu
			// 
			this.Camera1Menu.Name = "Camera1Menu";
			this.Camera1Menu.Size = new System.Drawing.Size(92, 26);
			this.Camera1Menu.Text = "1";
			this.Camera1Menu.Click += new System.EventHandler(this.CameraMenu1_Click);
			// 
			// Camera2Menu
			// 
			this.Camera2Menu.Name = "Camera2Menu";
			this.Camera2Menu.Size = new System.Drawing.Size(92, 26);
			this.Camera2Menu.Text = "2";
			this.Camera2Menu.Click += new System.EventHandler(this.Camera2Menu_Click);
			// 
			// calibrateToolStripMenuItem
			// 
			this.calibrateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToHomeToolStripMenuItem,
            this.paperPositionToolStripMenuItem,
            this.robotPositionToolStripMenuItem});
			this.calibrateToolStripMenuItem.Name = "calibrateToolStripMenuItem";
			this.calibrateToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
			this.calibrateToolStripMenuItem.Text = "Calibrate";
			// 
			// goToHomeToolStripMenuItem
			// 
			this.goToHomeToolStripMenuItem.Name = "goToHomeToolStripMenuItem";
			this.goToHomeToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
			this.goToHomeToolStripMenuItem.Text = "Go To Home";
			this.goToHomeToolStripMenuItem.Click += new System.EventHandler(this.goToHomeToolStripMenuItem_Click);
			// 
			// paperPositionToolStripMenuItem
			// 
			this.paperPositionToolStripMenuItem.Name = "paperPositionToolStripMenuItem";
			this.paperPositionToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
			this.paperPositionToolStripMenuItem.Text = "Paper Position";
			this.paperPositionToolStripMenuItem.Click += new System.EventHandler(this.paperPositionToolStripMenuItem_Click);
			// 
			// robotPositionToolStripMenuItem
			// 
			this.robotPositionToolStripMenuItem.Name = "robotPositionToolStripMenuItem";
			this.robotPositionToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
			this.robotPositionToolStripMenuItem.Text = "Robot Position";
			this.robotPositionToolStripMenuItem.Click += new System.EventHandler(this.robotPositionToolStripMenuItem_Click);
			// 
			// AutoConnect
			// 
			this.AutoConnect.Location = new System.Drawing.Point(12, 415);
			this.AutoConnect.Name = "AutoConnect";
			this.AutoConnect.Size = new System.Drawing.Size(75, 23);
			this.AutoConnect.TabIndex = 6;
			this.AutoConnect.Text = "Connect";
			this.AutoConnect.UseVisualStyleBackColor = true;
			this.AutoConnect.Click += new System.EventHandler(this.AutoConnect_Click);
			// 
			// RGBValues
			// 
			this.RGBValues.AutoSize = true;
			this.RGBValues.Location = new System.Drawing.Point(12, 308);
			this.RGBValues.Name = "RGBValues";
			this.RGBValues.Size = new System.Drawing.Size(106, 17);
			this.RGBValues.TabIndex = 9;
			this.RGBValues.Text = "R:     G:     B:    ";
			// 
			// ClickLocation
			// 
			this.ClickLocation.AutoSize = true;
			this.ClickLocation.Location = new System.Drawing.Point(12, 291);
			this.ClickLocation.Name = "ClickLocation";
			this.ClickLocation.Size = new System.Drawing.Size(78, 17);
			this.ClickLocation.TabIndex = 10;
			this.ClickLocation.Text = "X:      Y:     ";
			// 
			// RobotSpeedSlider
			// 
			this.RobotSpeedSlider.Location = new System.Drawing.Point(338, 274);
			this.RobotSpeedSlider.Maximum = 235;
			this.RobotSpeedSlider.Minimum = 5;
			this.RobotSpeedSlider.Name = "RobotSpeedSlider";
			this.RobotSpeedSlider.Size = new System.Drawing.Size(426, 56);
			this.RobotSpeedSlider.TabIndex = 11;
			this.RobotSpeedSlider.Value = 100;
			this.RobotSpeedSlider.Scroll += new System.EventHandler(this.RobotSpeedSlider_Scroll);
			// 
			// RobotSpeedLabel
			// 
			this.RobotSpeedLabel.AutoSize = true;
			this.RobotSpeedLabel.Location = new System.Drawing.Point(770, 294);
			this.RobotSpeedLabel.Name = "RobotSpeedLabel";
			this.RobotSpeedLabel.Size = new System.Drawing.Size(73, 17);
			this.RobotSpeedLabel.TabIndex = 12;
			this.RobotSpeedLabel.Text = "Speed: 10";
			// 
			// RobotConnected
			// 
			this.RobotConnected.AutoCheck = false;
			this.RobotConnected.AutoSize = true;
			this.RobotConnected.Location = new System.Drawing.Point(12, 388);
			this.RobotConnected.Name = "RobotConnected";
			this.RobotConnected.Size = new System.Drawing.Size(98, 21);
			this.RobotConnected.TabIndex = 15;
			this.RobotConnected.Text = "Connected";
			this.RobotConnected.UseVisualStyleBackColor = true;
			// 
			// RobotPort
			// 
			this.RobotPort.AutoSize = true;
			this.RobotPort.Location = new System.Drawing.Point(12, 368);
			this.RobotPort.Name = "RobotPort";
			this.RobotPort.Size = new System.Drawing.Size(42, 17);
			this.RobotPort.TabIndex = 16;
			this.RobotPort.Text = "Port: ";
			// 
			// Stack
			// 
			this.Stack.Location = new System.Drawing.Point(909, 415);
			this.Stack.Name = "Stack";
			this.Stack.Size = new System.Drawing.Size(75, 23);
			this.Stack.TabIndex = 22;
			this.Stack.Text = "Stack!";
			this.Stack.UseVisualStyleBackColor = true;
			this.Stack.Click += new System.EventHandler(this.Stack_Click);
			// 
			// PrescaleSlider
			// 
			this.PrescaleSlider.Location = new System.Drawing.Point(338, 317);
			this.PrescaleSlider.Maximum = 255;
			this.PrescaleSlider.Name = "PrescaleSlider";
			this.PrescaleSlider.Size = new System.Drawing.Size(426, 56);
			this.PrescaleSlider.TabIndex = 23;
			this.PrescaleSlider.Scroll += new System.EventHandler(this.PrescaleSlider_Scroll);
			// 
			// PrescaleLabel
			// 
			this.PrescaleLabel.AutoSize = true;
			this.PrescaleLabel.Location = new System.Drawing.Point(770, 332);
			this.PrescaleLabel.Name = "PrescaleLabel";
			this.PrescaleLabel.Size = new System.Drawing.Size(79, 17);
			this.PrescaleLabel.TabIndex = 24;
			this.PrescaleLabel.Text = "Prescale: 0";
			// 
			// statusStrip1
			// 
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FpsStatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 449);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1005, 25);
			this.statusStrip1.TabIndex = 25;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// FpsStatusLabel
			// 
			this.FpsStatusLabel.Name = "FpsStatusLabel";
			this.FpsStatusLabel.Size = new System.Drawing.Size(79, 20);
			this.FpsStatusLabel.Text = "000.00 FPS";
			// 
			// RotateRightVisual
			// 
			this.RotateRightVisual.Image = global::RobotArmUR2.Properties.Resources.RotateRightOff;
			this.RotateRightVisual.Location = new System.Drawing.Point(263, 358);
			this.RotateRightVisual.Name = "RotateRightVisual";
			this.RotateRightVisual.Size = new System.Drawing.Size(50, 60);
			this.RotateRightVisual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.RotateRightVisual.TabIndex = 20;
			this.RotateRightVisual.TabStop = false;
			// 
			// RotateLeftVisual
			// 
			this.RotateLeftVisual.Image = global::RobotArmUR2.Properties.Resources.RotateLeftOff;
			this.RotateLeftVisual.Location = new System.Drawing.Point(161, 358);
			this.RotateLeftVisual.Name = "RotateLeftVisual";
			this.RotateLeftVisual.Size = new System.Drawing.Size(50, 60);
			this.RotateLeftVisual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.RotateLeftVisual.TabIndex = 19;
			this.RotateLeftVisual.TabStop = false;
			// 
			// RetractVisual
			// 
			this.RetractVisual.Image = global::RobotArmUR2.Properties.Resources.RetractOff;
			this.RetractVisual.Location = new System.Drawing.Point(217, 388);
			this.RetractVisual.Name = "RetractVisual";
			this.RetractVisual.Size = new System.Drawing.Size(40, 50);
			this.RetractVisual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.RetractVisual.TabIndex = 18;
			this.RetractVisual.TabStop = false;
			// 
			// ExtendVisual
			// 
			this.ExtendVisual.Image = global::RobotArmUR2.Properties.Resources.ExtendOff;
			this.ExtendVisual.Location = new System.Drawing.Point(217, 332);
			this.ExtendVisual.Name = "ExtendVisual";
			this.ExtendVisual.Size = new System.Drawing.Size(40, 50);
			this.ExtendVisual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ExtendVisual.TabIndex = 17;
			this.ExtendVisual.TabStop = false;
			// 
			// Image3
			// 
			this.Image3.Location = new System.Drawing.Point(664, 31);
			this.Image3.Name = "Image3";
			this.Image3.Size = new System.Drawing.Size(320, 240);
			this.Image3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.Image3.TabIndex = 5;
			this.Image3.TabStop = false;
			// 
			// Image2
			// 
			this.Image2.Location = new System.Drawing.Point(338, 31);
			this.Image2.Name = "Image2";
			this.Image2.Size = new System.Drawing.Size(320, 240);
			this.Image2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.Image2.TabIndex = 2;
			this.Image2.TabStop = false;
			// 
			// Image1
			// 
			this.Image1.Location = new System.Drawing.Point(12, 31);
			this.Image1.Name = "Image1";
			this.Image1.Size = new System.Drawing.Size(320, 240);
			this.Image1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.Image1.TabIndex = 0;
			this.Image1.TabStop = false;
			this.Image1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OriginalImage_MouseClick);
			// 
			// TriangleCount
			// 
			this.TriangleCount.AutoSize = true;
			this.TriangleCount.Location = new System.Drawing.Point(563, 365);
			this.TriangleCount.Name = "TriangleCount";
			this.TriangleCount.Size = new System.Drawing.Size(83, 17);
			this.TriangleCount.TabIndex = 26;
			this.TriangleCount.Text = "Triangles: 0";
			// 
			// SquareCount
			// 
			this.SquareCount.AutoSize = true;
			this.SquareCount.Location = new System.Drawing.Point(563, 382);
			this.SquareCount.Name = "SquareCount";
			this.SquareCount.Size = new System.Drawing.Size(77, 17);
			this.SquareCount.TabIndex = 27;
			this.SquareCount.Text = "Squares: 0";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1005, 474);
			this.Controls.Add(this.SquareCount);
			this.Controls.Add(this.TriangleCount);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.PrescaleLabel);
			this.Controls.Add(this.PrescaleSlider);
			this.Controls.Add(this.Stack);
			this.Controls.Add(this.RotateRightVisual);
			this.Controls.Add(this.RotateLeftVisual);
			this.Controls.Add(this.RetractVisual);
			this.Controls.Add(this.ExtendVisual);
			this.Controls.Add(this.RobotPort);
			this.Controls.Add(this.RobotConnected);
			this.Controls.Add(this.RobotSpeedLabel);
			this.Controls.Add(this.RobotSpeedSlider);
			this.Controls.Add(this.ClickLocation);
			this.Controls.Add(this.RGBValues);
			this.Controls.Add(this.AutoConnect);
			this.Controls.Add(this.Image3);
			this.Controls.Add(this.Image2);
			this.Controls.Add(this.ResolutionText);
			this.Controls.Add(this.Image1);
			this.Controls.Add(this.menuStrip1);
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Robot Arm UR2";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RobotSpeedSlider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrescaleSlider)).EndInit();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RotateRightVisual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RotateLeftVisual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RetractVisual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExtendVisual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Image3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Image2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Image1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label ResolutionText;
		private System.Windows.Forms.PictureBox Image2;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem screenshotToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cameraToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem Camera0Menu;
		private System.Windows.Forms.ToolStripMenuItem Camera1Menu;
		private System.Windows.Forms.ToolStripMenuItem Camera2Menu;
		private System.Windows.Forms.PictureBox Image3;
		private System.Windows.Forms.Button AutoConnect;
		private System.Windows.Forms.ToolStripMenuItem calibrateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem goToHomeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem paperPositionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem robotPositionToolStripMenuItem;
		private System.Windows.Forms.Label RGBValues;
		private System.Windows.Forms.Label ClickLocation;
		private System.Windows.Forms.TrackBar RobotSpeedSlider;
		private System.Windows.Forms.Label RobotSpeedLabel;
		private System.Windows.Forms.CheckBox RobotConnected;
		private System.Windows.Forms.Label RobotPort;
		private System.Windows.Forms.PictureBox ExtendVisual;
		private System.Windows.Forms.PictureBox RetractVisual;
		private System.Windows.Forms.PictureBox RotateLeftVisual;
		private System.Windows.Forms.PictureBox RotateRightVisual;
		private System.Windows.Forms.Button Stack;
		private System.Windows.Forms.TrackBar PrescaleSlider;
		private System.Windows.Forms.Label PrescaleLabel;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel FpsStatusLabel;
		private System.Windows.Forms.PictureBox Image1;
		private System.Windows.Forms.Label TriangleCount;
		private System.Windows.Forms.Label SquareCount;
	}
}

