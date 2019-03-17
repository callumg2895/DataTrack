using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public abstract class QueryExecutor<TBase> where TBase : Entity, new()
    {
        private protected Query<TBase> Query;
        private protected Stopwatch stopwatch;
        private protected Type baseType = typeof(TBase);
        private protected SqlConnection _connection;
        private protected SqlTransaction _transaction;
    }
}
