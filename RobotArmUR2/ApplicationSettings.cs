using Emgu.CV.Structure;
using RobotArmUR2.VisionProcessing;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public static class ApplicationSettings {

		public static readonly Bgr TriangleHighlightColor = new Bgr(Color.Yellow);
		public static readonly Bgr SquareHighlightColor = new Bgr(Color.Red);
		public const int ShapeHighlightThickness = 3;

		//Keybindings
		public const Keys Key_RotateCCW = Keys.A;
		public const Keys Key_RotateCW = Keys.D;
		public const Keys Key_ExtendOutward = Keys.W;
		public const Keys Key_ExtendInward = Keys.S;
		public const Keys Key_RaiseServo = Keys.E;
		public const Keys Key_LowerServo = Keys.Q;
		public const Keys Key_MagnetOn = Keys.M;
		public const Keys Key_MagnetOff = Keys.N;

		public static readonly PaperCalibration PaperCalibration = new PaperCalibration();

		public static readonly Setting<byte> BasePrescale = new Setting<byte>(nameof(Properties.Settings.Default.BasePrescale));
		public static readonly Setting<byte> CarriagePrescale = new Setting<byte>(nameof(Properties.Settings.Default.CarriagePrescale));

		public static void SaveSettings() {
			Properties.Settings.Default.Save();
		}

	}
}
