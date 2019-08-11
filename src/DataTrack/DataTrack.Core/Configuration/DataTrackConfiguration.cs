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
		private static LogConfiguration loggingConfig;
		private static CacheConfiguration cacheConfig;

		private const string configFileName = "DataTrackConfig";
		private const string configFileExtension = ".xml";

		private const string databaseConfigNode = "Database";
		private const string loggingConfigNode = "Logging";
		private const string cacheConfigNode = "Cache";

		private static string configFilePath = $"{Path.GetPathRoot(Environment.SystemDirectory)}DataTrack/config";

		#endregion

		#region Methods
		public static void Init()
		{
			LoadConfiguration();

			MappingCache.Init(cacheConfig.CacheSizeLimit);
			Logger.Init(loggingConfig);

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
				Logger.Warn(MethodBase.GetCurrentMethod(), "Failed to open new SQL connection - configuration not initialised");
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

			doc.Load($"{configFilePath}/{configFileName}{configFileExtension}");

			XmlNode rootNode = doc.SelectSingleNode(configFileName);

			foreach (XmlNode node in rootNode.ChildNodes)
			{
				switch (node.Name)
				{
					case databaseConfigNode:
						databaseConfig = new DatabaseConfiguration(node);
						break;

					case loggingConfigNode:
						loggingConfig = new LogConfiguration(node);
						break;

					case cacheConfigNode:
						cacheConfig = new CacheConfiguration(node);
						break;

					default:
						return;
				}
			}
		}

		#endregion
	}
}
