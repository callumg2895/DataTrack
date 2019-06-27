using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Util.Extensions;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.SQL
{
	public class Transaction<TBase> : IDisposable where TBase : IEntity, new()
	{
		#region Members

		private readonly SqlTransaction transaction;
		private readonly SqlConnection connection;
		private readonly Stopwatch stopwatch;
		private readonly List<object> results;

		#endregion

		#region Constructors

		public Transaction()
		{
			connection = DataTrackConfiguration.CreateConnection();
			transaction = connection.BeginTransaction();
			stopwatch = new Stopwatch();
			results = new List<object>();
		}

		#endregion

		#region Methods

		public dynamic Execute(Query<TBase> query)
		{
			return query.Execute(connection.CreateCommand(), connection, transaction);
		}

		public void RollBack()
		{
			stopwatch.Start();
			transaction.Rollback();
			stopwatch.Stop();

			Logger.Info(MethodBase.GetCurrentMethod(), $"Rolled back Transaction ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");
		}

		public void Commit()
		{
			stopwatch.Start();
			transaction.Commit();
			stopwatch.Stop();

			Logger.Info(MethodBase.GetCurrentMethod(), $"Committed Transaction ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");
		}

		public void Dispose()
		{
			transaction.Dispose();
		}

		#endregion
	}
}
