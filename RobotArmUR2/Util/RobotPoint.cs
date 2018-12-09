
namespace RobotArmUR2.Util {

	/// <summary>Seperates the points used by images (PointF) and points meant to be used as robot positions.</summary>
	public class RobotPoint {

		/// <summary>The rotation of the robot, in degrees.</summary>
		public float Rotation { get; set; }

		/// <summary>The extension of the robot, in millimeters.</summary>
		public float Extension { get; set; }

		/// <summary>Creates a new point at 0, 0</summary>
		public RobotPoint() : this(0, 0) { }

		/// <summary>Creates a new point at the given coordinates.</summary>
		/// <param name="rotation"></param>
		/// <param name="extension"></param>
		public RobotPoint(float rotation, float extension) {
			this.Rotation = rotation;
			this.Extension = extension;
		}

		/// <summary> Creates a deep copy of the point, so change one won't effect the other. </summary>
		/// <param name="point"></param>
		public RobotPoint(RobotPoint point) {
			SetPoint(point);
		}

		/// <summary>Sets the position of this point equal to that of another point. Same as creating a deep copy.</summary>
		/// <param name="point"></param>
		public void SetPoint(RobotPoint point) {
			Rotation = point.Rotation;
			Extension = point.Extension;
		}

		/// <summary>Returns the position of the point in a string format.</summary>
		/// <returns></returns>
		public override string ToString() {
			return "{ " + Rotation.ToString("N2") + "° , " + Extension.ToString("N2") + " mm }";
		}

		/// <summary>Returns the position of the point in a string format with the specified digits.</summary>
		/// <param name="digits"></param>
		/// <returns></returns>
		public string ToString(uint digits) {
			return "{ " + Rotation.ToString("N" + digits) + "° , " + Extension.ToString("N" + digits) + " mm }";
		}

	}
}
