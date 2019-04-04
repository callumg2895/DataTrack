using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public abstract class QueryExecutor<TBase> where TBase : IEntity, new()
    {
        private protected Mapping<TBase> mapping;
        private protected Stopwatch stopwatch;
        private protected Type baseType;
        private protected SqlConnection _connection;
        private protected SqlTransaction _transaction;

        public QueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction)
        {
            stopwatch = new Stopwatch();
            mapping = query.Mapping;
            baseType = typeof(TBase);
            _connection = connection;

            if (transaction != null)
                _transaction = transaction;
        }
    }
}
