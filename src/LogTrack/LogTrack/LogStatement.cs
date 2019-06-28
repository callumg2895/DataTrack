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

		private static Regex Trace = new Regex("TRC");
		private static Regex Debug = new Regex("DBG");
		private static Regex Info = new Regex("INF");
		private static Regex Warning = new Regex("WRN");
		private static Regex Error = new Regex("ERR");
		private static Regex Fatal = new Regex("ERF");

		public LogStatement(string statement)
		{
			text = new StringBuilder();
			LogLevel = GetLogLevel(statement);

			text.Append(statement);
		}

		public void Write()
		{
			switch (LogLevel)
			{
				case LogLevel.Trace:
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case LogLevel.Debug:
					Console.ForegroundColor = ConsoleColor.Blue;
					break;
				case LogLevel.Info:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case LogLevel.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LogLevel.Fatal:
					Console.BackgroundColor = ConsoleColor.Yellow;
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LogLevel.Unknown:
					Console.ForegroundColor = ConsoleColor.Magenta;
					break;
			}

			Console.WriteLine(text.ToString());
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
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
