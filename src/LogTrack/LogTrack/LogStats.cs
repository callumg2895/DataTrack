using System;
using System.Collections.Generic;
using System.Text;

namespace LogTrack
{
	public class LogStats
	{
		public int totalTRC { get; private set; }
		public int totalDBG { get; private set; }
		public int totalINF { get; private set; }
		public int totalWRN { get; private set; }
		public int totalERR { get; private set; }
		public int totalERF { get; private set; }

		public LogStats()
		{
			totalTRC = 0;
			totalDBG = 0;
			totalINF = 0;
			totalWRN = 0;
			totalERR = 0;
			totalERF = 0;
		}

		public void Update(LogStatement statement)
		{

			if (statement.LogLevel == LogLevel.Trace)
			{
				totalTRC++;
			}

			if (statement.LogLevel == LogLevel.Debug)
			{
				totalDBG++;
			}

			if (statement.LogLevel == LogLevel.Info)
			{
				totalINF++;
			}

			if (statement.LogLevel == LogLevel.Warning)
			{
				totalWRN++;
			}

			if (statement.LogLevel == LogLevel.Error)
			{
				totalERR++;
			}

			if (statement.LogLevel == LogLevel.Fatal)
			{
				totalERF++;
			}
		}
			
	}
}
