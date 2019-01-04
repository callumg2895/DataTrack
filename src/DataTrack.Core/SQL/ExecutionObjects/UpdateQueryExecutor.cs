using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class UpdateQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {

        internal UpdateQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction transaction = null)
        {
            Query = query;
            stopwatch = new Stopwatch();
            _connection = connection;
            _transaction = transaction;
        }

        internal int Execute(SqlDataReader reader)
        {
            stopwatch.Start();

            // Update operations always check the number of rows affected after the query has executed
            int affectedRows = reader.Read() ? (int)reader["affected_rows"] : 0;

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Update statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");

            return affectedRows;
        }

    }
}
