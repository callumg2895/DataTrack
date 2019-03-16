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
        public List<Table> Tables { get; set; } = new List<Table>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnAliases { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Map<Type, Table> TypeTableMapping { get; set; } = new Map<Type, Table>();
        internal Dictionary<ColumnMappingAttribute, string> ColumnPropertyNames { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        internal Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> Parameters { get; set; } = new Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>>();
        internal Dictionary<ColumnMappingAttribute, string> Restrictions { get; set; } = new Dictionary<ColumnMappingAttribute, string>();
        public Map<Table, DataTable> DataTableMapping { get; set; } = new Map<Table, DataTable>();

        public Mapping()
        {
            GetTableByType(BaseType, out TableMappingAttribute tableAttribute);
            GetColumnsByType(BaseType, out List<ColumnMappingAttribute> columnAttributes);

            Table table = new Table(BaseType, tableAttribute, columnAttributes);

            Tables.Add(table);
            TypeTableMapping[BaseType] = table;

            foreach (var prop in BaseType.GetProperties())
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

                GetTableByType(genericArgumentType, out TableMappingAttribute tableAttribute);
                GetColumnsByType(genericArgumentType, out List<ColumnMappingAttribute> columnAttributes);

                Table table = new Table(genericArgumentType, tableAttribute, columnAttributes);

                Tables.Add(table);
                TypeTableMapping[genericArgumentType] = table;

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
                    Table table = TypeTableMapping[type];

                    Dictionaries.TypeMappingCache[type] = (table.TableAttribute, table.ColumnAttributes);
                    Dictionaries.TableMappingCache[table.TableAttribute] = table.ColumnAttributes;
                }
            }
        }

        private protected bool TryGetTableMappingAttribute(Type type, out TableMappingAttribute mappingAttribute)
        {
            mappingAttribute = null;

            // Check the dictionary first to save using reflection
            if (TypeTableMapping.ContainsKey(type))
            {
                mappingAttribute = TypeTableMapping[type].TableAttribute;
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
            if (TypeTableMapping.ContainsKey(type))
            {
                attributes = TypeTableMapping[type].ColumnAttributes;
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
