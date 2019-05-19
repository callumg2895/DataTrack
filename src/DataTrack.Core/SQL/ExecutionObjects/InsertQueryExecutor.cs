using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DataTrack.Core.SQL.BuilderObjects;
using DataTrack.Core.Interface;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class InsertQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity
    {
        internal InsertQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
            : base(query, connection, transaction)
        {

        }

        internal bool Execute()
        {
            stopwatch.Start();

            foreach (Table table in mapping.Tables)
            {
                if (mapping.DataTableMapping.ContainsKey(table))
                    WriteToServer(table);
            }

            stopwatch.Stop();
            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");

            return true;
        }

        private void WriteToServer(Table table)
        {
            CreateStagingTable(table);

            Logger.Debug($"Executing Bulk Insert for {table.Name}");

            SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, copyOptions, _transaction);

            bulkCopy.DestinationTableName = table.StagingName;
            bulkCopy.WriteToServer(mapping.DataTableMapping[table]);

            InsertFromStagingTable(table);
        }

        private void CreateStagingTable(Table table)
        {
            SQLBuilder<TBase> sql = new SQLBuilder<TBase>(mapping);

            sql.CreateStagingTable(table);

            using (SqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _transaction;

                Logger.Debug($"Creating staging table {table.StagingName}");
                Logger.Debug($"Executing SQL: {sql.ToString()}");

                cmd.ExecuteNonQuery();
            }
        }

        private void InsertFromStagingTable(Table table)
        {
            SQLBuilder<TBase> sql = new SQLBuilder<TBase>(mapping);
            string primaryKeyColumnName = table.GetPrimaryKeyColumn().Name;
            List<dynamic> ids = new List<dynamic>();

            sql.BuildInsertFromStagingToMainWithOutputIds(table);

            using (SqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = _transaction;

                Logger.Debug($"Reading primary keys inserted into {table.Name}");
                Logger.Debug($"Executing SQL: {sql.ToString()}");

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader[primaryKeyColumnName];
                        ids.Add(id);
                        Logger.Trace($"Inserted {table.Name} entity with primary key {id.ToString()}");
                    }
                }

                Logger.Debug($"{(ids.Count == 0 ? "No" : ids.Count.ToString())} {table.Name} were inserted");
                mapping.UpdateDataTableForeignKeys(table, ids);
            }
        }
    }
}
