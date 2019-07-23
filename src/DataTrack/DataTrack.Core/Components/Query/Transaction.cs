using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.Components.Query
{
	public class Transaction : IDisposable
	{
		#region Members

		private readonly SqlTransaction transaction;
		private readonly SqlConnection connection;
		private readonly Stopwatch stopwatch;

		#endregion

		#region Constructors

		public Transaction()
		{
			connection = DataTrackConfiguration.CreateConnection();
			transaction = connection.BeginTransaction();
			stopwatch = new Stopwatch();
		}

		#endregion

		#region Methods

		public dynamic Execute(IQuery query)
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
