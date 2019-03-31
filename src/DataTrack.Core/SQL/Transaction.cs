using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.SQL
{
    public class Transaction<TBase> : IDisposable where TBase : Entity, new()
    {
        #region Members

        private SqlTransaction transaction;
        private SqlConnection connection;
        private Stopwatch stopwatch;
        private List<Object> results;

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
