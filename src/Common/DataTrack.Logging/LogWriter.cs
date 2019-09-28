using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DataTrack.Logging
{
	internal static class LogWriter
	{
		private static volatile Dictionary<LogConfiguration, List<LogItem>> logs = new Dictionary<LogConfiguration, List<LogItem>>();
		private static volatile Dictionary<LogConfiguration, int> logSize = new Dictionary<LogConfiguration, int>();

		private static Thread loggingThread;
		private static volatile bool shouldExecute;
		private static volatile bool logBufferInUse;

		private static readonly object configLock = new object();
		private static readonly object logBufferInUseLock = new object();
		private static readonly object shouldExecuteLock = new object();

		internal static void Init(LogConfiguration logConfig)
		{
			lock (configLock)
			{
				if (!logs.ContainsKey(logConfig))
				{
					Clear(logConfig);
					logs.Add(logConfig, new List<LogItem>());
					logSize.Add(logConfig, 0);
					Create(logConfig);
				}
			}

			logBufferInUse = true;
			shouldExecute = true;

			if (loggingThread == null)
			{
				loggingThread = new Thread(new ThreadStart(Logging));
				loggingThread.Start();
			}
		}

		internal static void Log(LogConfiguration config, MethodBase? method, string message, LogLevel level)
		{
			lock (configLock)
			{
				// If we race, we may hit this lock first. So we should always make sure that the buffer
				// is initialized before we use it.
				if (!logs.ContainsKey(config))
				{
					logs.Add(config, new List<LogItem>());
					logSize.Add(config, 0);
				}

				if (level < config.LogLevel)
				{
					return;
				}

				logs[config].Add(new LogItem(method, message, level));
			}

			lock (logBufferInUseLock)
			{
				logBufferInUse = true;
			}
		}
		internal static void Stop()
		{
			EndExecution();
		}

		private static void Create(LogConfiguration config)
		{
			lock (configLock)
			{
				if (logSize[config] < config.MaxFileSize && logSize[config] != 0)
				{
					return;
				}

				config.CreateLogFile();

				logSize[config] = 0;
			}
		}

		private static void Clear(LogConfiguration config)
		{
			config.DeleteLogFiles();
		}

		private static void Logging()
		{
			List<LogItem> threadLogBuffer = new List<LogItem>();

			while (ShouldExecute())
			{
				Thread.Sleep(100);

				List<LogConfiguration> tempConfigs = null;
				bool isActive = false;

				lock (configLock)
				{
					tempConfigs = new List<LogConfiguration>(logs.Keys);
				}

				foreach (LogConfiguration config in tempConfigs)
				{
					threadLogBuffer = GetLogBuffer(config);

					foreach (LogItem log in threadLogBuffer)
					{
						Output(config, log);
					}

					isActive |= threadLogBuffer.Count > 0;
				}

				lock (logBufferInUseLock)
				{
					logBufferInUse = isActive;
				}
			}
		}

		private static List<LogItem> GetLogBuffer(LogConfiguration config)
		{
			List<LogItem>? threadLogBuffer = null;

			lock (configLock)
			{
				threadLogBuffer = new List<LogItem>(logs[config]);
				logs[config].Clear();
			}

			return threadLogBuffer ?? new List<LogItem>();
		}

		private static void Output(LogConfiguration config, LogItem log)
		{
			string logOutput = log.ToString();

			lock (configLock)
			{
				using (StreamWriter writer = new StreamWriter(config.GetFullPath(), true))
				{
					writer.WriteLine(logOutput);
					logSize[config] = logSize[config] + 1;
				}
			}

			Create(config);
		}

		private static bool ShouldExecute()
		{
			lock (shouldExecuteLock)
			{
				return shouldExecute;
			}
		}

		private static bool LoggingInProgress()
		{
			lock (logBufferInUseLock)
			{
				return logBufferInUse;
			}
		}

		private static void EndExecution()
		{
			while (LoggingInProgress())
			{
				continue;
			}

			lock (shouldExecuteLock)
			{
				shouldExecute = false;
			}
		}
	}
}