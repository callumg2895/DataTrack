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
		private LogStats logStats;
		private string filePath;
		private string fileName;

		public LogReader(string path, string name)
		{
			filePath = path;
			fileName = name;
			logBuffer = new List<LogStatement>();
			logStats = new LogStats();
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
						logStats.Update(statement);
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

		public LogStats ReadStats()
		{
			return logStats;
		}
	}
}
