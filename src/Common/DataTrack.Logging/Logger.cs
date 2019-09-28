using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Logging
{
	public class Logger
	{
		public LogConfiguration Config { get; set; }

		public Logger(LogConfiguration config)
		{
			Config = config;

			LogWriter.Init(Config);
		}

		public void Trace(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.Trace);
		}

		public void Trace(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.Trace);
		}

		public void Debug(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.Debug);
		}

		public void Debug(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.Debug);
		}

		public void Info(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.Info);
		}

		public void Info(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.Info);
		}

		public void Warn(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.Warn);
		}

		public void Warn(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.Warn);
		}

		public void Error(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.Error);
		}

		public void Error(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.Error);
		}

		public void ErrorFatal(MethodBase method, string message)
		{
			LogWriter.Log(Config, method, message, LogLevel.ErrorFatal);
		}

		public void ErrorFatal(string message)
		{
			LogWriter.Log(Config, null, message, LogLevel.ErrorFatal);
		}

		public void Stop()
		{
			LogWriter.Stop();
		}

	}
}
