using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotArmUR2.Util {

	/// <summary>Estimates the Frames Per Second being achieved, averaged across a given number of frames.
	/// MUST call Tick() every single frame, or else will not return accurate result.</summary>
	public class FPSCounter {

		/// <summary>The last calculated value of the FPS.</summary>
		public float FPS { get; private set; }
		//TODO use timer, more accurate
		/// <summary>The number of frames to average</summary>
		private int fpsFramesToAverage;

		/// <summary>The times calculated for each given frame</summary>
		private Queue<DateTime> frameTimes;

		/// <summary>Initializes the FPS counter</summary>
		/// <param name="NumFramesToAverage">The number of frames to average the FPS across. Default to 60</param>
		public FPSCounter(int NumFramesToAverage = 60) {
			fpsFramesToAverage = NumFramesToAverage;
			frameTimes = new Queue<DateTime>(fpsFramesToAverage);
		}

		/// <summary>Must call every frame, preferrably at the same point in the code.
		/// Calculates the new FPS based off the new frame and returns the value.</summary>
		/// <returns>The new FPS</returns>
		public float Tick() {
			DateTime currentTime = DateTime.UtcNow;
			
			if (frameTimes.Count == fpsFramesToAverage) frameTimes.Dequeue();
			frameTimes.Enqueue(currentTime);
			//I realize now this isn't that great.
			if(frameTimes.Count < 2) {
				return FPS = 0f;
			} else {
				float deltaTime = (float)frameTimes.Last().Subtract(frameTimes.First()).TotalSeconds;
				return FPS = (frameTimes.Count / deltaTime);
			}
		}

		/// <summary>Clears stored frame times and sets the FPS to zero. </summary>
		public void Reset() {
			frameTimes.Clear();
		}

	}
}
