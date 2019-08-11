using System;
using System.IO;

namespace DataTrack.Logging
{
	public class LogConfiguration
	{
		private readonly string fileName;
		private readonly string filePath;
		private readonly string fileExtension;
		private DateTime fileDate;
		private readonly string fileDateString;
		private int fileIndex;

		public LogConfiguration(string projectName)
		{
			fileName = $"{projectName}Log_";
			filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}/logs";
			fileExtension = ".txt";
			fileDate = DateTime.Now.Date;
			fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			fileIndex = 0;
		}

		public void CreateLogFile()
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

		public void DeleteLogFiles()
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

		public string GetFullPath()
		{
			return $@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}";
		}
	}
}
