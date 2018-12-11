using RobotArmUR2.Util;
using RobotArmUR2.VisionProcessing;
using System.Threading;

namespace RobotArmUR2.RobotControl.Programs {

	/// <summary> The main stacking program. Will keep detecting, picking up, and stacking shapes until none are detected.</summary>
	public class StackingProgram : RobotProgram {

		/// <summary> The vision class that is grabbing frames.</summary>
		private Vision vision;

		/// <summary>The number of empty frames required for the program to be considred finished.</summary>
		private const int EmptyFramesNeeded = 20;

		/// <summary>Number of empty frames detected.</summary>
		private int emptyFrameCount = 0;

		public StackingProgram(Vision vision) {
			this.vision = vision;
		}

		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.Initialize(RobotInterface)"/>
		public override bool Initialize(RobotInterface serial) {
			if (vision.Images == null || vision.Images.Shapes == null) return false;
			if (!serial.RaiseServo()) return false;
			if (!serial.PowerMagnetOff()) return false;
			if (!serial.ReturnHome()) return false;
			Thread.Sleep(1000);
			return true;
		}
		
		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.ProgramStep(RobotInterface)"/>
		public override bool ProgramStep(RobotInterface serial) {
			VisionImages images = vision.Images; //Grab the most recently updated output images.
			if (images == null || images.Shapes == null) return false; //If no image, assume bad things and exit the program.
			DetectedShapes shapes = images.Shapes; 
			if (shapes.RelativeTrianglePoints.Count > 0) { //If there is a triangle, stack it.
				PaperPoint center = shapes.RelativeTrianglePoints[0];
				if (!stackShape(serial, center, true)) return false;
			} else if (shapes.RelativeSquarePoints.Count > 0) { //If there is a square, stack it.
				PaperPoint center = shapes.RelativeSquarePoints[0];
				if (!stackShape(serial, center, false)) return false;
			} else { //If there are no detected shapes, increase count and check foe exit.
				emptyFrameCount++;
				if (emptyFrameCount >= EmptyFramesNeeded) {
					return false;
				}
			}

			return true;
		}

		/// <summary>Given a shape center, will move robot to it, pick it up, and drop in proper stack.</summary>
		/// <param name="serial">Robot interface</param>
		/// <param name="center">Center of the shape.</param>
		/// <returns>false if there was an error.</returns>
		private bool stackShape(RobotInterface serial, PaperPoint center, bool isTriangle) {
			if (!moveRobotToPaperPoint(serial, center)) return false;
			if (!pickUpShape(serial, true)) return false;
			if (isTriangle) {
				if (!moveToTriangleStack(serial)) return false;
			} else {
				if (!moveToSquareStack(serial)) return false;
			}
			if (!pickUpShape(serial, false)) return false;
			emptyFrameCount = 0;
			return true;
		}

		/// <summary>Lowers servo, enables or disables magnet, then raises servo.</summary>
		/// <param name="serial">Robot interface</param>
		/// <param name="magnetOn">true will enable the magnet, false will disable it.</param>
		/// <returns>false if any errors ocurred.</returns>
		private bool pickUpShape(RobotInterface serial, bool magnetOn) {
			Thread.Sleep(1000);
			if (!serial.LowerServo()) return false;
			Thread.Sleep(1000);
			if(!serial.PowerMagnet(magnetOn)) return false;
			Thread.Sleep(1000);
			if (!serial.RaiseServo()) return false;
			Thread.Sleep(1000);

			return true;
		}

		/// <see cref="RobotArmUR2.RobotControl.RobotProgram.ProgramCancelled(RobotInterface)"/>
		public override void ProgramCancelled(RobotInterface serial) {
			serial.PowerMagnetOff(); //Drop any shape we may be holding.
		}

	}
}
