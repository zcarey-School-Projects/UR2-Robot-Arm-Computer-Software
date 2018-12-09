using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RobotArmUR2.Util;
using RobotArmUR2.Util.Calibration.Paper;
using RobotArmUR2.Util.Calibration.Robot;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2 {

	/// <summary>Handles settings for saving, as well as application-wide settings / constants. </summary>
	public static class ApplicationSettings {

		/// <summary> After an image is grabbed, the height it will be scaled to for vision processing. Smaller numbers will run faster and with less memory, but also less accurate. </summary>
		public static double WorkingImageScaledHeight { get; } = 480d;

		#region Visuals
		/// <summary> Color of triangles when being drawn to output Shapes image. </summary>
		public static Bgr TriangleHighlightColor { get => new Bgr(trigColor.Blue, trigColor.Green, trigColor.Red); }
		private static Bgr trigColor = new Bgr(Color.Yellow);

		/// <summary> Color of squares when being drawn to output Shapes image. </summary>
		public static Bgr SquareHighlightColor { get => new Bgr(squareColor.Blue, squareColor.Green, squareColor.Red); }
		private static Bgr squareColor = new Bgr(Color.Red);

		/// <summary> Thickness of shapes when drawn to output Shapes image. </summary>
		public static int ShapeHighlightThickness { get; } = 3;

		#region Paper Calibrater
		/// <summary> The color of the ROI that is masked on top of input image. </summary>
		public static Bgr PaperROIMaskColor { get => new Bgr(paperMaskColor.Blue, paperMaskColor.Green, paperMaskColor.Red); }
		private static Bgr paperMaskColor = new Bgr(42, 240, 247);

		/// <summary> When ROI is masked on top of input image, the percentage visible the ROI should be. </summary>
		public static float PaperROIMaskTransparency { get; } = 0.2f;

		/// <summary> Color of circles drawn at corners of ROI in PaperCalibrater. </summary>
		public static Bgr PaperROICircleColor { get => new Bgr(paperCircleColor.Blue, paperCircleColor.Green, paperCircleColor.Red); }
		private static Bgr paperCircleColor = new Bgr(42, 240, 247);

		/// <summary> Thickness of circles drawn at corners of ROI in PaperCalibrater form. </summary>
		public static int PaperROICircleThickness { get; } = 3;

		/// <summary> Radius of circles drawn at corners of ROI in PaperCalibrater frrm.  </summary>
		public static int PaperROICircleRadius { get; } = 10;

		/// <summary> Font style of text drawn in PaperCalibrater form when selecting ROI. </summary>
		public static FontFace PaperROIFont { get; } = FontFace.HersheyPlain;

		/// <summary> Font color of text drawn in PaperCalibrater form when selecting ROI. </summary>
		public static Bgr PaperROIFontColor { get => new Bgr(paperFontColor.Blue, paperFontColor.Green, paperFontColor.Red); }
		private static Bgr paperFontColor = new Bgr();

		/// <summary> Font scale of text drawn in PaperCalibrater form when selecting ROI. </summary>
		public static double PaperROIFontScale { get; } = 1;

		/// <summary>Font thickness of text drawn in PaperCalibrater form when selecting ROI. </summary>
		public static int PaperROIFontThickness { get; } = 2;
		#endregion

		#endregion

		#region Keybindings
		/// <summary>Key to rotate the base of the robot counter clock-wise. </summary>
		public static Keys Keybind_RotateCCW { get; } = Keys.A;

		/// <summary>Key to rotate the base of the robot clock-wise. </summary>
		public static Keys Keybind_RotateCW { get; } = Keys.D;

		/// <summary>Key to move the carriage away from the base of the robot. </summary>
		public static Keys Keybind_ExtendOutward { get; } = Keys.W;

		/// <summary>Key to move the carriage towards the base of the robot.</summary>
		public static Keys Keybind_ExtendInward { get; } = Keys.S;

		/// <summary>Key to lower the servo holding the magnet. </summary>
		public static Keys Keybind_LowerServo { get; } = Keys.Q;

		/// <summary>Key to raise the servo holding the magnet.</summary>
		public static Keys Keybind_RaiseServo { get; } = Keys.E;
		
		/// <summary>Key that turns the magnet on.</summary>
		public static Keys Keybind_MagnetOn { get; } = Keys.M;

		/// <summary>Key that turns the magnet off.</summary>
		public static Keys Keybind_MagnetOff { get; } = Keys.N;
		#endregion

		/// <summary>The current calibration for the ROI of the paper.</summary>
		public static readonly PaperCalibration PaperCalibration = new PaperCalibration();

		/// <summary>The current calibration for the position of the robot.</summary>
		public static readonly RobotCalibration RobotCalibration = new RobotCalibration();

		/// <summary>The saved setting for the speed of the base motor.</summary>
		public static readonly Setting<byte> BasePrescale = new Setting<byte>(nameof(Properties.Settings.Default.BasePrescale));

		/// <summary>The saved setting for the speed of the carriage motor.</summary>
		public static readonly Setting<byte> CarriagePrescale = new Setting<byte>(nameof(Properties.Settings.Default.CarriagePrescale));

		/// <summary>Saves all settings to persistant storage by calling Properties.Settings.Default.Save().</summary>
		public static void SaveSettings() {
			Properties.Settings.Default.Save();
		}

		static ApplicationSettings() {
			Properties.Settings.Default.Reload(); //Just to be sure the saved data is loaded from storage.
		}
	}
}
