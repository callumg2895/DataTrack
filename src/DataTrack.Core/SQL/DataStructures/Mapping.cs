using DataTrack.Core.Attributes;
using DataTrack.Core.Logging;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Mapping<TBase> where TBase : new()
    {

        public Type BaseType { get; set; } = typeof(TBase);
        public List<TableWrapper> Tables { get; set; } = new List<TableWrapper>();
        internal Dictionary<TableMappingAttribute, string> TableAliases { get; set; } = new Dictionary<TableMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnAliases { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Map<Type, TableMappingAttribute> TypeTableMapping { get; set; } = new Map<Type, TableMappingAttribute>();
        internal Map<Type, List<ColumnMappingAttribute>> TypeColumnMapping { get; set; } = new Map<Type, List<ColumnMappingAttribute>>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnPropertyNames { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> Parameters { get; set; } = new Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>>();
        internal Dictionary<ColumnMappingAttribute, string> Restrictions { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        public Map<TableMappingAttribute, DataTable> DataTableMapping { get; set; } = new Map<TableMappingAttribute, DataTable>();

        public Mapping()
        {
            GetTableByType(BaseType, out TableMappingAttribute table);
            GetColumnsByType(BaseType, out List<ColumnMappingAttribute> columns);
            Tables.Add(new TableWrapper(table, columns));

            foreach(var prop in BaseType.GetProperties())
            {
                MapTablesByProperty(prop);
            }

            CacheMappingData();
        }

        private void GetTableByType(Type type, out TableMappingAttribute table)
        {
            if (Dictionaries.TypeMappingCache.ContainsKey(type))
            {
                LoadTableMappingFromCache(type, out table);
            }
            else
            {
                LoadTableMapping(type, out table);
            }
        }

        private void MapTablesByProperty(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            // If the property is a generic list, then it fits the profile of a child object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = propertyType.GetGenericArguments()[0];

                GetTableByType(genericArgumentType, out TableMappingAttribute table);
                GetColumnsByType(genericArgumentType, out List<ColumnMappingAttribute> columns);
                Tables.Add(new TableWrapper(table, columns));

                foreach (var prop in propertyType.GetProperties())
                {
                    MapTablesByProperty(prop);
                }
            }
        }

        private void LoadTableMapping(Type type, out TableMappingAttribute table)
        {
            if (TryGetTableMappingAttribute(type, out table))
            {
                TypeTableMapping[type] = table;
                TableAliases[table] = type.Name;

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
            }
            else
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
            }
        }

        private void LoadTableMappingFromCache(Type type, out TableMappingAttribute table)
        {
            table = Dictionaries.TypeMappingCache[type].Table;

            TypeTableMapping[type] = table;
            TableAliases[table] = type.Name;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}' from cache");
        }

        private void GetColumnsByType(Type type, out List<ColumnMappingAttribute> columns)
        {
            if (!Dictionaries.TypeMappingCache.ContainsKey(type))
            {
                LoadColumnMapping(type, out columns);
            }
            else
            {
                LoadColumnMappingFromCache(type, out columns);
            }
        }

        private void LoadColumnMapping(Type type, out List<ColumnMappingAttribute> columns)
        {
            if (TryGetColumnMappingAttributes(type, out columns))
            {
                TypeColumnMapping[type] = columns;

                foreach (ColumnMappingAttribute column in columns)
                {
                    ColumnAliases[column] = $"{type.Name}.{column.ColumnName}";
                    ColumnPropertyNames[column] = column.GetPropertyName(type);
                }

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
            }
            else
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{type.Name}'");
            }
        }

        private void LoadColumnMappingFromCache(Type type, out List<ColumnMappingAttribute> columns)
        {
            columns = Dictionaries.TypeMappingCache[type].Columns;

            TypeColumnMapping[type] = columns;

            foreach (ColumnMappingAttribute column in columns)
            {
                ColumnAliases[column] = $"{type.Name}.{column.ColumnName}";
                ColumnPropertyNames[column] = column.GetPropertyName(type);
            }

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}' from cache");
        }

        private void CacheMappingData()
        {
            foreach (Type type in TypeTableMapping.ForwardKeys)
            {
                if (!Dictionaries.TypeMappingCache.ContainsKey(type))
                {
                    TableMappingAttribute table = TypeTableMapping[type];
                    List<ColumnMappingAttribute> columns = TypeColumnMapping[type];

                    Dictionaries.TypeMappingCache[type] = (table, columns);
                    Dictionaries.TableMappingCache[table] = columns;
                }
            }
        }

        private protected bool TryGetTableMappingAttribute(Type type, out TableMappingAttribute mappingAttribute)
        {
            mappingAttribute = null;

            // Check the dictionary first to save using reflection
            if (TypeTableMapping.ContainsKey(type))
            {
                mappingAttribute = TypeTableMapping[type];
                return true;
            }

            foreach (Attribute attribute in type.GetCustomAttributes())
                mappingAttribute = attribute as TableMappingAttribute;

            return mappingAttribute != null;
        }

        private protected bool TryGetColumnMappingAttributes(Type type, out List<ColumnMappingAttribute> attributes)
        {
            attributes = new List<ColumnMappingAttribute>();

            // Check the dictionary first to save using reflection
            if (TypeColumnMapping.ContainsKey(type))
            {
                attributes = TypeColumnMapping[type];
                return true;
            }

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                {
                    ColumnMappingAttribute? mappingAttribute = attribute as ColumnMappingAttribute;
                    if (mappingAttribute != null)
                    {
                        attributes.Add(mappingAttribute);
                        break;
                    }
                }

            return attributes.Count > 0;
        }
    }
}
