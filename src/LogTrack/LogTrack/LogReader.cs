﻿using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogTrack
{
	public class LogReader
	{
		private volatile List<LogStatement> logBuffer;
		private volatile int maxLogSize;
		private volatile int maxLoadingBarSize;
		private volatile bool parsing;

		private LogStats logStats;
		private LogConfiguration logConfig;
		private int fileIndex;

		private object parsingLock = new object();

		public LogReader(LogConfiguration logConfiguration)
		{
			logBuffer = new List<LogStatement>();
			logStats = new LogStats();
			logConfig = logConfiguration;

			fileIndex = 0;
			maxLoadingBarSize = 10;

			lock (parsingLock)
			{
				parsing = false;
			}
		}

		public List<LogStatement> Read()
		{
			if (!Directory.Exists(logConfig.GetFilePath()))
			{
				return logBuffer;
			}

			while (File.Exists(logConfig.GetFullPath(fileIndex)))
			{
				ReadCurrentFile();
				fileIndex++;
			}

			return logBuffer;
		}

		public LogStats ReadStats()
		{
			return logStats;
		}

		private void ReadCurrentFile()
		{
			using (StreamReader reader = File.OpenText(logConfig.GetFullPath(fileIndex)))
			{
				lock (parsingLock)
				{
					parsing = true;
				}

				Thread loadingBarThread = new Thread(new ThreadStart(LoadingBar));
				bool running = true;

				loadingBarThread.Start();

				while (running)
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
							running = false;
							DisplayProgress();
						}
					}
				}
			}
		}

		private void LoadingBar()
		{
			bool running = true;

			while (running)
			{
				lock (parsingLock)
				{
					if (parsing)
					{
						DisplayProgress();
					}
					else
					{
						running = false;
					}
				}
			}
		}

		private void DisplayProgress()
		{
			TextFormat format = new TextFormat(ConsoleColor.Black, ConsoleColor.White);
			format.Reset();

			Console.SetCursorPosition(1, fileIndex);
			Console.Write($"Parsing log file {fileIndex}: [");

			format.Apply();

			int currentLogsParsed = Math.Max(logBuffer.Count(), 1) - (fileIndex * logConfig.MaxFileSize);
			int percentComplete = (int)Math.Round((currentLogsParsed / (decimal)logConfig.MaxFileSize) * maxLoadingBarSize);

			for (int i = 0; i < maxLoadingBarSize; i++)
			{
				if (i < percentComplete)
				{
					format.Apply();
					Console.Write(" ");
				}
				else
				{
					format.Reset();
					Console.Write(" ");
				}
			}

			format.Reset();
			Console.Write($"] {currentLogsParsed} entries loaded");
			Console.WriteLine();
		}
	}
}
