using System;
using System.IO;
using System.Xml;

namespace DataTrack.Logging
{
	public class LogConfiguration
	{
		public int MaxFileSize { get; private set; }

		public readonly string ProjectName;

		internal LogLevel LogLevel { get; set; }

		private string fileName;
		private string filePath;
		private string fileExtension;
		private DateTime fileDate;
		private string fileDateString;
		private int fileIndex;

		public LogConfiguration(string projectName, LogLevel logLevel, int maxFileSize = 10000)
		{
			ProjectName = projectName;
			LogLevel = logLevel;
			MaxFileSize = maxFileSize;

			Initialize();
		}

		public LogConfiguration(XmlNode loggingNode)
		{
			XmlNode xmlNodeProjectName = loggingNode.SelectSingleNode("ProjectName");
			XmlNode xmlNodeLogLevel = loggingNode.SelectSingleNode("LogLevel");
			XmlNode xmlNodeMaxFileLength = loggingNode.SelectSingleNode("MaxFileLength");

			ProjectName = xmlNodeProjectName.InnerText;
			LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), xmlNodeLogLevel.InnerText);
			MaxFileSize = int.Parse(xmlNodeMaxFileLength.InnerText);

			Initialize();
		}

		private void Initialize()
		{
			fileDate = DateTime.Now.Date;
			fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			fileName = $"{fileDateString}_{ProjectName}Log_";
			filePath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}{ProjectName}/logs";
			fileExtension = ".txt";
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
				fileDateString = fileDate.ToShortDateString().Replace("/", "_");
				fileName = $"{fileDateString}_{ProjectName}Log_";
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
				string[] files = Directory.GetFiles(filePath, $"{fileName}*");

				foreach (string file in files)
				{
					File.Delete(file);
				}
			}
		}

		internal string GetFullPath()
		{
			return $@"{filePath}\{fileName}{fileIndex}{fileExtension}";
		}
	}
}
