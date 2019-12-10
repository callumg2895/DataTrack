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
			Console.WriteLine($"Total TRACE: {stats.totalTRC}");
			Console.WriteLine($"Total DEBUG: {stats.totalDBG}");
			Console.WriteLine($"Total INFO: {stats.totalINF}");
			Console.WriteLine($"Total WARN: {stats.totalWRN}");
			Console.WriteLine($"Total ERROR: {stats.totalERR}");
			Console.WriteLine($"Total CRITICAL: {stats.totalERF}");
		}
	}
}
