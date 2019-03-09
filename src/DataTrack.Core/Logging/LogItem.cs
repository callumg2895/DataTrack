using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Logging
{
    struct LogItem
    {
        public LogItem(MethodBase? method, string message, LogLevel level)
        {
            Method = method;
            Message = message;
            Level = level;
        }

        public MethodBase? Method;
        public string Message;
        public LogLevel Level;

        public override string ToString()
        {
            StringBuilder logOutputBuilder = new StringBuilder();

            logOutputBuilder.Append(DateTime.Now.ToLongTimeString());
            logOutputBuilder.Append(" | ");
            logOutputBuilder.Append(Level.ToString());
            logOutputBuilder.Append(" | ");

            if (Method != null)
            {
                logOutputBuilder.Append($"{Method.ReflectedType.Name}::{Method.Name}()");
                logOutputBuilder.Append(" | ");
            }

            if (Level == LogLevel.ErrorFatal)
                Message = $"FATAL {Message}";

            logOutputBuilder.Append(Message);

            return logOutputBuilder.ToString();
        }
    }
}
