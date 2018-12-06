using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DataTrack.Core.Util
{
    public static class Logger
    {

        private const string filePath = @".\";
        private const string fileName = @"DataTrackLog_";
        private const string fileExtension = @".txt";
        private const int maxLogLength = 10000;

        private static int fileIndex = 0;
        private static string fullPath;
        private static int currentLength = 0;

        private static Thread loggingThread;
        private volatile static bool running;
        private static List<(MethodBase method, string message, OutputTypes type)> logBuffer;
        private static OutputTypes outputType;

        public static void Init(OutputTypes consoleOutputType = OutputTypes.None)
        {
            fullPath = $"{filePath}{fileName}{fileIndex}{fileExtension}";
            outputType = consoleOutputType;
            logBuffer = new List<(MethodBase method, string message, OutputTypes type)>();
            running = true;

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
                    fullPath = $"{filePath}{fileName}{++fileIndex}{fileExtension}";

                using (StreamWriter writer = File.CreateText(fullPath)) { };
            }
        }

        private static void Log(MethodBase method, string message, OutputTypes type)
        {
            lock (logBuffer)
                logBuffer.Add((method, message, type));
        }

        public static void Info(MethodBase method, string message) => Log(method, $"Info: {message}", OutputTypes.Info);

        public static void Warn(MethodBase method, string message) => Log(method, $"Warning: {message}", OutputTypes.Warning);

        public static void Error(MethodBase method, string message) => Log(method, $"Error: {message}", OutputTypes.Error);

        public static void Stop() => running = false;

        private static void Clear()
        {
            lock (fullPath)
            {
                string[] files = Directory.GetFiles(filePath, $"{fileName}*");
                foreach (string file in files)
                    File.Delete(file);
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
            List<(MethodBase method, string message, OutputTypes type)> threadLogBuffer = new List<(MethodBase method, string message, OutputTypes type)>();

            while (running)
            {
                lock (logBuffer)
                {
                    threadLogBuffer.AddRange(logBuffer);
                    logBuffer.Clear();
                }

                foreach ((MethodBase method, string message, OutputTypes type) message in threadLogBuffer)
                {
                    string logOutput = $"{DateTime.Now} | {message.method.ReflectedType.Name}::{message.method.Name}() | {message.message}";

                    lock (fullPath)
                    {
                        using (StreamWriter writer = new StreamWriter(fullPath, true))
                        {
                            writer.WriteLine(logOutput);
                            currentLength++;
                        }
                    }

                    if (outputType == OutputTypes.All || (int)message.type <= (int)outputType)
                        switch (message.type)
                        {
                            case OutputTypes.Error: WriteErrorLine(logOutput); break;
                            case OutputTypes.Warning: WriteWarningLine(logOutput); break;
                            case OutputTypes.Info: WriteInfoLine(logOutput); break;
                            case OutputTypes.None:
                            default:
                                break;
                        }

                    if (currentLength == maxLogLength)
                        Create();
                }

                threadLogBuffer.Clear();
            }
        }
    }
}