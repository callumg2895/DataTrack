using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Interface;
using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataTrack.Core.Components.Execution
{
	public abstract class QueryExecutor<TBase> where TBase : IEntity
	{
		private protected Mapping<TBase> mapping;
		private protected Stopwatch stopwatch;
		private protected Type baseType;
		private protected SqlConnection _connection;
		private protected SqlTransaction? _transaction;

		public QueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction)
		{
			stopwatch = new Stopwatch();
			mapping = query.Mapping;
			baseType = typeof(TBase);
			_connection = connection;

			if (transaction != null)
			{
				_transaction = transaction;
			}
		}
	}
}
