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
        private List<TBase> results;
        private List<Table> tables;
        private Dictionary<Table, List<IEntity>> entityDictionary;

        internal ReadQueryExecutor(Query<TBase> query, SqlConnection connection, SqlTransaction? transaction = null)
            : base(query, connection, transaction)
        {
            results = new List<TBase>();
            tables = mapping.Tables;
            entityDictionary = new Dictionary<Table, List<IEntity>>();
        }

        internal List<TBase> Execute(SqlDataReader reader)
        {
            stopwatch.Start();

            foreach (Table table in tables)
            {
                ReadResultsForTable(reader, table);

                reader.NextResult();
            }

            stopwatch.Stop();

            Logger.Info(MethodBase.GetCurrentMethod(), $"Executed Read statement ({stopwatch.GetElapsedMicroseconds()}\u03BCs): {results.Count} result{(results.Count > 1 ? "s" : "")} retrieved");

            return results;
        }

        private void ReadResultsForTable(SqlDataReader reader, Table table)
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
                    AssociateWithParent(entity, table);
                }
                else
                {
                    results.Add((TBase)entity);
                }
            }
        }

        private void AssociateWithParent(IEntity entity, Table table)
        {
            Table parentTable = mapping.ChildParentMapping[table];

            foreach (IEntity parentEntity in entityDictionary[parentTable])
            {
                object foreignKey = entity.GetPropertyValue(table.GetForeignKeyColumn(parentTable.Name).PropertyName);
                object parentPrimaryKey = parentEntity.GetPropertyValue(parentTable.GetPrimaryKeyColumn().PropertyName);

                if (parentPrimaryKey.Equals(foreignKey))
                {
                    parentEntity.AddChildPropertyValue(table.Name, entity);
                    break;
                }
            }
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
