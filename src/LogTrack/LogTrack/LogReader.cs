using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogTrack
{
	public class LogReader
	{
		private volatile List<LogStatement> logBuffer;
		private LogStats logStats;
		private string filePath;
		private string fileName;
		private bool parsing;
		private object parsingLock;

		public LogReader(string path, string name)
		{
			filePath = path;
			fileName = name;
			logBuffer = new List<LogStatement>();
			logStats = new LogStats();
			parsingLock = new object();

			lock (parsingLock)
			{
				parsing = false;
			}
		}

		public List<LogStatement> Read()
		{
			if (!Directory.Exists(filePath))
			{
				return logBuffer;
			}

			using (StreamReader reader = File.OpenText($"{filePath}/{fileName}"))
			{
				lock (parsingLock)
				{
					parsing = true;
				}

				Thread loadingBarThread = new Thread(new ThreadStart(LoadingBar));
				loadingBarThread.Start();

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
						lock (parsingLock)
						{
							parsing = false;
						}

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

		private void LoadingBar()
		{
			int maxLogSize = 10000;
			int maxLoadingBarSize = 50;

			while (true)
			{
				lock (parsingLock)
				{
					if (parsing)
					{
						Console.SetCursorPosition(1, 1);
						Console.Write("Parsing log file: ");

						int currentLogsParsed = Math.Max(logBuffer.Count(), 1);
						int percentComplete = (int)Math.Round((currentLogsParsed / (decimal)maxLogSize) * maxLoadingBarSize);

						for (int i = 0; i < percentComplete; i++)
						{
							Console.BackgroundColor = ConsoleColor.White;
							Console.Write(" ");
						}

						Console.BackgroundColor = ConsoleColor.Black;
						Console.WriteLine();
					}
					else
					{
						break;
					}
				}
			}
		}
	}
}
