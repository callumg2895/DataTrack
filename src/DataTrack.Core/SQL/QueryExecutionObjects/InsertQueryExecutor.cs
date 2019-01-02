using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.QueryObjects;
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

namespace DataTrack.Core.SQL.QueryExecutionObjects
{
    public class InsertQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {
        internal InsertQueryExecutor(Query<TBase> query)
        {
            Query = query;
            stopwatch = new Stopwatch();
        }

        internal bool Execute(SqlConnection connection, SqlTransaction transaction = null)
        {
            stopwatch.Start();

            foreach (TableMappingAttribute table in Query.DataMap.ForwardKeys)
            {
                Logger.Info($"Executing Bulk Insert for {table.TableName}");

                SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, copyOptions, transaction);

                bulkCopy.DestinationTableName = Query.DataMap[table].TableName;
                bulkCopy.WriteToServer(Query.DataMap[table]);
            }

            stopwatch.Stop();
            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");

            return true;
        }

    }
}
