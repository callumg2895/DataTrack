using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
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
    public class Mapping<TBase> where TBase : Entity
    {
        public Type BaseType { get; set; } = typeof(TBase);
        public List<Table> Tables { get; set; } = new List<Table>();
        internal Dictionary<Type, Table> TypeTableMapping { get; set; } = new Dictionary<Type, Table>();
        internal Dictionary<Table, List<Table>> ParentChildMapping { get; set; } = new Dictionary<Table, List<Table>>();
        public Map<Table, DataTable> DataTableMapping { get; set; } = new Map<Table, DataTable>();

        public Mapping()
        {
            MapTable(BaseType);
            LogTableRelationships();
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
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded Table object for '{type.Name}' entity");
                Dictionaries.TypeMappingCache[type] = table;
                return (Table)table.Clone();
            }

            Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load Table object for '{type.Name}' entity");
            throw new TableMappingException(type, string.Empty);       
        }

        private Table LoadTableMappingFromCache(Type type)
        {
            Table table = Dictionaries.TypeMappingCache[type];

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded Table object for '{type.Name}' entity from cache");

            return (Table)table.Clone();
        }

        private protected bool TryGetTable(Type type, out Table? table)
        {
            TableMappingAttribute? tableAttribute = null;
            List<ColumnMappingAttribute> columnAttributes = new List<ColumnMappingAttribute>();

            foreach (Attribute attribute in type.GetCustomAttributes())
                tableAttribute = attribute as TableMappingAttribute;

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                {
                    ColumnMappingAttribute? mappingAttribute = attribute as ColumnMappingAttribute;
                    if (mappingAttribute != null)
                    {
                       columnAttributes.Add(mappingAttribute);
                        break;
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

        private void LogTableRelationships()
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine();

            foreach (Table table in Tables)
            {
                logMessage.AppendLine($"Mapped table '{table.Name}' with {ParentChildMapping[table].Count} child table(s):");

                foreach (Table childTable in ParentChildMapping[table])
                {
                    logMessage.AppendLine($"- {childTable.Name}");
                }

                logMessage.AppendLine();
            }

            Logger.Debug(logMessage.ToString());
        }
    }
}
