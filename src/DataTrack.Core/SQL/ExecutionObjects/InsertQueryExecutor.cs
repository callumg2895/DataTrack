using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DataTrack.Core.SQL.BuilderObjects;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class InsertQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {
        internal InsertQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
        {
            Query = query;
            stopwatch = new Stopwatch();
            _connection = connection;

            if (transaction != null)
                _transaction = transaction;
        }

        internal bool Execute()
        {
            stopwatch.Start();

            foreach (TableMappingAttribute table in Query.Mapping.DataTableMapping.ForwardKeys)
            {
                WriteToServer(table);
            }

            stopwatch.Stop();
            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");

            return true;
        }

        private void WriteToServer(TableMappingAttribute table)
        {
            SQLBuilder<TBase> createStagingTable = new SQLBuilder<TBase>(Query.Mapping);
            SQLBuilder<TBase> insertFromStagingTable = new SQLBuilder<TBase>(Query.Mapping);
            List<int> ids = new List<int>();

            createStagingTable.CreateStagingTable(Dictionaries.TableMappingCache[table], table);
            insertFromStagingTable.BuildInsertFromStagingToMainWithOutputIds(Dictionaries.TableMappingCache[table], table);

            using (SqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = createStagingTable.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _transaction;

                Logger.Debug($"Creating staging table {table.StagingTableName}");
                Logger.Debug($"Executing SQL: {createStagingTable.ToString()}");

                cmd.ExecuteNonQuery();
            }

            Logger.Debug($"Executing Bulk Insert for {table.TableName}");

            SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, copyOptions, _transaction);

            bulkCopy.DestinationTableName = table.StagingTableName;
            bulkCopy.WriteToServer(Query.Mapping.DataTableMapping[table]);

            using (SqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = insertFromStagingTable.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _transaction;

                Logger.Debug($"Reading primary keys inserted into {table.TableName}");
                Logger.Debug($"Executing SQL: {insertFromStagingTable.ToString()}");

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add((int)reader["id"]);
                    }
                }
            }

            foreach (int item in ids)
            {
                Logger.Debug($"Inserted {table.TableName} item with primary key {item}");
            }
        }

    }
}
