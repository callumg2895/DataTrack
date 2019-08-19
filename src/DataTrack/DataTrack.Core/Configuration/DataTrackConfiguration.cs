using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Configuration;
using DataTrack.Core.Enums;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
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

		#endregion

		#region Methods
		public static void Init()
		{
			LoadConfiguration();

			MappingCache.Init(cacheConfig.CacheSizeLimit);
			ChildPropertyCache.Init(cacheConfig.CacheSizeLimit);
			NativePropertyCache.Init(cacheConfig.CacheSizeLimit);
			CompiledActivatorCache.Init(cacheConfig.CacheSizeLimit);
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
			ChildPropertyCache.Stop();
			NativePropertyCache.Stop();
			CompiledActivatorCache.Stop();
			Logger.Stop();
		}		

		private static void LoadConfiguration()
		{
			DataTrackConfigReader reader = new DataTrackConfigReader();

			databaseConfig = new DatabaseConfiguration(reader.GetDatabaseConfigNode());
			loggingConfig = new LogConfiguration(reader.GetLoggingConfigNode());
			cacheConfig = new CacheConfiguration(reader.GetCacheConfigNode());
		}

		#endregion
	}
}
