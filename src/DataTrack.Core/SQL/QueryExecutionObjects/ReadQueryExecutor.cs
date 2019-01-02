using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryExecutionObjects
{
    public class ReadQueryExecutor<TBase> : QueryExecutor<TBase> where TBase : new()
    {
        internal ReadQueryExecutor(Query<TBase> query)
        {
            Query = query;
            stopwatch = new Stopwatch();
        }

        internal List<TBase> Execute(SqlDataReader reader)
        {
            List<TBase> results = new List<TBase>();

            List<ColumnMappingAttribute> mainColumns = Query.Mapping.TypeColumnMapping[baseType];

            int columnCount = 0;
            int originalColumnCount = 0;

            stopwatch.Start();

            while (reader.Read())
            {
                TBase obj = new TBase();
                columnCount = originalColumnCount;

                foreach (ColumnMappingAttribute column in mainColumns)
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

                    columnCount++;
                }

                results.Add(obj);
            }

            for (int tableCount = 1; tableCount < Query.Mapping.Tables.Count; tableCount++)
            {
                reader.NextResult();
                Type childType = Query.Mapping.TypeTableMapping[Query.Mapping.Tables[tableCount]];
                dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));
                originalColumnCount = columnCount;

                while (reader.Read())
                {
                    var childItem = Activator.CreateInstance(childType);
                    columnCount = originalColumnCount;

                    childType.GetProperties()
                             .ForEach(prop => prop.SetValue(childItem, Convert.ChangeType(reader[Query.Mapping.Columns[columnCount++].ColumnName], prop.PropertyType)));

                    MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                    addItem.Invoke(childCollection, new object[] { childItem });
                }

                foreach (TBase obj in results)
                {
                    PropertyInfo childProperty = Query.Mapping.Tables[0].GetChildProperty(baseType, Query.Mapping.Tables[tableCount].TableName);
                    childProperty.SetValue(obj, childCollection);
                }
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

            return results;
        }
    }
}
