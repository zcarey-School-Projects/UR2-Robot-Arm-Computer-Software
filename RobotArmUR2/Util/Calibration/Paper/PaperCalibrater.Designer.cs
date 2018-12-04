
namespace RobotArmUR2.Util.Calibration.Paper {
	partial class PaperCalibrater {
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
			this.PaperPicture = new System.Windows.Forms.PictureBox();
			this.ResetBounds = new System.Windows.Forms.Button();
			this.AutoDetect = new System.Windows.Forms.Button();
			this.PaperCoords = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.PaperPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// PaperPicture
			// 
			this.PaperPicture.Location = new System.Drawing.Point(0, 0);
			this.PaperPicture.Name = "PaperPicture";
			this.PaperPicture.Size = new System.Drawing.Size(640, 480);
			this.PaperPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.PaperPicture.TabIndex = 0;
			this.PaperPicture.TabStop = false;
			this.PaperPicture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PaperPicture_MouseDown);
			this.PaperPicture.MouseLeave += new System.EventHandler(this.PaperPicture_MouseLeave);
			this.PaperPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PaperPicture_MouseMove);
			this.PaperPicture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PaperPicture_MouseUp);
			// 
			// ResetBounds
			// 
			this.ResetBounds.Location = new System.Drawing.Point(646, 12);
			this.ResetBounds.Name = "ResetBounds";
			this.ResetBounds.Size = new System.Drawing.Size(75, 23);
			this.ResetBounds.TabIndex = 1;
			this.ResetBounds.Text = "Reset";
			this.ResetBounds.UseVisualStyleBackColor = true;
			this.ResetBounds.Click += new System.EventHandler(this.ResetBounds_Click);
			// 
			// AutoDetect
			// 
			this.AutoDetect.Location = new System.Drawing.Point(646, 56);
			this.AutoDetect.Name = "AutoDetect";
			this.AutoDetect.Size = new System.Drawing.Size(75, 23);
			this.AutoDetect.TabIndex = 2;
			this.AutoDetect.Text = "Auto";
			this.AutoDetect.UseVisualStyleBackColor = true;
			this.AutoDetect.Click += new System.EventHandler(this.AutoDetect_Click);
			// 
			// PaperCoords
			// 
			this.PaperCoords.AutoSize = true;
			this.PaperCoords.Location = new System.Drawing.Point(646, 158);
			this.PaperCoords.Name = "PaperCoords";
			this.PaperCoords.Size = new System.Drawing.Size(46, 17);
			this.PaperCoords.TabIndex = 3;
			this.PaperCoords.Text = "label1";
			// 
			// PaperCalibrater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(999, 489);
			this.Controls.Add(this.PaperCoords);
			this.Controls.Add(this.AutoDetect);
			this.Controls.Add(this.ResetBounds);
			this.Controls.Add(this.PaperPicture);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "PaperCalibrater";
			this.Text = "PaperCalibrater";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PaperCalibrater_FormClosing);
			this.Load += new System.EventHandler(this.PaperCalibrater_Load);
			((System.ComponentModel.ISupportInitialize)(this.PaperPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox PaperPicture;
		private System.Windows.Forms.Button ResetBounds;
		private System.Windows.Forms.Button AutoDetect;
		private System.Windows.Forms.Label PaperCoords;
	}
}