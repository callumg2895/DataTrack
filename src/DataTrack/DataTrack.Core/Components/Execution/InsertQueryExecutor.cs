using DataTrack.Core.Components.Builders;
using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Components.SQL;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DataTrack.Core.Components.Execution
{
	public class InsertQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity
	{
		internal InsertQueryExecutor(EntityQuery<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
			: base(query, connection, transaction)
		{

		}

		internal bool Execute()
		{
			stopwatch.Start();

			foreach (EntityTable table in mapping.Tables)
			{
				if (mapping.DataTableMapping.ContainsKey(table))
				{
					WriteToServer(table);
				}
			}

			stopwatch.Stop();
			Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Bulk Insert ({stopwatch.GetElapsedMicroseconds()}\u03BCs)");

			return true;
		}

		private void WriteToServer(EntityTable table)
		{
			CreateStagingTable(table);

			Logger.Debug($"Executing Bulk Insert for {table.Name}");

			SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default;

			SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, copyOptions, _transaction)
			{
				DestinationTableName = table.StagingTable.Name
			};

			bulkCopy.WriteToServer(mapping.DataTableMapping[table]);

			InsertFromStagingTable(table);
		}

		private void CreateStagingTable(EntityTable table)
		{
			EntitySQLBuilder<TBase> sql = new EntitySQLBuilder<TBase>(mapping);

			sql.CreateStagingTable(table);

			using SqlCommand cmd = _connection.CreateCommand();

			cmd.CommandText = sql.ToString();
			cmd.CommandType = CommandType.Text;
			cmd.Transaction = _transaction;

			Logger.Debug($"Creating staging table {table.StagingTable.Name}");
			Logger.Debug($"Executing SQL: {sql.ToString()}");

			cmd.ExecuteNonQuery();
		}

		private void InsertFromStagingTable(EntityTable table)
		{
			EntitySQLBuilder<TBase> sql = new EntitySQLBuilder<TBase>(mapping);
			string primaryKeyColumnName = table.GetPrimaryKeyColumn().Name;

			sql.BuildInsertFromStagingToMainWithOutputIds(table);

			using SqlCommand cmd = _connection.CreateCommand();

			cmd.CommandText = sql.ToString();
			cmd.CommandType = CommandType.Text;
			cmd.Transaction = _transaction;

			int totalPrimaryKeys = ReadPrimaryKeys(cmd, table, primaryKeyColumnName);

			Logger.Debug($"{(totalPrimaryKeys == 0 ? "No" : totalPrimaryKeys.ToString())} {table.Name} were inserted");
		}

		private int ReadPrimaryKeys(SqlCommand cmd, EntityTable table, string columnName)
		{
			int primaryKeyIndex = 0;

			Logger.Debug($"Reading primary keys inserted into {table.Name}");
			Logger.Debug($"Executing SQL: {cmd.CommandText}");

			using SqlDataReader reader = cmd.ExecuteReader();
			
			while (reader.Read())
			{
				dynamic primaryKey = reader[columnName];

				Logger.Trace($"Inserted '{table.Type.Name}' entity with primary key {primaryKey.ToString()}");

				/*
				 * We need to make sure that the data rows for child entities which have a foreign key relationship
				 * to this newly inserted entity are updated with foreign keys. These are currently set to the default
				 * value of their data type, and so will all be the same - any insert operation attempted at this point
				 * would cause a foreign key exception in SQL.
				 */

				mapping.UpdateDataTableForeignKeys(table, primaryKey, primaryKeyIndex++);
			}
			
			return primaryKeyIndex;
		}

	}
}
