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

			using (SqlCommand cmd = _connection.CreateCommand())
			{
				cmd.CommandText = sql.ToString();
				cmd.CommandType = CommandType.Text;
				cmd.Transaction = _transaction;

				Logger.Debug($"Creating staging table {table.StagingTable.Name}");
				Logger.Debug($"Executing SQL: {sql.ToString()}");

				cmd.ExecuteNonQuery();
			}
		}

		private void InsertFromStagingTable(EntityTable table)
		{
			EntitySQLBuilder<TBase> sql = new EntitySQLBuilder<TBase>(mapping);
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
						object id = reader[primaryKeyColumnName];
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
