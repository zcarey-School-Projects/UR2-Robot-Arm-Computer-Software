using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotHelpers {
	public class FPSCounter {

		public float FPS { get; private set; }

		private int fpsFramesToAverage;
		private Queue<DateTime> frameTimes;

		public FPSCounter() : this(60) {}

		public FPSCounter(int NumFramesToAverage) {
			fpsFramesToAverage = NumFramesToAverage;
			frameTimes = new Queue<DateTime>(fpsFramesToAverage);
		}

		public float Tick() {
			DateTime currentTime = DateTime.UtcNow;
			
			if (frameTimes.Count == fpsFramesToAverage) frameTimes.Dequeue();
			frameTimes.Enqueue(currentTime);

			if(frameTimes.Count < 2) {
				return FPS = 0f;
			} else {
				float deltaTime = (float)frameTimes.Last().Subtract(frameTimes.First()).TotalSeconds;
				return FPS = (frameTimes.Count / deltaTime);
			}
		}

		public void Reset() {
			frameTimes.Clear();
		}

	}
}
