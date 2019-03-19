﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
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
    public class Mapping<TBase> where TBase : Entity
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
            GetTableByType(BaseType, out Table table);

            Tables.Add(table);
            TypeTableMapping[BaseType] = table;

            foreach (var prop in BaseType.GetProperties())
            {
                MapTablesByProperty(prop);
            }

            CacheMappingData();
        }

        private void GetTableByType(Type type, out Table table)
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

                GetTableByType(genericArgumentType, out Table table);

                Tables.Add(table);
                TypeTableMapping[genericArgumentType] = table;

                foreach (var prop in propertyType.GetProperties())
                {
                    MapTablesByProperty(prop);
                }
            }
        }
        
        private void LoadTableMapping(Type type, out Table table)
        {
            if (TryGetTable(type, out table))
            {
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
            }
            else
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
            }

            if (table == null)
            {
                throw new TableMappingException(type, string.Empty);
            }
        }

        private void LoadTableMappingFromCache(Type type, out Table table)
        {
            table = Dictionaries.TypeMappingCache[type];

            foreach (ColumnMappingAttribute column in table.ColumnAttributes)
            {
                ColumnAliases[column] = $"{type.Name}.{column.ColumnName}";
                ColumnPropertyNames[column] = column.GetPropertyName(type);
            }

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}' from cache");
        }

        private void CacheMappingData()
        {
            foreach (Type type in TypeTableMapping.ForwardKeys)
            {
                if (!Dictionaries.TypeMappingCache.ContainsKey(type))
                {
                    Table table = TypeTableMapping[type];

                    Dictionaries.TypeMappingCache[type] = table;
                }
            }
        }

        private protected bool TryGetTable(Type type, out Table? table)
        {
            table = null;

            TableMappingAttribute tableAttribute = null;
            List<ColumnMappingAttribute> columnAttributes = new List<ColumnMappingAttribute>();

            // Check the dictionary first to save using reflection
            if (TypeTableMapping.ContainsKey(type))
            {
                table = TypeTableMapping[type];
                return true;
            }

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

            if (tableAttribute != null)
            {
                table = new Table(type, tableAttribute, columnAttributes);
                return true;
            }
            else
            {
                table = null;
                return false;
            }
        }
    }
}
