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
using DataTrack.Core.Interface;

namespace DataTrack.Core.SQL.ExecutionObjects
{
    public class ReadQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : IEntity, new()
    {
        internal ReadQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
            : base(query, connection, transaction)
        {

        }

        internal List<TBase> Execute(SqlDataReader reader)
        {
            List<TBase> results = new List<TBase>();
            List<Table> tables = mapping.Tables;

            stopwatch.Start();

            // TODO rewrite
            foreach (Table table in tables)
            {
                if (table.Type == baseType)
                {
                    while (reader.Read())
                    {
                        TBase obj = new TBase();

                        foreach (Column column in table.Columns)
                        {
                            PropertyInfo property = baseType.GetProperty(column.PropertyName);

                            if (reader[column.Name] != DBNull.Value)
                                property.SetValue(obj, Convert.ChangeType(reader[column.Name], property.PropertyType));
                            else
                                property.SetValue(obj, null);
                        }

                        results.Add(obj);
                    }
                }
                else
                {
                    reader.NextResult();
                    Type childType = table.Type;
                    dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));
                    int i = 0;

                    while (reader.Read())
                    {
                        var childItem = Activator.CreateInstance(childType);

                        childType.GetProperties()
                                 .ForEach(prop => prop.SetValue(childItem, Convert.ChangeType(reader[table.Columns[i++].Name], prop.PropertyType)));

                        MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                        addItem.Invoke(childCollection, new object[] { childItem });

                        i = 0;
                    }

                    foreach (TBase obj in results)
                    {
                        PropertyInfo childProperty = baseType.GetChildProperty(table.Name);
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
