using DataTrack.Core.Enums;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

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

        private static OutputTypes outputType;

        public static void Init(OutputTypes consoleOutputType = OutputTypes.None)
        {
            fullPath = $"{filePath}{fileName}{fileIndex}{fileExtension}";
            outputType = consoleOutputType;
            Clear();
            Create();
        }

        private static void Create()
        {
            currentLength = 0;

            while (File.Exists(fullPath))
                fullPath = $"{filePath}{fileName}{++fileIndex}{fileExtension}";

            using (StreamWriter writer = File.CreateText(fullPath)) { };

            Info(MethodBase.GetCurrentMethod(), $"Created file {fileName}{fileIndex}{fileExtension}");
        }

        private static void Log(MethodBase method, string message, OutputTypes type)
        {
            string logOutput = $"{DateTime.Now} | {method.ReflectedType.Name}::{method.Name}() | {message}";

            using (StreamWriter writer = new StreamWriter(fullPath, true))
            {
                writer.WriteLine(logOutput);
                currentLength++;
            }

            if(outputType == OutputTypes.All || (int)type <= (int)outputType)
                switch (type)
                {
                    case OutputTypes.Error:      WriteErrorLine(logOutput); break;
                    case OutputTypes.Warning:    WriteWarningLine(logOutput); break;
                    case OutputTypes.Info:       WriteInfoLine(logOutput); break;
                    case OutputTypes.None:
                    default:
                        break;
                }

            if (currentLength == maxLogLength)
                Create();
        }

        public static void Info(MethodBase method, string message) => Log(method, $"Info: {message}", OutputTypes.Info);

        public static void Warn(MethodBase method, string message) => Log(method, $"Warning: {message}", OutputTypes.Warning);

        public static void Error(MethodBase method, string message) => Log(method, $"Error: {message}", OutputTypes.Error);

        private static void Clear()
        {
            string[] files = Directory.GetFiles(filePath, $"{fileName}*");
            foreach (string file in files)
                File.Delete(file);
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

    }
}