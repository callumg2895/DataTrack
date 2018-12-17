using DataTrack.Core.Enums;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.Util
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
        private volatile static bool running;
        private static List<(MethodBase Method, string Message, LogLevel Type)> logBuffer;
        private static bool _enableConsoleLogging;

        public static void Init(bool enableConsoleLogging)
        {
            fullPath = $@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}";
            _enableConsoleLogging = enableConsoleLogging;
            logBuffer = new List<(MethodBase method, string message, LogLevel type)>();
            running = true;

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

        private static void Log(MethodBase method, string message, LogLevel type)
        {
            lock (logBuffer)
                logBuffer.Add((method, message, type));
        }

        public static void Info(MethodBase method, string message) => Log(method, message, LogLevel.Info);

        public static void Warn(MethodBase method, string message) => Log(method, message, LogLevel.Warn);

        public static void Error(MethodBase method, string message) => Log(method, message, LogLevel.Error);

        public static void Stop() => running = false;

        private static void Clear()
        {
            lock (fullPath)
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
            List<(MethodBase Method, string Message, LogLevel Type)> threadLogBuffer = new List<(MethodBase method, string message, LogLevel type)>();

            while (running)
            {
                lock (logBuffer)
                {
                    threadLogBuffer.AddRange(logBuffer);
                    logBuffer.Clear();
                }

                foreach ((MethodBase Method, string Message, LogLevel Level) log in threadLogBuffer)
                {
                    StringBuilder logOutputBuilder = new StringBuilder();

                    logOutputBuilder.Append(DateTime.Now.ToLongTimeString());
                    logOutputBuilder.Append(" | ");
                    logOutputBuilder.Append(log.Level.ToString());
                    logOutputBuilder.Append(" | ");
                    logOutputBuilder.Append($"{log.Method.ReflectedType.Name}::{log.Method.Name}()");
                    logOutputBuilder.Append(" | ");
                    logOutputBuilder.Append(log.Message);

                    string logOutput = logOutputBuilder.ToString();

                    lock (fullPath)
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

                threadLogBuffer.Clear();
            }
        }
    }
}