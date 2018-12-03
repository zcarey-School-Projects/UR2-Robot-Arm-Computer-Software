using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public static class ApplicationSettings {

		#region Visuals
		private static Bgr trigColor = new Bgr(Color.Yellow);
		public static Bgr TriangleHighlightColor { get => new Bgr(trigColor.Blue, trigColor.Green, trigColor.Red); }

		private static Bgr squareColor = new Bgr(Color.Red);
		public static Bgr SquareHighlightColor { get => new Bgr(squareColor.Blue, squareColor.Green, squareColor.Red); }

		public static int ShapeHighlightThickness { get; } = 3;

		private static Bgr paperMaskColor = new Bgr(42, 240, 247);
		public static Bgr PaperROIMaskColor { get => new Bgr(paperMaskColor.Blue, paperMaskColor.Green, paperMaskColor.Red); }

		public static float PaperROIMaskTransparency { get; } = 0.2f;

		private static Bgr paperCircleColor = new Bgr(42, 240, 247);
		public static Bgr PaperROICircleColor { get => new Bgr(paperCircleColor.Blue, paperCircleColor.Green, paperCircleColor.Red); }

		public static int PaperROICircleThickness { get; } = 3;

		public static int PaperROICircleRadius { get; } = 10;

		public static FontFace PaperROIFont { get; } = FontFace.HersheyPlain;

		private static Bgr paperFontColor = new Bgr();
		public static Bgr PaperROIFontColor { get => new Bgr(paperFontColor.Blue, paperFontColor.Green, paperFontColor.Red); }

		public static double PaperROIFontScale { get; } = 1;

		public static int PaperROIFontThickness { get; } = 2;
		#endregion

		#region Keybindings
		public static Keys Keybind_RotateCCW { get; } = Keys.A;
		public static Keys Keybind_RotateCW { get; } = Keys.D;
		public static Keys Keybind_ExtendOutward { get; } = Keys.W;
		public static Keys Keybind_ExtendInward { get; } = Keys.S;
		public static Keys Keybind_LowerServo { get; } = Keys.Q;
		public static Keys Keybind_RaiseServo { get; } = Keys.E;
		public static Keys Keybind_MagnetOn { get; } = Keys.M;
		public static Keys Keybind_MagnetOff { get; } = Keys.N;
		#endregion

		public static readonly PaperCalibration PaperCalibration = new PaperCalibration();
		public static readonly RobotCalibration RobotCalibration = new RobotCalibration();

		public static readonly Setting<byte> BasePrescale = new Setting<byte>(nameof(Properties.Settings.Default.BasePrescale));
		public static readonly Setting<byte> CarriagePrescale = new Setting<byte>(nameof(Properties.Settings.Default.CarriagePrescale));

		public static void SaveSettings() {
			Properties.Settings.Default.Save();
		}

	}
}
