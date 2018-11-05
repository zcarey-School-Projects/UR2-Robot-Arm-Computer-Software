using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotArmUR2 {
	public class RobotCalibration {

		public static float Angle1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotRotation"].DefaultValue);
		public static float Angle2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotRotation"].DefaultValue);
		public static float Angle3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotRotation"].DefaultValue);
		public static float Angle4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotRotation"].DefaultValue);

		public static float Distance1Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BLRobotDistance"].DefaultValue);
		public static float Distance2Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TLRobotDistance"].DefaultValue);
		public static float Distance3Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TRRobotDistance"].DefaultValue);
		public static float Distance4Default { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["BRRobotDistance"].DefaultValue);

		public static float TriangleStackAngleDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TrianglePileAngle"].DefaultValue);
		public static float TriangleStackDistanceDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["TrianglePileDistance"].DefaultValue);

		public static float SquareStackAngleDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["SquarePileAngle"].DefaultValue);
		public static float SquareStackDistanceDefault { get; private set; } = float.Parse((string)Properties.Settings.Default.Properties["SquarePileDistance"].DefaultValue);

		public float Angle1 { get; set; }
		public float Angle2 { get; set; }
		public float Angle3 { get; set; }
		public float Angle4 { get; set; }

		public float Distance1 { get; set; }
		public float Distance2 { get; set; }
		public float Distance3 { get; set; }
		public float Distance4 { get; set; }

		public float TriangleStackAngle { get; set; }
		public float TriangleStackDistance { get; set; }

		public float SquareStackAngle { get; set; }
		public float SquareStackDistance { get; set; }


		public RobotCalibration() {
			Angle1 = Properties.Settings.Default.BLRobotRotation;
			Angle2 = Properties.Settings.Default.TLRobotRotation;
			Angle3 = Properties.Settings.Default.TRRobotRotation;
			Angle4 = Properties.Settings.Default.BRRobotRotation;

			Distance1 = Properties.Settings.Default.BLRobotDistance;
			Distance2 = Properties.Settings.Default.TLRobotDistance;
			Distance3 = Properties.Settings.Default.TRRobotDistance;
			Distance4 = Properties.Settings.Default.BRRobotDistance;

			TriangleStackAngle = Properties.Settings.Default.TrianglePileAngle;
			TriangleStackDistance = Properties.Settings.Default.TrianglePileDistance;

			SquareStackAngle = Properties.Settings.Default.SquarePileAngle;
			SquareStackDistance = Properties.Settings.Default.SquarePileDistance;
		}

		public void SaveSettings() {
			Properties.Settings.Default.BLRobotRotation = Angle1;
			Properties.Settings.Default.TLRobotRotation = Angle2;
			Properties.Settings.Default.TRRobotRotation = Angle3;
			Properties.Settings.Default.BRRobotRotation = Angle4;

			Properties.Settings.Default.BLRobotDistance = Distance1;
			Properties.Settings.Default.TLRobotDistance = Distance2;
			Properties.Settings.Default.TRRobotDistance = Distance3;
			Properties.Settings.Default.BRRobotDistance = Distance4;

			Properties.Settings.Default.TrianglePileAngle = TriangleStackAngle;
			Properties.Settings.Default.TrianglePileDistance = TriangleStackDistance;

			Properties.Settings.Default.SquarePileAngle = SquareStackAngle;
			Properties.Settings.Default.SquarePileDistance = SquareStackDistance;

			Properties.Settings.Default.Save();
		}

	}
}
