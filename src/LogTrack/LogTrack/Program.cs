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
			string projectName = "DataTrack";
			string fileName = $"{projectName}Log_";
			string filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}";
			string fileExtension = ".txt";
			DateTime fileDate = DateTime.Now.Date;
			string fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			int fileIndex = 0;

			List<LogStatement> logBuffer = new List<LogStatement>();

			if (Directory.Exists(filePath))
			{
				using (StreamReader reader = File.OpenText($@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}"))
				{
					while (true)
					{
						LogStatement statement = new LogStatement(reader.ReadLine());

						if (statement.LogLevel == LogLevel.Unknown)
						{
							logBuffer.Last().Append(statement);
						}
						else
						{
							logBuffer.Add(statement);
						}

						if (reader.EndOfStream)
						{
							break;
						}
					}
				}
			}

			foreach (LogStatement statement in logBuffer)
			{
				statement.Write();
			}
		}
	}
}
