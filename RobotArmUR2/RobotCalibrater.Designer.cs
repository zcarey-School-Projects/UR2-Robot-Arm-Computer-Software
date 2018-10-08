namespace RobotArmUR2 {
	partial class RobotCalibrater {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
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
		private void InitializeComponent() {
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.BLMoveTo = new System.Windows.Forms.Button();
			this.BRMoveTo = new System.Windows.Forms.Button();
			this.TLMoveTo = new System.Windows.Forms.Button();
			this.TRMoveTo = new System.Windows.Forms.Button();
			this.BLCalibrate = new System.Windows.Forms.Button();
			this.BRCalibrate = new System.Windows.Forms.Button();
			this.TLCalibrate = new System.Windows.Forms.Button();
			this.TRCalibrate = new System.Windows.Forms.Button();
			this.ResetBL = new System.Windows.Forms.Button();
			this.ResetBR = new System.Windows.Forms.Button();
			this.ResetTL = new System.Windows.Forms.Button();
			this.ResetTR = new System.Windows.Forms.Button();
			this.ResetAll = new System.Windows.Forms.Button();
			this.BLLabel = new System.Windows.Forms.Label();
			this.BRLabel = new System.Windows.Forms.Label();
			this.TLLabel = new System.Windows.Forms.Label();
			this.TRLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(132, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Bottom-Left Corner:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(141, 17);
			this.label2.TabIndex = 1;
			this.label2.Text = "Bottom-Right Corner:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 73);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(113, 17);
			this.label3.TabIndex = 2;
			this.label3.Text = "Top-Left Corner:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 102);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(122, 17);
			this.label4.TabIndex = 3;
			this.label4.Text = "Top-Right Corner:";
			// 
			// BLMoveTo
			// 
			this.BLMoveTo.Location = new System.Drawing.Point(159, 12);
			this.BLMoveTo.Name = "BLMoveTo";
			this.BLMoveTo.Size = new System.Drawing.Size(75, 23);
			this.BLMoveTo.TabIndex = 4;
			this.BLMoveTo.Text = "Move To";
			this.BLMoveTo.UseVisualStyleBackColor = true;
			this.BLMoveTo.Click += new System.EventHandler(this.BLMoveTo_Click);
			// 
			// BRMoveTo
			// 
			this.BRMoveTo.Location = new System.Drawing.Point(159, 41);
			this.BRMoveTo.Name = "BRMoveTo";
			this.BRMoveTo.Size = new System.Drawing.Size(75, 23);
			this.BRMoveTo.TabIndex = 5;
			this.BRMoveTo.Text = "Move To";
			this.BRMoveTo.UseVisualStyleBackColor = true;
			this.BRMoveTo.Click += new System.EventHandler(this.BRMoveTo_Click);
			// 
			// TLMoveTo
			// 
			this.TLMoveTo.Location = new System.Drawing.Point(159, 70);
			this.TLMoveTo.Name = "TLMoveTo";
			this.TLMoveTo.Size = new System.Drawing.Size(75, 23);
			this.TLMoveTo.TabIndex = 6;
			this.TLMoveTo.Text = "Move To";
			this.TLMoveTo.UseVisualStyleBackColor = true;
			this.TLMoveTo.Click += new System.EventHandler(this.TLMoveTo_Click);
			// 
			// TRMoveTo
			// 
			this.TRMoveTo.Location = new System.Drawing.Point(159, 99);
			this.TRMoveTo.Name = "TRMoveTo";
			this.TRMoveTo.Size = new System.Drawing.Size(75, 23);
			this.TRMoveTo.TabIndex = 7;
			this.TRMoveTo.Text = "Move To";
			this.TRMoveTo.UseVisualStyleBackColor = true;
			this.TRMoveTo.Click += new System.EventHandler(this.TRMoveTo_Click);
			// 
			// BLCalibrate
			// 
			this.BLCalibrate.Location = new System.Drawing.Point(267, 12);
			this.BLCalibrate.Name = "BLCalibrate";
			this.BLCalibrate.Size = new System.Drawing.Size(75, 23);
			this.BLCalibrate.TabIndex = 8;
			this.BLCalibrate.Text = "Calibrate";
			this.BLCalibrate.UseVisualStyleBackColor = true;
			this.BLCalibrate.Click += new System.EventHandler(this.BLCalibrate_Click);
			// 
			// BRCalibrate
			// 
			this.BRCalibrate.Location = new System.Drawing.Point(267, 41);
			this.BRCalibrate.Name = "BRCalibrate";
			this.BRCalibrate.Size = new System.Drawing.Size(75, 23);
			this.BRCalibrate.TabIndex = 9;
			this.BRCalibrate.Text = "Calibrate";
			this.BRCalibrate.UseVisualStyleBackColor = true;
			this.BRCalibrate.Click += new System.EventHandler(this.BRCalibrate_Click);
			// 
			// TLCalibrate
			// 
			this.TLCalibrate.Location = new System.Drawing.Point(267, 70);
			this.TLCalibrate.Name = "TLCalibrate";
			this.TLCalibrate.Size = new System.Drawing.Size(75, 23);
			this.TLCalibrate.TabIndex = 10;
			this.TLCalibrate.Text = "Calibrate";
			this.TLCalibrate.UseVisualStyleBackColor = true;
			this.TLCalibrate.Click += new System.EventHandler(this.TLCalibrate_Click);
			// 
			// TRCalibrate
			// 
			this.TRCalibrate.Location = new System.Drawing.Point(267, 99);
			this.TRCalibrate.Name = "TRCalibrate";
			this.TRCalibrate.Size = new System.Drawing.Size(75, 23);
			this.TRCalibrate.TabIndex = 11;
			this.TRCalibrate.Text = "Calibrate";
			this.TRCalibrate.UseVisualStyleBackColor = true;
			this.TRCalibrate.Click += new System.EventHandler(this.TRCalibrate_Click);
			// 
			// ResetBL
			// 
			this.ResetBL.Location = new System.Drawing.Point(395, 12);
			this.ResetBL.Name = "ResetBL";
			this.ResetBL.Size = new System.Drawing.Size(75, 23);
			this.ResetBL.TabIndex = 12;
			this.ResetBL.Text = "Reset";
			this.ResetBL.UseVisualStyleBackColor = true;
			this.ResetBL.Click += new System.EventHandler(this.ResetBL_Click);
			// 
			// ResetBR
			// 
			this.ResetBR.Location = new System.Drawing.Point(395, 41);
			this.ResetBR.Name = "ResetBR";
			this.ResetBR.Size = new System.Drawing.Size(75, 23);
			this.ResetBR.TabIndex = 13;
			this.ResetBR.Text = "Reset";
			this.ResetBR.UseVisualStyleBackColor = true;
			this.ResetBR.Click += new System.EventHandler(this.ResetBR_Click);
			// 
			// ResetTL
			// 
			this.ResetTL.Location = new System.Drawing.Point(395, 70);
			this.ResetTL.Name = "ResetTL";
			this.ResetTL.Size = new System.Drawing.Size(75, 23);
			this.ResetTL.TabIndex = 14;
			this.ResetTL.Text = "Reset";
			this.ResetTL.UseVisualStyleBackColor = true;
			this.ResetTL.Click += new System.EventHandler(this.ResetTL_Click);
			// 
			// ResetTR
			// 
			this.ResetTR.Location = new System.Drawing.Point(395, 99);
			this.ResetTR.Name = "ResetTR";
			this.ResetTR.Size = new System.Drawing.Size(75, 23);
			this.ResetTR.TabIndex = 15;
			this.ResetTR.Text = "Reset";
			this.ResetTR.UseVisualStyleBackColor = true;
			this.ResetTR.Click += new System.EventHandler(this.ResetTR_Click);
			// 
			// ResetAll
			// 
			this.ResetAll.Location = new System.Drawing.Point(395, 163);
			this.ResetAll.Name = "ResetAll";
			this.ResetAll.Size = new System.Drawing.Size(75, 23);
			this.ResetAll.TabIndex = 16;
			this.ResetAll.Text = "Reset All";
			this.ResetAll.UseVisualStyleBackColor = true;
			this.ResetAll.Click += new System.EventHandler(this.ResetAll_Click);
			// 
			// BLLabel
			// 
			this.BLLabel.AutoSize = true;
			this.BLLabel.Location = new System.Drawing.Point(489, 15);
			this.BLLabel.Name = "BLLabel";
			this.BLLabel.Size = new System.Drawing.Size(147, 17);
			this.BLLabel.TabIndex = 17;
			this.BLLabel.Text = "(360.00^, 400.00 mm)";
			// 
			// BRLabel
			// 
			this.BRLabel.AutoSize = true;
			this.BRLabel.Location = new System.Drawing.Point(489, 44);
			this.BRLabel.Name = "BRLabel";
			this.BRLabel.Size = new System.Drawing.Size(147, 17);
			this.BRLabel.TabIndex = 18;
			this.BRLabel.Text = "(360.00^, 400.00 mm)";
			// 
			// TLLabel
			// 
			this.TLLabel.AutoSize = true;
			this.TLLabel.Location = new System.Drawing.Point(489, 73);
			this.TLLabel.Name = "TLLabel";
			this.TLLabel.Size = new System.Drawing.Size(147, 17);
			this.TLLabel.TabIndex = 19;
			this.TLLabel.Text = "(360.00^, 400.00 mm)";
			// 
			// TRLabel
			// 
			this.TRLabel.AutoSize = true;
			this.TRLabel.Location = new System.Drawing.Point(489, 102);
			this.TRLabel.Name = "TRLabel";
			this.TRLabel.Size = new System.Drawing.Size(147, 17);
			this.TRLabel.TabIndex = 20;
			this.TRLabel.Text = "(360.00^, 400.00 mm)";
			// 
			// RobotCalibrater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(648, 198);
			this.Controls.Add(this.TRLabel);
			this.Controls.Add(this.TLLabel);
			this.Controls.Add(this.BRLabel);
			this.Controls.Add(this.BLLabel);
			this.Controls.Add(this.ResetAll);
			this.Controls.Add(this.ResetTR);
			this.Controls.Add(this.ResetTL);
			this.Controls.Add(this.ResetBR);
			this.Controls.Add(this.ResetBL);
			this.Controls.Add(this.TRCalibrate);
			this.Controls.Add(this.TLCalibrate);
			this.Controls.Add(this.BRCalibrate);
			this.Controls.Add(this.BLCalibrate);
			this.Controls.Add(this.TRMoveTo);
			this.Controls.Add(this.TLMoveTo);
			this.Controls.Add(this.BRMoveTo);
			this.Controls.Add(this.BLMoveTo);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RobotCalibrater";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "RobotCalibrater";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RobotCalibrater_FormClosing);
			this.Load += new System.EventHandler(this.RobotCalibrater_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RobotCalibrater_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RobotCalibrater_KeyUp);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button BLMoveTo;
		private System.Windows.Forms.Button BRMoveTo;
		private System.Windows.Forms.Button TLMoveTo;
		private System.Windows.Forms.Button TRMoveTo;
		private System.Windows.Forms.Button BLCalibrate;
		private System.Windows.Forms.Button BRCalibrate;
		private System.Windows.Forms.Button TLCalibrate;
		private System.Windows.Forms.Button TRCalibrate;
		private System.Windows.Forms.Button ResetBL;
		private System.Windows.Forms.Button ResetBR;
		private System.Windows.Forms.Button ResetTL;
		private System.Windows.Forms.Button ResetTR;
		private System.Windows.Forms.Button ResetAll;
		private System.Windows.Forms.Label BLLabel;
		private System.Windows.Forms.Label BRLabel;
		private System.Windows.Forms.Label TLLabel;
		private System.Windows.Forms.Label TRLabel;
	}
}