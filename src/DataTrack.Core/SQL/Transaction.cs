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
    public class Transaction<TBase> where TBase : new()
    {
        private StringBuilder transactionSQLBuilder;
        private string transactionSQL;
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

        }

        public string BuildTransactionSQL()
        {
            transactionSQLBuilder = new StringBuilder();

            foreach (QueryBuilder<TBase> queryBuilder in queryBuilders)
            {
                transactionSQLBuilder.AppendLine(queryBuilder.ToString());
                parameters.AddRange(queryBuilder.GetParameters().Where(p => !parameters.Select(pa => pa.Handle).Contains(p.Handle)));
            }

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

                                GetResultsForReadQueryBuilder(reader, (ReadQueryBuilder<TBase>)queryBuilder, ref results);
                                reader.NextResult();
                                break;


                            case CRUDOperationTypes.Create :
                            case CRUDOperationTypes.Update :

                                // Create and Update operations always check the number of rows affected after the query has executed
                                if (reader.Read());
                                    results.Add((object)reader["affected_rows"]);
                                reader.NextResult();
                                break;

                            case CRUDOperationTypes.Delete :
                                break;
                        }
                    }

                }
            }

            return results;
        }

        private void GetResultsForReadQueryBuilder(SqlDataReader reader, ReadQueryBuilder<TBase> queryBuilder, ref List<object> results)
        {
            List<TBase> readQueryResults = new List<TBase>();

            List<ColumnMappingAttribute> mainColumns = queryBuilder.Columns.Where(c => c.TableName == queryBuilder.Tables[0].TableName).ToList();

            int columnCount = 0;

            while (reader.Read())
            {
                TBase obj = new TBase();

                foreach (ColumnMappingAttribute column in mainColumns)
                {
                    if (queryBuilder.ColumnPropertyNames.ContainsKey(column))
                    {
                        PropertyInfo property = typeof(TBase).GetProperty(queryBuilder.ColumnPropertyNames[column]);

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

                    columnCount++;
                    readQueryResults.Add(obj);
                }
            }

            for (int tableCount = 1; tableCount < queryBuilder.Tables.Count; tableCount++)
            {
                reader.NextResult();
                Type childType = queryBuilder.TableTypeMapping[queryBuilder.Tables[tableCount]];
                dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType)); 

                while (reader.Read())
                {
                    var childItem = Activator.CreateInstance(childType);

                    foreach(PropertyInfo property in childType.GetProperties())
                    {
                        property.SetValue(
                            childItem,
                            Convert.ChangeType(reader[queryBuilder.Columns[++columnCount].ColumnName], property.PropertyType));
                    }

                    childCollection.Add(childItem);
                }

                foreach(TBase obj in readQueryResults)
                {
                    PropertyInfo childProperty = queryBuilder.Tables[0].GetChildProperty(typeof(TBase), queryBuilder.Tables[tableCount].TableName);
                    childProperty.SetValue(obj, childCollection);
                }
            }

            results.Add(readQueryResults);
        }
    }
}
