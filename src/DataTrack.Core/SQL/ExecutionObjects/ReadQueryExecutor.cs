using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class ReadQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {
        internal ReadQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
        {
            Query = query;
            stopwatch = new Stopwatch();
            _connection = connection;

            if (transaction != null)
                _transaction = transaction;
        }

        internal List<TBase> Execute(SqlDataReader reader)
        {
            List<TBase> results = new List<TBase>();
            List<Table> tables = Query.Mapping.Tables;

            stopwatch.Start();

            foreach (Table table in tables)
            {
                if (Query.Mapping.TypeTableMapping[table] == baseType)
                {
                    while (reader.Read())
                    {
                        TBase obj = new TBase();

                        foreach (ColumnMappingAttribute column in table.ColumnAttributes)
                        {
                            if (Query.Mapping.ColumnPropertyNames.ContainsKey(column))
                            {
                                PropertyInfo property = baseType.GetProperty(Query.Mapping.ColumnPropertyNames[column]);

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

                        results.Add(obj);
                    }
                }
                else
                {
                    reader.NextResult();
                    Type childType = Query.Mapping.TypeTableMapping[table];
                    dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));
                    int i = 0;

                    while (reader.Read())
                    {
                        var childItem = Activator.CreateInstance(childType);

                        childType.GetProperties()
                                 .ForEach(prop => prop.SetValue(childItem, Convert.ChangeType(reader[table.ColumnAttributes[i++].ColumnName], prop.PropertyType)));

                        MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                        addItem.Invoke(childCollection, new object[] { childItem });

                        i = 0;
                    }

                    foreach (TBase obj in results)
                    {
                        PropertyInfo childProperty = Query.Mapping.Tables[0].TableAttribute.GetChildProperty(baseType, table.TableAttribute.TableName);
                        childProperty.SetValue(obj, childCollection);
                    }
                }
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

            return results;
        }
    }
}
