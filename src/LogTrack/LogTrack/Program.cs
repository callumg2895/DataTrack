using System;
using System.IO;

namespace LogTrack
{
	class Program
	{
		static void Main(string[] args)
		{
			string projectName = "DataTrack";
			string fileName = $"{projectName}Log_";
			string filePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}{projectName}";
			string fileExtension = ".txt";
			DateTime fileDate = DateTime.Now.Date;
			string fileDateString = fileDate.ToShortDateString().Replace("/", "_");
			int fileIndex = 0;

			if (Directory.Exists(filePath))
			{
				using (StreamReader reader = File.OpenText($@"{filePath}\{fileDateString}_{fileName}{fileIndex}{fileExtension}"))
				{
					while (true)
					{
						Console.WriteLine(reader.ReadLine());

						if (reader.EndOfStream)
						{
							break;
						}

					}
				}
			}
		}
	}
}
