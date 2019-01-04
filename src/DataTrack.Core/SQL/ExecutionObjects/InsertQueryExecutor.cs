using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class InsertQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {
        internal InsertQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction transaction = null)
        {
            Query = query;
            stopwatch = new Stopwatch();
            _connection = connection;
            _transaction = transaction;
        }

        internal bool Execute()
        {
            stopwatch.Start();

            foreach (TableMappingAttribute table in Query.DataMap.ForwardKeys)
            {
                WriteToServer(table);
            }

            stopwatch.Stop();
            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");

            return true;
        }

        private void WriteToServer(TableMappingAttribute table)
        {
            Logger.Info($"Executing Bulk Insert for {table.TableName}");

            SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, copyOptions, _transaction);

            bulkCopy.DestinationTableName = Query.DataMap[table].TableName;
            bulkCopy.WriteToServer(Query.DataMap[table]);
        }

    }
}
