using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.BuilderObjects
{
    internal class MappingBuilder<TBase> where TBase : new()
    {
        #region Members

        private readonly Type BaseType = typeof(TBase);
        private Mapping<TBase> Mapping = new Mapping<TBase>();

        #endregion

        #region Methods

        public Mapping<TBase> GetMapping()
        {
            MapTables();
            MapColumns();
            CacheMappingData();

            return Mapping;
        }

        private void MapTables()
        {
            Type type = typeof(TBase);

            // Get the table mapping for TBase
            if (!Dictionaries.TypeMappingCache.ContainsKey(type))
            {
                LoadTableMapping(type);
            }
            else
            {
                LoadTableMappingFromCache(type);
            }

            // Get the table mapping for all child objects
            type.GetProperties().ForEach(prop => MapPropertyTables(prop));
        }

        private void MapPropertyTables(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            // If the property is a generic list, then it fits the profile of a child object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = propertyType.GetGenericArguments()[0];

                if (!Dictionaries.TypeMappingCache.ContainsKey(genericArgumentType))
                {
                    LoadTableMapping(genericArgumentType);
                }
                else
                {
                    LoadTableMappingFromCache(genericArgumentType);
                }

                propertyType.GetProperties().ForEach(prop => MapPropertyTables(prop));
            }
        }

        private void LoadTableMapping(Type type)
        {
            TableMappingAttribute table;

            if (TryGetTableMappingAttribute(type, out table))
            {
                Mapping.TypeTableMapping[type] = table;
                Mapping.Tables.Add(table);
                Mapping.TableAliases[table] = type.Name;

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
            }
            else
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
            }
        }

        private void LoadTableMappingFromCache(Type type)
        {
            TableMappingAttribute table = Dictionaries.TypeMappingCache[type].Table;

            Mapping.TypeTableMapping[type] = table;
            Mapping.Tables.Add(table);
            Mapping.TableAliases[table] = type.Name;

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}' from cache");
        }

        private void MapColumns()
        {
            if (!Dictionaries.TypeMappingCache.ContainsKey(BaseType))
            {
                LoadColumnMapping(BaseType);
            }
            else
            {
                LoadColumnMappingFromCache(BaseType);
            }

            BaseType.GetProperties().ForEach(prop => MapPropertyColumns(prop));
        }

        private void MapPropertyColumns(PropertyInfo property)
        {
            Type type = property.PropertyType;

            // If the property is a generic list, then it fits the profile of a child object
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = type.GetGenericArguments()[0];

                if (!Dictionaries.TypeMappingCache.ContainsKey(genericArgumentType))
                {
                    LoadColumnMapping(genericArgumentType);
                }
                else
                {
                    LoadColumnMappingFromCache(genericArgumentType);
                }

                genericArgumentType.GetProperties().ForEach(prop => MapPropertyColumns(prop));
            }
        }

        private void LoadColumnMapping(Type type)
        {
            List<ColumnMappingAttribute> columns;

            if (TryGetColumnMappingAttributes(type, out columns))
            {
                Mapping.TypeColumnMapping[type] = columns;
                Mapping.Columns.AddRange(columns);

                foreach (ColumnMappingAttribute column in columns)
                {
                    Mapping.ColumnAliases[column] = $"{type.Name}.{column.ColumnName}";
                    Mapping.ColumnPropertyNames[column] = column.GetPropertyName(type);
                }

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
            }
            else
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{type.Name}'");
            }
        }

        private void LoadColumnMappingFromCache(Type type)
        {
            List<ColumnMappingAttribute> columns = Dictionaries.TypeMappingCache[type].Columns;

            Mapping.TypeColumnMapping[type] = columns;
            Mapping.Columns.AddRange(columns);

            foreach (ColumnMappingAttribute column in columns)
            {
                Mapping.ColumnAliases[column] = $"{type.Name}.{column.ColumnName}";
                Mapping.ColumnPropertyNames[column] = column.GetPropertyName(type);
            }

            Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}' from cache");
        }

        private void CacheMappingData()
        {
            foreach (Type type in Mapping.TypeTableMapping.ForwardKeys)
            {
                if (!Dictionaries.TypeMappingCache.ContainsKey(type))
                {
                    TableMappingAttribute table = Mapping.TypeTableMapping[type];
                    List<ColumnMappingAttribute> columns = Mapping.TypeColumnMapping[type];

                    Dictionaries.TypeMappingCache[type] = (table, columns);
                    Dictionaries.TableMappingCache[table] = columns;
                }
            }
        }

        private protected bool TryGetTableMappingAttribute(Type type, out TableMappingAttribute mappingAttribute)
        {
            mappingAttribute = null;

            // Check the dictionary first to save using reflection
            if (Mapping.TypeTableMapping.ContainsKey(type))
            {
                mappingAttribute = Mapping.TypeTableMapping[type];
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
            if (Mapping.TypeColumnMapping.ContainsKey(type))
            {
                attributes = Mapping.TypeColumnMapping[type];
                return true;
            }

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                {
                    ColumnMappingAttribute mappingAttribute = attribute as ColumnMappingAttribute;
                    if (mappingAttribute != null)
                    {
                        attributes.Add(mappingAttribute);
                        break;
                    }
                }

            return attributes.Count > 0;
        }

        #endregion
    }
}
