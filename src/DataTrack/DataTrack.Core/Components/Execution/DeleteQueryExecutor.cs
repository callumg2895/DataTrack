using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Extensions;
using System.Data.SqlClient;
using System.Reflection;

namespace DataTrack.Core.Components.Execution
{
	public class DeleteQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		internal DeleteQueryExecutor(EntityQuery<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
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
