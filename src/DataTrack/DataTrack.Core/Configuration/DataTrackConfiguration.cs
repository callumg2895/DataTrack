using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Configuration;
using DataTrack.Core.Enums;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml;

namespace DataTrack.Core
{

	public static class DataTrackConfiguration
	{

		#region Members

		public static string ConnectionString = string.Empty;

		private static DatabaseConfiguration databaseConfig;
		private static LoggingConfiguration loggingConfig;
		private static CacheConfiguration cacheConfig;

		private static string configFilePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}DataTrack/config";

		#endregion

		#region Methods
		public static void Init()
		{
			LoadConfiguration();

			MappingCache.Init(cacheConfig.CacheSizeLimit);
			Logger.Init(false, loggingConfig.LogLevel, loggingConfig.MaxFileSize);

			ConnectionString = databaseConfig.GetConnectionString();
		}

		public static SqlConnection CreateConnection()
		{

			SqlConnection connection = new SqlConnection();

			if (!string.IsNullOrEmpty(ConnectionString))
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();
				Logger.Info(MethodBase.GetCurrentMethod(), "Successfully opened new SQL connection");
			}
			else
			{
				Logger.Warn(MethodBase.GetCurrentMethod(), "Failed to open new SQL connection - connection string not supplied");
			}

			return connection;
		}

		public static void Dispose()
		{
			MappingCache.Stop();
			Logger.Stop();
		}		

		private static void LoadConfiguration()
		{
			XmlDocument doc = new XmlDocument();

			string rootNode = "DataTrackConfig";

			List<string> nodes = new List<string>(3)
			{
				"Database",
				"Logging",
				"Cache"
			};

			databaseConfig = new DatabaseConfiguration();
			loggingConfig = new LoggingConfiguration();
			cacheConfig = new CacheConfiguration();

			doc.Load($"{configFilePath}/{rootNode}.xml");

			foreach (string node in nodes)
			{
				switch (node)
				{
					case "Database":
						XmlNode xmlNode = doc.SelectSingleNode($"{rootNode}/{node}/Connection");

						databaseConfig.DataSource = xmlNode.Attributes.GetNamedItem("source").Value;
						databaseConfig.InitalCatalog = xmlNode.Attributes.GetNamedItem("catalog").Value;
						databaseConfig.UserID = xmlNode.Attributes.GetNamedItem("id").Value;
						databaseConfig.Password = xmlNode.Attributes.GetNamedItem("password").Value;

						break;

					case "Logging":
						XmlNode xmlNodeLogLevel = doc.SelectSingleNode($"{rootNode}/{node}/LogLevel");
						XmlNode xmlNodeMaxFileLength = doc.SelectSingleNode($"{rootNode}/{node}/MaxFileLength");

						loggingConfig.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), xmlNodeLogLevel.InnerText);
						loggingConfig.MaxFileSize = int.Parse(xmlNodeMaxFileLength.InnerText);

						break;

					case "Cache":
						XmlNode xmlNodeMaxCacheSize = doc.SelectSingleNode($"{rootNode}/{node}/MaxCacheSize");

						cacheConfig.CacheSizeLimit = int.Parse(xmlNodeMaxCacheSize.InnerText);

						break;

					default:
						return;
				}
			}
		}

		#endregion
	}
}
