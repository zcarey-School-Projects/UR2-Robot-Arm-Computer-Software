﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2.VisionProcessing {
	public class PaperPoint {

		public float X { get; set; }
		public float Y { get; set; }

		public PaperPoint() : this(0, 0) { }

		public PaperPoint(float x, float y) {
			this.X = x;
			this.Y = y;
		}

		//Deep copy
		public PaperPoint(PaperPoint point) {
			X = point.X;
			Y = point.Y;
		}

		public Point GetScreenCoord(Size screenSize) {
			return new Point((int)(X * (screenSize.Width - 1) + 0.5f), (int)(Y * (screenSize.Height - 1) + 0.5f));
		}

		public Point GetClippedScreenCoord(Size screenSize) {
			Point coord = GetScreenCoord(screenSize);
			coord.X = Math.Max(0, Math.Min(screenSize.Width - 1, coord.X));
			coord.Y = Math.Max(0, Math.Min(screenSize.Height - 1, coord.Y));

			return coord;
		}

		public static implicit operator PointF(PaperPoint pt) {
			return new PointF(pt.X, pt.Y);
		}

		public static implicit operator PaperPoint(PointF pt) {
			return new PaperPoint(pt.X, pt.Y);
		}

	}
}
