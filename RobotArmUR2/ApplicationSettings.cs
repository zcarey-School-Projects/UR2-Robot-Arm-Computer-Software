using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;

namespace RobotArmUR2 {
	public static class ApplicationSettings {

		public static Bgr SquareHighlightColor = new Bgr(Color.Red);
		public static int SquareHighlightThickness = 3;

		public static Bgr TriangleHighlightColor = new Bgr(Color.Yellow);
		public static int TriangleHighlightThickness = 3;

		//Keybindings
		public const Keys Key_RotateCCW = Keys.A;
		public const Keys Key_RotateCW = Keys.D;
		public const Keys Key_ExtendOutward = Keys.W;
		public const Keys Key_ExtendInward = Keys.S;
		public const Keys Key_RaiseServo = Keys.E;
		public const Keys Key_LowerServo = Keys.Q;
		public const Keys Key_MagnetOn = Keys.M;
		public const Keys Key_MagnetOff = Keys.N;

	}
}
