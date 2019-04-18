using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Logging;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    internal class Mapping<TBase> where TBase : IEntity
    {
        internal Type BaseType { get; set; }
        internal List<Table> Tables { get; set; }
        internal Dictionary<Type, Table> TypeTableMapping { get; set; }
        internal Dictionary<Table, List<Table>> ParentChildMapping { get; set; }
        internal Dictionary<IEntity, List<IEntity>> ParentChildEntityMapping { get; set; }
        internal Dictionary<IEntity, DataRow> EntityDataRowMapping { get; set; }
        internal Map<Table, DataTable> DataTableMapping { get; set; }

        internal Mapping()
        {
            BaseType = typeof(TBase);

            Tables = new List<Table>();

            TypeTableMapping = new Dictionary<Type, Table>();
            ParentChildMapping = new Dictionary<Table, List<Table>>();
            ParentChildEntityMapping = new Dictionary<IEntity, List<IEntity>>();
            EntityDataRowMapping = new Dictionary<IEntity, DataRow>();
            DataTableMapping = new Map<Table, DataTable>();

            MapTable(BaseType);
        }

        internal void UpdateDataTableForeignKeys(Table table, List<dynamic> primaryKeys)
        {
            Type type = table.Type;
            int primaryKeyIndex = 0;

            bool hasChildren = ParentChildMapping.TryGetValue(table, out List<Table> childTables);

            if (!hasChildren)
                return;

            foreach (var entity in ParentChildEntityMapping.Keys)
            {
                if (entity.GetType() != type)
                    continue;

                foreach( var childEntity in ParentChildEntityMapping[entity])
                {
                    SetForeignKeyValue(childEntity, table.Name, primaryKeys?[primaryKeyIndex] ?? 0);
                }

                primaryKeyIndex++;
            }
        }

        private void SetForeignKeyValue(IEntity item, string foreignTable, dynamic foreignKey)
        {
            Table table = TypeTableMapping[item.GetType()];
            Column column = table.GetForeignKeyColumn(foreignTable);

            EntityDataRowMapping[item][column.Name] = foreignKey;
        }

        private void MapTable(Type type)
        {
            Table table = GetTableByType(type);

            Tables.Add(table);
            TypeTableMapping.Add(type, table);
            ParentChildMapping.Add(table, new List<Table>());

            foreach (var prop in type.GetProperties())
            {
                MapTablesByProperty(prop, table);
            }
        }

        private void MapTablesByProperty(PropertyInfo property, Table parentTable)
        {
            Type propertyType = property.PropertyType;

            // If the property is a generic list, then it fits the profile of a child object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = propertyType.GetGenericArguments()[0];

                MapTable(genericArgumentType);

                Table mappedTable = TypeTableMapping[genericArgumentType];

                ParentChildMapping[parentTable].Add(mappedTable);
            }
        }

        private Table GetTableByType(Type type)
        {
            return Dictionaries.TypeMappingCache.ContainsKey(type)
                ? LoadTableMappingFromCache(type)
                : LoadTableMapping(type);
        }

        private Table LoadTableMapping(Type type)
        {
            if (TryGetTable(type, out Table? table) && table != null)
            {
                Logger.Trace($"Caching database mapping for Entity '{type.Name}'");
                Dictionaries.TypeMappingCache[type] = table;

                return (Table)table.Clone();
            }

            Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load Table object for '{type.Name}' entity");
            throw new TableMappingException(type, string.Empty);       
        }

        private Table LoadTableMappingFromCache(Type type)
        {
            Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity from cache");
            return (Table)Dictionaries.TypeMappingCache[type].Clone();
        }

        private protected bool TryGetTable(Type type, out Table? table)
        {
            Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity");

            TableMappingAttribute? tableAttribute = null;
            List<ColumnMappingAttribute> columnAttributes = new List<ColumnMappingAttribute>();

            foreach (Attribute attribute in type.GetCustomAttributes())
                tableAttribute = attribute as TableMappingAttribute;

            if (tableAttribute == null)
                throw new NullReferenceException($"Could not find TableMappingAttribute for type {type.Name}");

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.Name == "ID")
                {
                    columnAttributes.Add(new ColumnMappingAttribute(tableAttribute.TableName, "id", (byte)KeyTypes.PrimaryKey));
                    continue;
                }

                foreach (Attribute attribute in property.GetCustomAttributes())
                {
                    ColumnMappingAttribute? mappingAttribute = attribute as ColumnMappingAttribute;
                    if (mappingAttribute != null)
                    {
                        columnAttributes.Add(mappingAttribute);
                        break;
                    }
                }
            }

            if (tableAttribute != null && columnAttributes.Count > 0)
            {
                table = new Table(type, tableAttribute, columnAttributes);
                return true;
            }

            table = null;
            return false;          
        }
    }
}
