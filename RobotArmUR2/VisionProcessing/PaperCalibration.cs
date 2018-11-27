using System.Drawing;

namespace RobotArmUR2.VisionProcessing {
	public class PaperCalibration {
		//TODO put text next to paper clibration points

		private PointF ptBL;
		private PointF ptBR;
		private PointF ptTL;
		private PointF ptTR;

		public PointF BL { get => ptBL; set { if (value != null) ptBL = value; } }
		public PointF BR { get => ptBR; set { if (value != null) ptBR = value; } }
		public PointF TL { get => ptTL; set { if (value != null) ptTL = value; } }
		public PointF TR { get => ptTR; set { if (value != null) ptTR = value; } }

		public PaperCalibration(){
			ptBL = new PointF(0, 1);
			ptBR = new PointF(1, 1);
			ptTL = new PointF(0, 0);
			ptTR = new PointF(1, 0);
		}

		public PaperCalibration(PointF BL, PointF BR, PointF TL, PointF TR) {
			this.BL = new PointF(BL.X, BL.Y);
			this.BR = new PointF(BR.X, BR.Y);
			this.TL = new PointF(TL.X, TL.Y);
			this.TR = new PointF(TR.X, TR.Y);
		}

		public PaperCalibration(PaperCalibration deepCopy) : this(deepCopy.BL, deepCopy.BR, deepCopy.TL, deepCopy.TR) {}

		public PaperCalibration ShallowCopy() {
			PaperCalibration paper = new PaperCalibration();
			paper.BL = BL;
			paper.BR = BR;
			paper.TL = TL;
			paper.TR = TR;

			return paper;
		}

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
			Properties.Settings.Default.PaperPoint0X = BL.X;
			Properties.Settings.Default.PaperPoint0Y = BL.Y;
			Properties.Settings.Default.PaperPoint1X = TL.X;
			Properties.Settings.Default.PaperPoint1Y = TL.Y;
			Properties.Settings.Default.PaperPoint2X = TR.X;
			Properties.Settings.Default.PaperPoint2Y = TR.Y;
			Properties.Settings.Default.PaperPoint3X = BR.X;
			Properties.Settings.Default.PaperPoint3Y = BR.Y;
			Properties.Settings.Default.Save();
		}

		public void LoadSettings() {
			ptBL.X = Properties.Settings.Default.PaperPoint0X;
			ptBL.Y = Properties.Settings.Default.PaperPoint0Y;
			ptTL.X = Properties.Settings.Default.PaperPoint1X;
			ptTL.Y = Properties.Settings.Default.PaperPoint1Y;
			ptTR.X = Properties.Settings.Default.PaperPoint2X;
			ptTR.Y = Properties.Settings.Default.PaperPoint2Y;
			ptBR.X = Properties.Settings.Default.PaperPoint3X;
			ptBR.Y = Properties.Settings.Default.PaperPoint3Y;
		}
	}

	public enum CalibrationPoint {
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight
	}
}
