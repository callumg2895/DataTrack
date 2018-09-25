using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Sql.Read;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL
{
    public abstract class Transaction
    {
        private protected StringBuilder transactionSQLBuilder;
        private protected string transactionSQL;
    }

    public class Transaction<T, TBase> : Transaction where T : QueryBuilder<TBase>
    { 
        private List<QueryBuilder<TBase>> queryBuilders = new List<QueryBuilder<TBase>>();
        private List<(string Handle, object Value)> parameters = new List<(string Handle, object Value)>();

        public Transaction(QueryBuilder<TBase> queryBuilder) => BuildTransactionData(queryBuilder);

        public Transaction(List<QueryBuilder<TBase>> queryBuilders)
        {
            foreach (QueryBuilder<TBase> queryBuilder in queryBuilders) BuildTransactionData(queryBuilder);
        }

        private void BuildTransactionData(QueryBuilder<TBase> queryBuilder)
        {
            queryBuilders.Add(queryBuilder);
            parameters.AddRange(queryBuilder.GetParameters().Where( p => !parameters.Select(pa => pa.Handle).Contains(p.Handle)));
        }

        public string BuildTransactionSQL()
        {
            transactionSQLBuilder = new StringBuilder();

            foreach(QueryBuilder<TBase> queryBuilder in queryBuilders)
                transactionSQLBuilder.AppendLine(queryBuilder.ToString());

            transactionSQL = transactionSQLBuilder.ToString();

            return transactionSQL;
        }

        public List<object> Execute()
        {
            List<object> results = new List<object>();

            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                SqlCommand command = connection.CreateCommand();

                command.CommandType = CommandType.Text;
                command.CommandText = BuildTransactionSQL();
                command.AddParameters(parameters);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    foreach (QueryBuilder<TBase> queryBuilder in queryBuilders)
                    {
                        switch (queryBuilder.OperationType)
                        {
                            case CRUDOperationTypes.Read :
                                List<TBase> readResults = new List<TBase>();
                                while (reader.Read())
                                {
                                    TBase obj = default;
                                    foreach (ColumnMappingAttribute column in queryBuilder.Columns)
                                    {
                                        string propertyName;
                                        if (column.TryGetPropertyName(typeof(TBase), out propertyName))
                                        {
                                            PropertyInfo property = typeof(TBase).GetProperty(propertyName);
                                            if (reader[column.ColumnName] != DBNull.Value)
                                                property.SetValue(obj, Convert.ChangeType(reader[column.ColumnName], property.PropertyType));
                                            else
                                                property.SetValue(obj, null);
                                        }
                                        else
                                        {
                                            Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find property in class {typeof(TBase)} mapped to column {column.ColumnName}");
                                            break;
                                        }
                                    }
                                    readResults.Add(obj);
                                }
                                results.Add(readResults);
                                reader.NextResult();
                                break;
                            case CRUDOperationTypes.Create :
                            case CRUDOperationTypes.Update :
                            case CRUDOperationTypes.Delete :
                                break;
                        }
                    }

                }
            }

            return results;
        }
    }
}
