using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace DataTrack.Util.Helpers
{
	public class DataTrackConfigReader
	{
		private XmlDocument configFile { get; set; }

		private const string configFileName = "DataTrackConfig";
		private const string configFileExtension = ".xml";

		private const string databaseConfigNode = "Database";
		private const string loggingConfigNode = "Logging";
		private const string cacheConfigNode = "Cache";

		private static string configFilePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}DataTrack/config";

		public DataTrackConfigReader()
		{
			configFile = new XmlDocument();

			configFile.Load($"{configFilePath}/{configFileName}{configFileExtension}");
		}

		public XmlNode GetDatabaseConfigNode()
		{
			return GetRoot().SelectSingleNode(databaseConfigNode);
		}

		public XmlNode GetLoggingConfigNode()
		{
			return GetRoot().SelectSingleNode(loggingConfigNode);
		}
		
		public XmlNode GetCacheConfigNode()
		{
			return GetRoot().SelectSingleNode(cacheConfigNode);
		}

		private XmlNode GetRoot()
		{
			return configFile.SelectSingleNode(configFileName);
		}
	}
}
