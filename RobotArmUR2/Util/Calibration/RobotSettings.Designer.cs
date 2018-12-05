namespace RobotArmUR2.Util.Calibration {
	partial class RobotSettings {
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
			this.CarriagePrescaleLabel = new System.Windows.Forms.Label();
			this.CarriagePrescaleSlider = new System.Windows.Forms.TrackBar();
			this.BasePrescaleLabel = new System.Windows.Forms.Label();
			this.BasePrescaleSlider = new System.Windows.Forms.TrackBar();
			this.SaveSettings = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.CarriagePrescaleSlider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BasePrescaleSlider)).BeginInit();
			this.SuspendLayout();
			// 
			// CarriagePrescaleLabel
			// 
			this.CarriagePrescaleLabel.AutoSize = true;
			this.CarriagePrescaleLabel.Location = new System.Drawing.Point(444, 74);
			this.CarriagePrescaleLabel.Name = "CarriagePrescaleLabel";
			this.CarriagePrescaleLabel.Size = new System.Drawing.Size(141, 17);
			this.CarriagePrescaleLabel.TabIndex = 38;
			this.CarriagePrescaleLabel.Text = "Carriage Prescale:  0";
			// 
			// CarriagePrescaleSlider
			// 
			this.CarriagePrescaleSlider.Location = new System.Drawing.Point(12, 74);
			this.CarriagePrescaleSlider.Maximum = 20;
			this.CarriagePrescaleSlider.Name = "CarriagePrescaleSlider";
			this.CarriagePrescaleSlider.Size = new System.Drawing.Size(426, 56);
			this.CarriagePrescaleSlider.TabIndex = 37;
			this.CarriagePrescaleSlider.Scroll += new System.EventHandler(this.CarriagePrescaleSlider_Scroll);
			// 
			// BasePrescaleLabel
			// 
			this.BasePrescaleLabel.AutoSize = true;
			this.BasePrescaleLabel.Location = new System.Drawing.Point(444, 12);
			this.BasePrescaleLabel.Name = "BasePrescaleLabel";
			this.BasePrescaleLabel.Size = new System.Drawing.Size(123, 17);
			this.BasePrescaleLabel.TabIndex = 36;
			this.BasePrescaleLabel.Text = "Base Prescale: 15";
			// 
			// BasePrescaleSlider
			// 
			this.BasePrescaleSlider.Location = new System.Drawing.Point(12, 12);
			this.BasePrescaleSlider.Maximum = 20;
			this.BasePrescaleSlider.Name = "BasePrescaleSlider";
			this.BasePrescaleSlider.Size = new System.Drawing.Size(426, 56);
			this.BasePrescaleSlider.TabIndex = 35;
			this.BasePrescaleSlider.Value = 15;
			this.BasePrescaleSlider.Scroll += new System.EventHandler(this.BasePrescaleSlider_Scroll);
			// 
			// SaveSettings
			// 
			this.SaveSettings.Location = new System.Drawing.Point(704, 146);
			this.SaveSettings.Name = "SaveSettings";
			this.SaveSettings.Size = new System.Drawing.Size(75, 23);
			this.SaveSettings.TabIndex = 39;
			this.SaveSettings.Text = "Save";
			this.SaveSettings.UseVisualStyleBackColor = true;
			this.SaveSettings.Click += new System.EventHandler(this.SaveSettings_Click);
			// 
			// RobotSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(791, 181);
			this.Controls.Add(this.SaveSettings);
			this.Controls.Add(this.CarriagePrescaleLabel);
			this.Controls.Add(this.CarriagePrescaleSlider);
			this.Controls.Add(this.BasePrescaleLabel);
			this.Controls.Add(this.BasePrescaleSlider);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "RobotSettings";
			this.Text = "RobotSettings";
			this.Load += new System.EventHandler(this.RobotSettings_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RobotSettings_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RobotSettings_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.CarriagePrescaleSlider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BasePrescaleSlider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label CarriagePrescaleLabel;
		private System.Windows.Forms.TrackBar CarriagePrescaleSlider;
		private System.Windows.Forms.Label BasePrescaleLabel;
		private System.Windows.Forms.TrackBar BasePrescaleSlider;
		private System.Windows.Forms.Button SaveSettings;
	}
}