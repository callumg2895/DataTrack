using DataTrack.Core.SQL.QueryObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataTrack.Core.SQL
{
    public class Transaction<TBase> : IDisposable where TBase : new()
    {
        #region Members

        private SqlTransaction transaction;
        private SqlConnection connection;
        private List<Query<TBase>> queries;

        #endregion

        #region Constructors

        private Transaction()
        {
            connection = DataTrackConfiguration.CreateConnection();
            transaction = connection.BeginTransaction();
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
            List<object> results = new List<object>();

            queries.ForEach(query => results.Add(query.Execute(connection.CreateCommand(), transaction)));

            return results;
        }

        public void RollBack()
        {
            transaction.Rollback();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Dispose()
        {
            transaction.Dispose();
        }

        #endregion
    }
}
