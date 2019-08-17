using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogTrack
{
	class Program
	{
		static void Main(string[] args)
		{
			DataTrackConfigReader configReader = new DataTrackConfigReader();
			LogConfiguration logConfig = new LogConfiguration(configReader.GetLoggingConfigNode());

			LogReader reader = new LogReader(logConfig);

			List<LogStatement> logBuffer = reader.Read();

			foreach (LogStatement statement in logBuffer)
			{
				statement.Write();
			}

			LogStats stats = reader.ReadStats();

			Console.WriteLine();
			Console.WriteLine($"Total TRC: {stats.totalTRC}");
			Console.WriteLine($"Total DBG: {stats.totalDBG}");
			Console.WriteLine($"Total INF: {stats.totalINF}");
			Console.WriteLine($"Total WRN: {stats.totalWRN}");
			Console.WriteLine($"Total ERR: {stats.totalERR}");
			Console.WriteLine($"Total ERF: {stats.totalERF}");
		}
	}
}
