using System;
using System.Diagnostics;

namespace DataTrack.Core.Util.Extensions
{
	public static class StopwatchExtension
	{
		public static double GetElapsedMicroseconds(this Stopwatch stopwatch)
		{
			double frequency = Stopwatch.Frequency;
			double ticks = stopwatch.ElapsedTicks;

			return Math.Round((ticks / frequency) * 1000000);
		}
	}
}
