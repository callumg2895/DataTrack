using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LogTrack
{
	public enum LogLevel
	{
		Trace,
		Debug,
		Info,
		Warning,
		Error,
		Fatal,
		Unknown
	}

	public class LogStatement
	{
		private StringBuilder text;
		public LogLevel LogLevel;

		private static Regex Trace = new Regex("TRACE");
		private static Regex Debug = new Regex("DEBUG");
		private static Regex Info = new Regex("INFO");
		private static Regex Warning = new Regex("WARN");
		private static Regex Error = new Regex("ERROR");
		private static Regex Fatal = new Regex("CRITICAL");

		Dictionary<LogLevel, TextFormat> LogTextFormat = new Dictionary<LogLevel, TextFormat>()
		{
			{ LogLevel.Trace, new TextFormat(ConsoleColor.Cyan) },
			{ LogLevel.Debug, new TextFormat(ConsoleColor.Blue) },
			{ LogLevel.Info, new TextFormat(ConsoleColor.Green) },
			{ LogLevel.Warning, new TextFormat(ConsoleColor.Yellow) },
			{ LogLevel.Error, new TextFormat(ConsoleColor.Red) },
			{ LogLevel.Fatal, new TextFormat(ConsoleColor.Red, ConsoleColor.Yellow) },
			{ LogLevel.Unknown, new TextFormat(ConsoleColor.Magenta) },
		};

		public LogStatement(string statement)
		{
			text = new StringBuilder();
			LogLevel = GetLogLevel(statement);

			text.Append(statement);
		}

		public void Write()
		{
			TextFormat format = LogTextFormat[LogLevel];

			format.Apply();

			Console.WriteLine(text.ToString());

			format.Reset();

		}

		public void Append(LogStatement statement)
		{
			text.AppendLine(statement.ToString());
		}

		public override string ToString()
		{
			return text.ToString();
		}

		private LogLevel GetLogLevel(string statement)
		{
			if (Trace.IsMatch(statement))
			{
				return LogLevel.Trace;
			}

			if (Debug.IsMatch(statement))
			{
				return LogLevel.Debug;
			}

			if (Info.IsMatch(statement))
			{
				return LogLevel.Info;
			}

			if (Warning.IsMatch(statement))
			{
				return LogLevel.Warning;
			}

			if (Error.IsMatch(statement))
			{
				return LogLevel.Error;
			}

			if (Fatal.IsMatch(statement))
			{
				return LogLevel.Fatal;
			}

			return LogLevel.Unknown;
		}
	}
}
