using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryBuilders;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.SQL
{
    public class Transaction<TBase> where TBase : new()
    {
        #region Members

        private List<QueryBuilder<TBase>> queryBuilders = new List<QueryBuilder<TBase>>();
        private List<(string Handle, object Value)> parameters = new List<(string Handle, object Value)>();
        private Dictionary<QueryBuilder<TBase>, string> querySQLMapping = new Dictionary<QueryBuilder<TBase>, string>();

        #endregion

        #region Constructors

        public Transaction(QueryBuilder<TBase> queryBuilder) => BuildTransactionData(queryBuilder);

        public Transaction(List<QueryBuilder<TBase>> queryBuilders)
        {
            foreach (QueryBuilder<TBase> queryBuilder in queryBuilders) BuildTransactionData(queryBuilder);
        }

        #endregion

        #region Methods

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
                            case CRUDOperationTypes.Read:

                                GetResultsForReadQueryBuilder(reader, (ReadQueryBuilder<TBase>)queryBuilder, ref results);
                                break;


                            case CRUDOperationTypes.Create:

                                GetResultForInsertQueryBuilder(reader, (InsertQueryBuilder<TBase>)queryBuilder, ref results);
                                break;

                            case CRUDOperationTypes.Update:

                                // Update operations always check the number of rows affected after the query has executed
                                if (reader.Read())
                                    results.Add((int)(object)reader["affected_rows"]);

                                reader.NextResult();

                                break;

                            case CRUDOperationTypes.Delete:
                                break;
                        }
                    }

                }
            }

            return results;
        }

        private string BuildTransactionSQL()
        {
            StringBuilder transactionSQLBuilder = new StringBuilder();

            foreach (QueryBuilder<TBase> queryBuilder in queryBuilders)
            {
                Thread thread = new Thread(() => BuildQuery(queryBuilder));
                thread.Start();
            }

            while (true)
            {
                lock (querySQLMapping)
                {
                    bool done = true;
                    foreach (QueryBuilder<TBase> queryBuilder in queryBuilders)
                        done &= querySQLMapping.ContainsKey(queryBuilder);

                    if (done)
                        break;
                }
            }

            foreach (QueryBuilder<TBase> queryBuilder in queryBuilders)
            {
                transactionSQLBuilder.Append(querySQLMapping[queryBuilder]);
                parameters.AddRange(queryBuilder.GetParameters().Where(p => !parameters.Select(pa => pa.Handle).Contains(p.Handle)));
            }

            return transactionSQLBuilder.ToString();
        }

        private void BuildTransactionData(QueryBuilder<TBase> queryBuilder) => queryBuilders.Add(queryBuilder);

        private void BuildQuery(QueryBuilder<TBase> queryBuilder)
        {
            string sql = queryBuilder.ToString();

            lock (querySQLMapping)
            {
                querySQLMapping[queryBuilder] = sql;
            }
        }

        private void GetResultForInsertQueryBuilder(SqlDataReader reader, InsertQueryBuilder<TBase> queryBuilder, ref List<object> results)
        {
            int affectedRows = 0;

            // Create operations always check the number of rows affected after the query has executed
            for (int tableCount = 0; tableCount < queryBuilder.Tables.Count; tableCount++)
            {
                if (tableCount == 0)
                {
                    if (reader.Read())
                        affectedRows += (int)(object)reader["affected_rows"];

                    reader.NextResult();
                }
                else
                {
                    dynamic childCollection = queryBuilder.Tables[0].GetChildPropertyValues(
                        queryBuilder.GetPropertyValue("Item"),
                        queryBuilder.Tables[tableCount].TableName);

                    foreach (var item in childCollection)
                    {
                        if (reader.Read())
                            affectedRows += (int)(object)reader["affected_rows"];

                        reader.NextResult();
                    }
                }
            }

            results.Add(affectedRows);
        }

        private void GetResultsForReadQueryBuilder(SqlDataReader reader, ReadQueryBuilder<TBase> queryBuilder, ref List<object> results)
        {
            List<TBase> readQueryResults = new List<TBase>();

            List<ColumnMappingAttribute> mainColumns = Dictionaries.MappingCache[typeof(TBase)].Columns;

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
                Type childType = queryBuilder.TypeTableMapping[queryBuilder.Tables[tableCount]];
                dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));
                int originalColumnCount = columnCount;

                while (reader.Read())
                {
                    var childItem = Activator.CreateInstance(childType);
                    columnCount = originalColumnCount;

                    foreach(PropertyInfo property in childType.GetProperties())
                    {
                        property.SetValue(
                            childItem,
                            Convert.ChangeType(reader[queryBuilder.Columns[columnCount++].ColumnName], property.PropertyType));
                    }

                    MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                    addItem.Invoke(childCollection, new object[] { childItem });
                }

                foreach(TBase obj in readQueryResults)
                {
                    PropertyInfo childProperty = queryBuilder.Tables[0].GetChildProperty(typeof(TBase), queryBuilder.Tables[tableCount].TableName);
                    childProperty.SetValue(obj, childCollection);
                }
            }

            results.Add(readQueryResults);
            reader.NextResult();
        }

        #endregion
    }
}
