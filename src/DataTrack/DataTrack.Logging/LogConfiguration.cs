using System;
using System.IO;
using System.Xml;

namespace DataTrack.Logging
{
	public class LogConfiguration
	{
		internal LogLevel LogLevel { get; set; }
		internal int MaxFileSize { get; set; }
		internal bool EnableConsoleLogging { get; set; }

		private readonly string projectName;
		private readonly string fileName;
		private readonly string filePath;
		private readonly string fileExtension;
		private DateTime fileDate;
		private readonly string fileDateString;
		private int fileIndex;

		public LogConfiguration(XmlNode loggingNode)
		{
			XmlNode xmlNodeProjectName = loggingNode.SelectSingleNode("ProjectName");
			XmlNode xmlNodeLogLevel = loggingNode.SelectSingleNode("LogLevel");
			XmlNode xmlNodeMaxFileLength = loggingNode.SelectSingleNode("MaxFileLength");

			projectName = xmlNodeProjectName.InnerText;
			LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), xmlNodeLogLevel.InnerText);
			MaxFileSize = int.Parse(xmlNodeMaxFileLength.InnerText);
			EnableConsoleLogging = false;

			fileName = $"{projectName}Log_";
			filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}/logs";
			fileExtension = ".txt";
			fileDate = DateTime.Now.Date;
			fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			fileIndex = 0;
		}

		internal void CreateLogFile()
		{
			if (!Directory.Exists(filePath))
			{
				Directory.CreateDirectory(filePath);
			}

			if (DateTime.Now.Date > fileDate)
			{
				fileDate = DateTime.Now.Date;
			}

			while (File.Exists(GetFullPath()))
			{
				fileIndex++;
			}

			using (StreamWriter writer = File.CreateText(GetFullPath())) { };
		}

		internal void DeleteLogFiles()
		{
			if (Directory.Exists(filePath))
			{
				string[] files = Directory.GetFiles(filePath, $"{fileDateString}_{fileName}*");

				foreach (string file in files)
				{
					File.Delete(file);
				}
			}
		}

		internal string GetFullPath()
		{
			return $@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}";
		}
	}
}
