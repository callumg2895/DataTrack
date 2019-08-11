using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DataTrack.Logging
{
	public static class Logger
	{
		private static LogConfiguration config;

		private static LogLevel logLevel = LogLevel.Trace;
		private static int maxLogLength = 10000;
		private static int currentLength = 0;

		private static Thread loggingThread;
		private static volatile bool shouldExecute;
		private static volatile bool logBufferInUse;
		private static volatile bool isStarted = false;
		private static List<LogItem> logBuffer;
		private static bool _enableConsoleLogging;

		private static readonly object configLock = new object();
		private static readonly object logBufferLock = new object();
		private static readonly object logBufferInUseLock = new object();
		private static readonly object shouldExecuteLock = new object();

		public static void Init(bool enableConsoleLogging, LogLevel level, int maxFileSize)
		{
			config = new LogConfiguration("DataTrack");

			logLevel = level;
			maxLogLength = maxFileSize;

			_enableConsoleLogging = enableConsoleLogging;
			logBuffer = new List<LogItem>();
			logBufferInUse = true;
			shouldExecute = true;

			Clear();
			Create();

			loggingThread = new Thread(new ThreadStart(Logging));
			loggingThread.Start();

			isStarted = true;
		}

		private static void Create()
		{
			if (currentLength < maxLogLength && currentLength != 0)
			{
				return;
			}

			lock (configLock)
			{
				config.CreateLogFile();
			}

			currentLength = 0;
		}

		private static void Log(MethodBase? method, string message, LogLevel level)
		{
			if (level < logLevel)
			{
				return;
			}

			lock (logBuffer)
			{
				logBuffer.Add(new LogItem(method, message, level));
			}

			lock (logBufferInUseLock)
			{
				logBufferInUse = true;
			}
		}

		public static void Trace(MethodBase method, string message)
		{
			Log(method, message, LogLevel.Trace);
		}

		public static void Trace(string message)
		{
			Log(null, message, LogLevel.Trace);
		}

		public static void Debug(MethodBase method, string message)
		{
			Log(method, message, LogLevel.Debug);
		}

		public static void Debug(string message)
		{
			Log(null, message, LogLevel.Debug);
		}

		public static void Info(MethodBase method, string message)
		{
			Log(method, message, LogLevel.Info);
		}

		public static void Info(string message)
		{
			Log(null, message, LogLevel.Info);
		}

		public static void Warn(MethodBase method, string message)
		{
			Log(method, message, LogLevel.Warn);
		}

		public static void Warn(string message)
		{
			Log(null, message, LogLevel.Warn);
		}

		public static void Error(MethodBase method, string message)
		{
			Log(method, message, LogLevel.Error);
		}

		public static void Error(string message)
		{
			Log(null, message, LogLevel.Error);
		}

		public static void ErrorFatal(MethodBase method, string message)
		{
			Log(method, message, LogLevel.ErrorFatal);
		}

		public static void ErrorFatal(string message)
		{
			Log(null, message, LogLevel.ErrorFatal);
		}

		public static void Stop()
		{
			EndExecution();
		}

		private static void Clear()
		{
			lock (configLock)
			{
				config.DeleteLogFiles();
			}
		}

		private static void WriteWarningLine(string message)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
		}

		private static void WriteErrorLine(string message)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
		}

		private static void WriteInfoLine(string message)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
		}

		private static void Logging()
		{
			List<LogItem> threadLogBuffer = new List<LogItem>();

			while (ShouldExecute())
			{
				Thread.Sleep(100);

				threadLogBuffer = GetLogBufferForThread();

				foreach (LogItem log in threadLogBuffer)
				{
					Output(log);
				}

				lock (logBufferInUseLock)
				{
					logBufferInUse = threadLogBuffer.Count > 0;
				}
			}
		}

		private static List<LogItem> GetLogBufferForThread()
		{
			List<LogItem>? threadLogBuffer = null;

			lock (logBufferLock)
			{
				threadLogBuffer = new List<LogItem>(logBuffer.Count);
				threadLogBuffer.AddRange(logBuffer);
				logBuffer.Clear();
			}

			return threadLogBuffer ?? new List<LogItem>();
		}

		private static void Output(LogItem log)
		{
			string logOutput = log.ToString();

			lock (configLock)
			{
				using (StreamWriter writer = new StreamWriter(config.GetFullPath(), true))
				{
					writer.WriteLine(logOutput);
					currentLength++;
				}
			}

			if (_enableConsoleLogging)
			{
				switch (log.Level)
				{
					case LogLevel.Error: WriteErrorLine(logOutput); break;
					case LogLevel.Warn: WriteWarningLine(logOutput); break;
					case LogLevel.Info: WriteInfoLine(logOutput); break;
					default:
						break;
				}
			}

			Create();
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

		public static bool IsStarted()
		{
			return isStarted;
		}
	}
}