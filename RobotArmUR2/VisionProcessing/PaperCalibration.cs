using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {
	public class PaperCalibration {

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

		public void SortPointOrder() {
			PointF center = calculateCenter();
			PointF temp;
			//We are basically manually bubble sorting the points
			//TODO this should not be a thing.
			if (isPointLess(center, TL, BL)) { //If TL is less than BL
				temp = TL;
				TL = BL;
				BL = temp;
			}
			if(isPointLess(center, TR, TL)) {
				temp = TR;
				TR = TL;
				TL = temp;
			}
			if(isPointLess(center, BR, TR)) {
				temp = BR;
				BR = TR;
				TR = temp;
			}
			if (isPointLess(center, TR, TL)) {
				temp = TR;
				TR = TL;
				TL = temp;
			}
			if (isPointLess(center, BR, TR)) {
				temp = BR;
				BR = TR;
				TR = temp;
			}
			if (isPointLess(center, TR, TL)) {
				temp = TR;
				TR = TL;
				TL = temp;
			}
		}

		private PointF calculateCenter() {
			float x = (BL.X + BR.X + TL.X + TR.X) / 4f;
			float y = (BL.Y + BR.Y + TL.Y + TR.Y) / 4f;

			return new PointF(x, y);
		}

		private static bool isPointLess(PointF center, PointF a, PointF b) {
			if (a.X - center.X <= 0 && b.X - center.X > 0)
				return true;
			if (a.X - center.X > 0 && b.X - center.X <= 0)
				return false;
			if (a.X - center.X == 0 && b.X - center.X == 0) {
				if (a.Y - center.Y >= 0 || b.Y - center.Y >= 0)
					return a.Y < b.Y;
				return b.Y < a.Y;
			}

			// compute the cross product of vectors (center -> a) x (center -> b)
			float det = (a.X - center.X) * (b.Y - center.Y) - (b.X - center.X) * (a.Y - center.Y);
			if (det > 0)
				return true;
			if (det < 0)
				return false;

			// points a and b are on the same line from the center
			// check which point is closer to the center
			float d1 = (a.X - center.X) * (a.X - center.X) + (a.Y - center.Y) * (a.Y - center.Y);
			float d2 = (b.X - center.X) * (b.X - center.X) + (b.Y - center.Y) * (b.Y - center.Y);
			return d1 < d2;
		}

		/*
		private static bool isPointLess(PointF center, PointF a, PointF b) {
			if (a.X - center.X >= 0 && b.X - center.X < 0)
				return true;
			if (a.X - center.X < 0 && b.X - center.X >= 0)
				return false;
			if (a.X - center.X == 0 && b.X - center.X == 0) {
				if (a.Y - center.Y >= 0 || b.Y - center.Y >= 0)
					return a.Y > b.Y;
				return b.Y > a.Y;
			}

			// compute the cross product of vectors (center -> a) x (center -> b)
			float det = (a.X - center.X) * (b.Y - center.Y) - (b.X - center.X) * (a.Y - center.Y);
			if (det < 0)
				return true;
			if (det > 0)
				return false;

			// points a and b are on the same line from the center
			// check which point is closer to the center
			float d1 = (a.X - center.X) * (a.X - center.X) + (a.Y - center.Y) * (a.Y - center.Y);
			float d2 = (b.X - center.X) * (b.X - center.X) + (b.Y - center.Y) * (b.Y - center.Y);
			return d1 > d2;
		}
		*/
	}

	public enum CalibrationPoint {
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight
	}
}
