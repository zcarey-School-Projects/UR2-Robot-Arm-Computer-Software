using System.Drawing;

namespace RobotArmUR2.VisionProcessing {
	public class PaperCalibration {
		//TODO put text next to paper clibration points

		public PaperCalibrationPoint BottomLeft { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointBLX), nameof(Properties.Settings.Default.PaperPointBLY));
		public PaperCalibrationPoint TopLeft { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointTLX), nameof(Properties.Settings.Default.PaperPointTLY));
		public PaperCalibrationPoint TopRight { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointTRX), nameof(Properties.Settings.Default.PaperPointTRY));
		public PaperCalibrationPoint BottomRight { get; } = new PaperCalibrationPoint(nameof(Properties.Settings.Default.PaperPointBRX), nameof(Properties.Settings.Default.PaperPointBRY));

		public PaperCalibration(){
			
		}

		public void SaveSettings() {
			BottomLeft.Save();
			TopLeft.Save();
			TopRight.Save();
			BottomRight.Save();

			Properties.Settings.Default.Save();
		}

		public void ResetToDefault() {
			BottomLeft.ResetToDefault();
			TopLeft.ResetToDefault();
			TopRight.ResetToDefault();
			BottomRight.ResetToDefault();
		}

		/// <summary>
		/// Puts paper points into an array.
		/// Returns {BottomLeft, TopLeft, TopRight, BottomRight}
		/// </summary>
		/// <returns></returns>
		public PaperPoint[] ToArray() {
			return new PaperPoint[] {
				BottomLeft,
				TopLeft,
				TopRight,
				BottomRight
			};
		}

		/// <summary>
		/// Returns image coordinates in array format. Same as calling GetScreenCoord(size);
		/// Returns {BottomLeft, TopLeft, TopRight, BottomRight}
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public PointF[] ToArray(Size size) {
			return new PointF[]{
				BottomLeft.GetScreenCoord(size),
				TopLeft.GetScreenCoord(size),
				TopRight.GetScreenCoord(size),
				BottomRight.GetScreenCoord(size)
			};
		}

	}

}
