using DataTrack.Core.Components.Cache;
using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Configuration;
using DataTrack.Core.Enums;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace DataTrack.Core
{

	public sealed class DataTrackConfiguration : IHostedService
	{

		#region Members

		public static string ConnectionString = string.Empty;
		internal static Logger Logger;

		private static DataTrackConfiguration instance;
		private static DatabaseConfiguration databaseConfig;
		private static LogConfiguration loggingConfig;
		private static CacheConfiguration cacheConfig;
		
		public IServiceProvider ServiceProvider { get; }

		public static DataTrackConfiguration Initialize(IServiceProvider serviceProvider)
		{
			if (instance == null)
			{
				instance = new DataTrackConfiguration(serviceProvider);

				return Instance;
			}

			throw new InvalidOperationException("Cannot initialize DataTrackConfiguration instance when already initialized.");
		}

		// Thread safe lazy singleton
		public static DataTrackConfiguration Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				throw new NullReferenceException("DataTrackConfiguration has not yet been initialized.");
			}
		}

		#endregion

		#region Constructors

		private DataTrackConfiguration(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		#endregion

		#region Methods
		private void Init()
		{
			LoadConfiguration();

			MappingCache.Init(cacheConfig.CacheSizeLimit, loggingConfig);
			ChildPropertyCache.Init(cacheConfig.CacheSizeLimit, loggingConfig);
			NativePropertyCache.Init(cacheConfig.CacheSizeLimit, loggingConfig);
			CompiledActivatorCache.Init(cacheConfig.CacheSizeLimit, loggingConfig);

			Logger = new Logger(loggingConfig);
			ConnectionString = databaseConfig.GetConnectionString();
		}

		public SqlConnection CreateConnection()
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

		private void Stop()
		{
			MappingCache.Instance.Stop();
			ChildPropertyCache.Instance.Stop();
			NativePropertyCache.Instance.Stop();
			CompiledActivatorCache.Instance.Stop();

			Logger.Stop();
		}		

		private void LoadConfiguration()
		{
			DataTrackConfigReader reader = new DataTrackConfigReader();

			databaseConfig = new DatabaseConfiguration(reader.GetDatabaseConfigNode());
			loggingConfig = new LogConfiguration(reader.GetLoggingConfigNode());
			cacheConfig = new CacheConfiguration(reader.GetCacheConfigNode());
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			Task task = new Task(Instance.Init, cancellationToken);

			task.RunSynchronously();

			return task;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			Task task = new Task(Instance.Stop, cancellationToken);

			task.RunSynchronously();

			return task;
		}

		#endregion
	}
}
