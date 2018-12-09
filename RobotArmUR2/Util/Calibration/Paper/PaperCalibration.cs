using System.Drawing;

namespace RobotArmUR2.Util.Calibration.Paper {

	/// <summary>Calibration for the paper mask, saved/loaded from persistant storage. </summary>
	public class PaperCalibration {

		public PaperCalibrationPoint BottomLeft { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointBLX), nameof(Properties.Settings.Default.PaperPointBLY));
		public PaperCalibrationPoint TopLeft { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointTLX), nameof(Properties.Settings.Default.PaperPointTLY));
		public PaperCalibrationPoint TopRight { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointTRX), nameof(Properties.Settings.Default.PaperPointTRY));
		public PaperCalibrationPoint BottomRight { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointBRX), nameof(Properties.Settings.Default.PaperPointBRY));

		public PaperCalibration(){
			
		}

		/// <summary>Saved all points to persistant storage.</summary>
		public void SaveSettings() {
			BottomLeft.Save();
			TopLeft.Save();
			TopRight.Save();
			BottomRight.Save();

			ApplicationSettings.SaveSettings();
		}

		/// <summary>Resets all points to their defaults.</summary>
		public void ResetToDefault() {
			BottomLeft.ResetToDefault();
			TopLeft.ResetToDefault();
			TopRight.ResetToDefault();
			BottomRight.ResetToDefault();
		}

		/// <summary>Puts paper points into an array. Returns {BottomLeft, TopLeft, TopRight, BottomRight} </summary>
		/// <returns></returns>
		public PaperPoint[] ToArray() {
			return new PaperPoint[] {
				BottomLeft,
				TopLeft,
				TopRight,
				BottomRight
			};
		}

		/// <summary> Returns image coordinates in array format. Same as calling GetScreenCoord(size); Returns {BottomLeft, TopLeft, TopRight, BottomRight} </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public Point[] ToArray(Size size) {
			return new Point[]{
				Point.Round(BottomLeft.GetScreenCoord(size)),
				Point.Round(TopLeft.GetScreenCoord(size)),
				Point.Round(TopRight.GetScreenCoord(size)),
				Point.Round(BottomRight.GetScreenCoord(size))
			};
		}

		/// <summary>  Puts the points into array format, multiplying them by the width and height.</summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public PointF[] ToArray(int width, int height) {
			Size size = new Size(width, height);
			return new PointF[]{
				BottomLeft.GetScreenCoord(size),
				TopLeft.GetScreenCoord(size),
				TopRight.GetScreenCoord(size),
				BottomRight.GetScreenCoord(size)
			};
		}

	}

}
