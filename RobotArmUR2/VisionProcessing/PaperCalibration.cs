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

		public PointF[] ToArray() {
			return new PointF[] {
				BottomLeft,
				TopLeft,
				TopRight,
				BottomRight
			};
		}

		/*
		public PointF[] ToArray() {
			return ToArray(2, 2);
		}

		public PointF[] ToArray(int width, int height) {
			width -= 1;
			height -= 1;
			return new PointF[] { new PointF(BL.X * width, BL.Y * height), new PointF(TL.X * width, TL.Y * height), new PointF(TR.X * width, TR.Y * height), new PointF(BR.X * width, BR.Y * height) };
		}

		public PointF[] ToArray(Size size) {
			return ToArray(size.Width, size.Height);
		}

		public PointF GetPoint(CalibrationPoint point) {
			switch (point) {
				case CalibrationPoint.BottomLeft: return BL;
				case CalibrationPoint.TopLeft: return TL;
				case CalibrationPoint.TopRight: return TR;
				case CalibrationPoint.BottomRight: return BR;
				default: return new PointF(); //Should never happen.
			}
		}

		public bool SetPoint(CalibrationPoint point, PointF value) {
			if (value == null) return false;
			switch (point) {
				case CalibrationPoint.BottomLeft: BL = value; return true;
				case CalibrationPoint.TopLeft: TL = value; return true;
				case CalibrationPoint.TopRight: TR = value; return true;
				case CalibrationPoint.BottomRight: BR = value; return true;
				default: return false; //Should never happen.
			}
		}
		//TODO load/save bug. points save in wrong order
		public void SaveSettings() {
			Properties.Settings.Default.PaperPointBLX = BL.X;
			Properties.Settings.Default.PaperPointBLY = BL.Y;
			Properties.Settings.Default.PaperPointTLX = TL.X;
			Properties.Settings.Default.PaperPointTLY = TL.Y;
			Properties.Settings.Default.PaperPointTRX = TR.X;
			Properties.Settings.Default.PaperPointTRY = TR.Y;
			Properties.Settings.Default.PaperPointBRX = BR.X;
			Properties.Settings.Default.PaperPointBRY = BR.Y;
			Properties.Settings.Default.Save();
		}

		public void LoadSettings() {
			ptBL.X = Properties.Settings.Default.PaperPointBLX;
			ptBL.Y = Properties.Settings.Default.PaperPointBLY;
			ptTL.X = Properties.Settings.Default.PaperPointTLX;
			ptTL.Y = Properties.Settings.Default.PaperPointTLY;
			ptTR.X = Properties.Settings.Default.PaperPointTRX;
			ptTR.Y = Properties.Settings.Default.PaperPointTRY;
			ptBR.X = Properties.Settings.Default.PaperPointBRX;
			ptBR.Y = Properties.Settings.Default.PaperPointBRY;
		}
		*/
	}

}
