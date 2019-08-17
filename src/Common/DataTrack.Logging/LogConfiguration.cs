using System;
using System.IO;
using System.Xml;

namespace DataTrack.Logging
{
	public class LogConfiguration
	{
		public int MaxFileSize { get; set; }
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public string FileExtension { get; set; }

		internal LogLevel LogLevel { get; set; }
		internal bool EnableConsoleLogging { get; set; }

		private readonly string projectName;
		private DateTime fileDate;
		private string fileDateString;
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

			fileDate = DateTime.Now.Date;
			fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			FileName = $"{fileDateString}_{projectName}Log_";
			FilePath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}/logs";
			FileExtension = ".txt";
			fileIndex = 0;
		}

		internal void CreateLogFile()
		{
			if (!Directory.Exists(FilePath))
			{
				Directory.CreateDirectory(FilePath);
			}

			if (DateTime.Now.Date > fileDate)
			{
				fileDate = DateTime.Now.Date;
				fileDateString = fileDate.ToShortDateString().Replace("/", "_");
				FileName = $"{fileDateString}_{projectName}Log_";
			}

			while (File.Exists(GetFullPath()))
			{
				fileIndex++;
			}

			using (StreamWriter writer = File.CreateText(GetFullPath())) { };
		}

		internal void DeleteLogFiles()
		{
			if (Directory.Exists(FilePath))
			{
				string[] files = Directory.GetFiles(FilePath, $"{FileName}*");

				foreach (string file in files)
				{
					File.Delete(file);
				}
			}
		}

		internal string GetFullPath()
		{
			return $@"{FilePath}\{FileName}{fileIndex}{FileExtension}";
		}
	}
}
