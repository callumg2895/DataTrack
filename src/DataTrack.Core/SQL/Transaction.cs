using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL
{
    public abstract class Transaction
    {
        private protected StringBuilder transactionSQLBuilder;
        private protected string transactionSQL;
    }

    public class Transaction<T> : Transaction where T : QueryBuilder<T>
    { 
        private List<QueryBuilder<T>> queryBuilders = new List<QueryBuilder<T>>();
        private List<(string Handle, object Value)> parameters = new List<(string Handle, object Value)>();

        public Transaction(QueryBuilder<T> queryBuilder) => BuildTransactionData(queryBuilder);

        public Transaction(List<QueryBuilder<T>> queryBuilders)
        {
            foreach (QueryBuilder<T> queryBuilder in queryBuilders) BuildTransactionData(queryBuilder);
        }

        private void BuildTransactionData(QueryBuilder<T> queryBuilder)
        {
            queryBuilders.Add(queryBuilder);
            parameters.AddRange(queryBuilder.GetParameters().Where( p => !parameters.Select(pa => pa.Handle).Contains(p.Handle)));
        }

        public string BuildTransactionSQL()
        {
            transactionSQLBuilder = new StringBuilder();

            foreach(QueryBuilder<T> queryBuilder in queryBuilders)
                transactionSQLBuilder.AppendLine(queryBuilder.ToString());

            transactionSQL = transactionSQLBuilder.ToString();

            return transactionSQL;
        }
    }
}
