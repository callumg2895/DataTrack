using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using DataTrack.Core.Logging;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class DeleteQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : Entity, new()
    {
        internal DeleteQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
            : base(query, connection, transaction)
        {

        }

        internal int Execute(SqlDataReader reader)
        {
            stopwatch.Start();

            // Delete operations always check the number of rows affected after the query has executed
            int affectedRows = reader.Read() ? (int)reader["affected_rows"] : 0;

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Update statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");

            return affectedRows;
        }

    }
}
