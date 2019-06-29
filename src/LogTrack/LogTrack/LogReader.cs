using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogTrack
{
	public class LogReader
	{
		private List<LogStatement> logBuffer;
		private string filePath;
		private string fileName;

		public int totalTRC { get; private set;}
		public int totalDBG {get; private set;}
		public int totalINF {get; private set;}
		public int totalWRN {get; private set;}
		public int totalERR {get; private set;}
		public int totalERF {get; private set;}

		public LogReader(string path, string name)
		{
			filePath = path;
			fileName = name;
			logBuffer = new List<LogStatement>();

			totalTRC = 0;
			totalDBG = 0;
			totalINF = 0;
			totalWRN = 0;
			totalERR = 0;
			totalERF = 0;
		}

		public List<LogStatement> Read()
		{
			if (!Directory.Exists(filePath))
			{
				return logBuffer;
			}

			using (StreamReader reader = File.OpenText($"{filePath}/{fileName}"))
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

						logBuffer.Add(statement);
					}

					if (reader.EndOfStream)
					{
						break;
					}
				}
			}

			return logBuffer;
		}
	}
}
