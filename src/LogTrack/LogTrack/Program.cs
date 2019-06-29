﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogTrack
{
	class Program
	{
		static void Main(string[] args)
		{
			string projectName = "DataTrack";
			string fileName = $"{projectName}Log_";
			string filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}";
			string fileExtension = ".txt";
			DateTime fileDate = DateTime.Now.Date;
			string fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			int fileIndex = 0;

			LogReader reader = new LogReader(filePath, $"{fileDateString}_{fileName}{fileIndex}{fileExtension}");

			List<LogStatement> logBuffer = reader.Read();

			foreach (LogStatement statement in logBuffer)
			{
				statement.Write();
			}

			Console.WriteLine();
			Console.WriteLine($"Total TRC: {reader.totalTRC}");
			Console.WriteLine($"Total DBG: {reader.totalDBG}");
			Console.WriteLine($"Total INF: {reader.totalINF}");
			Console.WriteLine($"Total WRN: {reader.totalWRN}");
			Console.WriteLine($"Total ERR: {reader.totalERR}");
			Console.WriteLine($"Total ERF: {reader.totalERF}");
		}
	}
}
