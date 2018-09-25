﻿namespace RobotArmUR2
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
			this.OriginalImage = new System.Windows.Forms.PictureBox();
			this.ResolutionText = new System.Windows.Forms.Label();
			this.GrayImage = new System.Windows.Forms.PictureBox();
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
			this.CannyImage = new System.Windows.Forms.PictureBox();
			this.AutoConnect = new System.Windows.Forms.Button();
			this.RGBValues = new System.Windows.Forms.Label();
			this.ClickLocation = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.OriginalImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GrayImage)).BeginInit();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CannyImage)).BeginInit();
			this.SuspendLayout();
			// 
			// OriginalImage
			// 
			this.OriginalImage.Location = new System.Drawing.Point(12, 31);
			this.OriginalImage.Name = "OriginalImage";
			this.OriginalImage.Size = new System.Drawing.Size(320, 240);
			this.OriginalImage.TabIndex = 0;
			this.OriginalImage.TabStop = false;
			this.OriginalImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OriginalImage_MouseClick);
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
			// GrayImage
			// 
			this.GrayImage.Location = new System.Drawing.Point(338, 31);
			this.GrayImage.Name = "GrayImage";
			this.GrayImage.Size = new System.Drawing.Size(320, 240);
			this.GrayImage.TabIndex = 2;
			this.GrayImage.TabStop = false;
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.calibrateToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1032, 28);
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
			// 
			// CannyImage
			// 
			this.CannyImage.Location = new System.Drawing.Point(664, 31);
			this.CannyImage.Name = "CannyImage";
			this.CannyImage.Size = new System.Drawing.Size(320, 240);
			this.CannyImage.TabIndex = 5;
			this.CannyImage.TabStop = false;
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
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1032, 450);
			this.Controls.Add(this.ClickLocation);
			this.Controls.Add(this.RGBValues);
			this.Controls.Add(this.AutoConnect);
			this.Controls.Add(this.CannyImage);
			this.Controls.Add(this.GrayImage);
			this.Controls.Add(this.ResolutionText);
			this.Controls.Add(this.OriginalImage);
			this.Controls.Add(this.menuStrip1);
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Robot Arm UR2";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.OriginalImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GrayImage)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CannyImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox OriginalImage;
		private System.Windows.Forms.Label ResolutionText;
		private System.Windows.Forms.PictureBox GrayImage;
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
		private System.Windows.Forms.PictureBox CannyImage;
		private System.Windows.Forms.Button AutoConnect;
		private System.Windows.Forms.ToolStripMenuItem calibrateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem goToHomeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem paperPositionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem robotPositionToolStripMenuItem;
		private System.Windows.Forms.Label RGBValues;
		private System.Windows.Forms.Label ClickLocation;
	}
}

