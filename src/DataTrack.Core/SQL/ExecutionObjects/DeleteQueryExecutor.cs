﻿using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Util.Extensions;
using DataTrack.Logging;
using System.Data.SqlClient;
using System.Reflection;

namespace DataTrack.Core.SQL.ExecutionObjects
{
	public class DeleteQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity
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

			Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Delete statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {affectedRows} row{(affectedRows > 1 ? "s" : "")} affected");

			return affectedRows;
		}

	}
}
