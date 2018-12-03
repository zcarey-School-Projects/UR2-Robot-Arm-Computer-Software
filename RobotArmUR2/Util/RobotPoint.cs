using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2 {
	public class RobotPoint {

		public float Rotation { get; set; }
		public float Extension { get; set; }

		public RobotPoint() : this(0, 0) { }

		public RobotPoint(float rotation, float extension) {
			this.Rotation = rotation;
			this.Extension = extension;
		}

		//Deep copy
		public RobotPoint(RobotPoint point) {
			SetPoint(point);
		}

		public void SetPoint(RobotPoint point) {
			Rotation = point.Rotation;
			Extension = point.Extension;
		}

		public override string ToString() {
			return "{ " + Rotation.ToString("N2") + "° , " + Extension.ToString("N2") + " mm }";
		}

		public string ToString(uint digits) {
			return "{ " + Rotation.ToString("N" + digits) + "° , " + Extension.ToString("N" + digits) + " mm }";
		}

	}
}
