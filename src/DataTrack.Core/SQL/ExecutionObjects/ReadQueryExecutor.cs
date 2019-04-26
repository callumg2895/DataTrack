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
using System.Linq;

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
            Dictionary<Table, List<IEntity>> entityDictionary = new Dictionary<Table, List<IEntity>>();

            stopwatch.Start();

            foreach (Table table in tables)
            {
                while (reader.Read())
                {
                    IEntity entity = ReadEntity(reader, table);

                    if (!entityDictionary.ContainsKey(table))
                    {
                        entityDictionary.Add(table, new List<IEntity>());
                    }

                    entityDictionary[table].Add(entity);
                    
                    if (mapping.ChildParentMapping.ContainsKey(table))
                    {
                        Table parentTable = mapping.ChildParentMapping[table];
                        var foreignKey = entity.GetPropertyValue(table.GetForeignKeyColumn(parentTable.Name).PropertyName);

                        foreach(IEntity parentEntity in entityDictionary[parentTable])
                        {
                            var parentPrimaryKey = parentEntity.GetPropertyValue(parentTable.GetPrimaryKeyColumn().PropertyName);
                            if (parentPrimaryKey.ToString() == foreignKey.ToString())
                            {
                                parentEntity.AddChildPropertyValue(table.Name, entity);
                                break;
                            }
                        }
                    }
                    else
                    {
                        results.Add((TBase)entity);
                    }
                }

                if (!reader.NextResult())
                {
                    break;
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

            entity.InstantiateChildProperties();

            return entity;
        }
    }
}
