using DataTrack.Core.Enums;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.Logging
{
    public static class Logger
    {
        private const string fileName = @"DataTrackLog_";
        private const string fileExtension = @".txt";
        private const int maxLogLength = 10000;

        private static int fileIndex = 0;
        private static string fullPath;
        private static DateTime fileDate = DateTime.Now.Date;
        private static string fileDateString = fileDate.ToShortDateString().Replace("/", "_");
        private static string filePath = Path.GetPathRoot(Environment.SystemDirectory) + "DataTrack";
        private static int currentLength = 0;

        private static Thread loggingThread;
        private volatile static bool shouldExecute;
        private static List<LogItem> logBuffer;
        private static bool _enableConsoleLogging;

        private static object fullPathLock = new object();
        private static object logBufferLock = new object();
        private static object shouldExecuteLock = new object();

        public static void Init(bool enableConsoleLogging)
        {
            fullPath = $@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}";
            _enableConsoleLogging = enableConsoleLogging;
            logBuffer = new List<LogItem>();
            shouldExecute = true;

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            Clear();
            Create();

            loggingThread = new Thread(new ThreadStart(Logging));
            loggingThread.Start();
        }

        private static void Create()
        {
            lock (fullPath)
            {
                currentLength = 0;

                while (File.Exists(fullPath))
                    fullPath = $@"{filePath}\{fileDateString}_{fileName}{++fileIndex}{fileExtension}";

                using (StreamWriter writer = File.CreateText(fullPath)) { };
            }
        }

        private static void Log(MethodBase? method, string message, LogLevel level)
        {
            lock (logBuffer)
                logBuffer.Add(new LogItem(method, message, level));
        }

        public static void Info(MethodBase method, string message) => Log(method, message, LogLevel.Info);
        public static void Info(string message) => Log(null, message, LogLevel.Info);

        public static void Debug(MethodBase method, string message) => Log(method, message, LogLevel.Debug);
        public static void Debug(string message) => Log(null, message, LogLevel.Debug);

        public static void Warn(MethodBase method, string message) => Log(method, message, LogLevel.Warn);
        public static void Warn(string message) => Log(null, message, LogLevel.Warn);

        public static void Error(MethodBase method, string message) => Log(method, message, LogLevel.Error);
        public static void Error(string message) => Log(null, message, LogLevel.Error);

        public static void ErrorFatal(MethodBase method, string message) => Log(method, message, LogLevel.ErrorFatal);
        public static void ErrorFatal(string message) => Log(null, message, LogLevel.ErrorFatal);

        public static void Stop() => EndExecution();

        private static void Clear()
        {
            lock (fullPathLock)
            {
                string[] files = Directory.GetFiles(filePath, $"{fileDateString}_{fileName}*");
                files.ForEach(file => File.Delete(file));
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
                threadLogBuffer = GetLogBufferForThread();

                foreach (LogItem log in threadLogBuffer)
                {
                    Output(log);
                }

                threadLogBuffer.Clear();
            }
        }

        private static List<LogItem> GetLogBufferForThread()
        {
            List<LogItem> threadLogBuffer = new List<LogItem>();

            lock (logBufferLock)
            {
                threadLogBuffer.AddRange(logBuffer);
                logBuffer.Clear();
            }

            return threadLogBuffer;
        }

        private static void Output(LogItem log)
        {
            string logOutput = log.ToString();

            lock (fullPathLock)
            {
                using (StreamWriter writer = new StreamWriter(fullPath, true))
                {
                    writer.WriteLine(logOutput);
                    currentLength++;
                }
            }

            if (_enableConsoleLogging)
                switch (log.Level)
                {
                    case LogLevel.Error: WriteErrorLine(logOutput); break;
                    case LogLevel.Warn: WriteWarningLine(logOutput); break;
                    case LogLevel.Info: WriteInfoLine(logOutput); break;
                    default:
                        break;
                }

            if (currentLength == maxLogLength || DateTime.Now.Date > fileDate)
            {
                fileDate = DateTime.Now.Date;
                Create();
            }
        }

        private static bool ShouldExecute()
        {
            lock (shouldExecuteLock)
            {
                return shouldExecute;
            }
        }

        private static void EndExecution()
        {
            lock (shouldExecuteLock)
            {
                shouldExecute = false;
            }
        }
    }
}