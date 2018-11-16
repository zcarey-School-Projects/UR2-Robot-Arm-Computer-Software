using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2 {
	public class RobotPoint {

		public float Rotation;
		public float Extension;

		public RobotPoint() : this(0, 0) { }

		public RobotPoint(float rotation, float extension) {
			this.Rotation = rotation;
			this.Extension = extension;
		}

		//Deep copy
		public RobotPoint(RobotPoint point) {
			Rotation = point.Rotation;
			Extension = point.Extension;
		}

	}
}
