using System;
using System.Reflection;
using System.Text;

namespace DataTrack.Logging
{
	internal struct LogItem
	{
		public LogItem(MethodBase? method, string message, LogLevel level)
		{
			Method = method;
			Message = message;
			DateTime = DateTime.Now;
			Level = level;
		}

		public MethodBase? Method;
		public string Message;
		public DateTime DateTime;
		public LogLevel Level;

		public override string ToString()
		{
			StringBuilder logOutputBuilder = new StringBuilder();

			logOutputBuilder.Append($"[{DateTime.ToString("HH:mm:ss.fff")}]");
			logOutputBuilder.Append(" | ");
			logOutputBuilder.Append(GetLogLevelString());
			logOutputBuilder.Append(" | ");

			if (Method != null)
			{
				logOutputBuilder.Append($"{Method.ReflectedType.Name}::{Method.Name}()");
				logOutputBuilder.Append(" | ");
			}

			if (Level == LogLevel.ErrorFatal)
			{
				Message = $"FATAL {Message}";
			}

			logOutputBuilder.Append(Message);

			return logOutputBuilder.ToString();
		}

		private string GetLogLevelString()
		{
			switch (Level)
			{
				case LogLevel.Trace:		return "TRACE";
				case LogLevel.Debug:		return "DEBUG";
				case LogLevel.Info:			return "INFO";
				case LogLevel.Warn:			return "WARN";
				case LogLevel.Error:		return "ERROR";
				case LogLevel.ErrorFatal:	return "CRITICAL";
				default:
					return string.Empty;
			}
		}
	}
}
