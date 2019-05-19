using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataTrack.Logging
{
    public class LogConfiguration
    {
        private string fileName;
        private string filePath;
        private string fileExtension;
        private DateTime fileDate;
        private string fileDateString;

        public LogConfiguration(string projectName)
        {
            fileName = $"{projectName}Log_";
            filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}";
            fileExtension = ".txt";
            fileDate = DateTime.Now.Date;
            fileDateString = fileDate.ToShortDateString().Replace("/", "_");
        }

        public void CreateLogDirectory()
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        }

        public void DeleteAllLogs()
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

        public void UpdateFileDate()
        {
            fileDate = DateTime.Now.Date;
        }

        public string GetFullPath(int index)
        {
            return $@"{filePath}\{fileDateString}_{fileName}{index}{fileExtension}";
        }

        public DateTime GetFileDate()
        {
            return fileDate;
        }
    }
}
