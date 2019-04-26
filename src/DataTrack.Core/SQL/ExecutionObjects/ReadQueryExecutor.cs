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

            foreach (Table table in tables)
            {
                if (table.Type == baseType)
                {
                    while (reader.Read())
                    {
                        TBase obj = (TBase)ReadEntity(reader, table);

                        results.Add(obj);
                    }
                }
                else
                {
                    reader.NextResult();
                    Type childType = table.Type;
                    dynamic childCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

                    while (reader.Read())
                    {
                        var childItem = ReadEntity(reader, table);

                        MethodInfo addItem = childCollection.GetType().GetMethod("Add");
                        addItem.Invoke(childCollection, new object[] { childItem });
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

        private IEntity ReadEntity(SqlDataReader reader, Table table)
        {
            Type type = table.Type;
            IEntity entity = (IEntity)Activator.CreateInstance(type);

            foreach(Column column in table.Columns)
            {
                PropertyInfo property = type.GetProperty(column.PropertyName);

                property.SetValue(entity, Convert.ChangeType(reader[column.Name], property.PropertyType));
            }

            return entity;
        }
    }
}
