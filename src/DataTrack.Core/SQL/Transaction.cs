using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DataTrack.Core.SQL
{
    public class Transaction<TBase> : IDisposable where TBase : new()
    {
        #region Members

        private SqlTransaction transaction;
        private SqlConnection connection;
        private List<Query<TBase>> queries;
        private Stopwatch stopwatch;
        private List<Object> results;

        #endregion

        #region Constructors

        private Transaction()
        {
            connection = DataTrackConfiguration.CreateConnection();
            transaction = connection.BeginTransaction();
            stopwatch = new Stopwatch();
            results = new List<object>();
        }

        public Transaction(Query<TBase> query)
            : this()
        {
            this.queries = new List<Query<TBase>>() { query };
        }

        public Transaction(List<Query<TBase>> queries)
            : this()
        {
            this.queries = queries;
        }

        #endregion

        #region Methods

        public List<object> Execute()
        {
            stopwatch.Start();
            queries.ForEach(query => results.Add(query.Execute(connection.CreateCommand(), transaction)));
            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Transaction ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {queries.Count} {(queries.Count > 1 ? "queries" : "query")} executed");

            return results;
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
