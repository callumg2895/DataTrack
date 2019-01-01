using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
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
            TableMappingAttribute mappingAttribute;

            // Get the table mapping for TBase
            if (!Dictionaries.TypeMappingCache.ContainsKey(type))
            {
                if (TryGetTableMappingAttribute(type, out mappingAttribute))
                {
                    Mapping.TypeTableMapping[type] = mappingAttribute;
                    Mapping.Tables.Add(mappingAttribute);
                    Mapping.TableAliases[mappingAttribute] = type.Name;
                    Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
                }
                else
                    Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
            }
            else
            {
                mappingAttribute = Dictionaries.TypeMappingCache[type].Table;
                Mapping.TypeTableMapping[type] = mappingAttribute;
                Mapping.Tables.Add(mappingAttribute);
                Mapping.TableAliases[mappingAttribute] = type.Name;
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}' from cache");
            }

            // Get the table mapping for all child objects
            type.GetProperties().ForEach(prop => MapPropertyTables(prop));
        }

        private void MapPropertyTables(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            TableMappingAttribute mappingAttribute;

            // If the property is a generic list, then it fits the profile of a child object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = propertyType.GetGenericArguments()[0];

                if (!Dictionaries.TypeMappingCache.ContainsKey(genericArgumentType))
                {
                    if (TryGetTableMappingAttribute(genericArgumentType, out mappingAttribute))
                    {
                        Mapping.TypeTableMapping[genericArgumentType] = mappingAttribute;
                        Mapping.Tables.Add(mappingAttribute);
                        Mapping.TableAliases[mappingAttribute] = genericArgumentType.Name;
                    }
                }
                else
                {
                    mappingAttribute = Dictionaries.TypeMappingCache[genericArgumentType].Table;
                    Mapping.TypeTableMapping[genericArgumentType] = mappingAttribute;
                    Mapping.Tables.Add(mappingAttribute);
                    Mapping.TableAliases[mappingAttribute] = genericArgumentType.Name;
                }

                propertyType.GetProperties().ForEach(prop => MapPropertyTables(prop));
            }
        }

        private void MapColumns()
        {
            List<ColumnMappingAttribute> columnAttributes;

            if (!Dictionaries.TypeMappingCache.ContainsKey(BaseType))
            {
                if (TryGetColumnMappingAttributes(BaseType, out columnAttributes))
                {
                    Mapping.TypeColumnMapping[BaseType] = columnAttributes;
                    Mapping.Columns.AddRange(columnAttributes);

                    foreach (var attribute in columnAttributes)
                    {
                        Mapping.ColumnAliases[attribute] = $"{BaseType.Name}.{attribute.ColumnName}";
                        Mapping.ColumnPropertyNames[attribute] = attribute.GetPropertyName(BaseType);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{BaseType.Name}'");
                }
                else
                    Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{BaseType.Name}'");
            }
            else
            {
                columnAttributes = Dictionaries.TypeMappingCache[BaseType].Columns;

                Mapping.TypeColumnMapping[BaseType] = columnAttributes;
                Mapping.Columns.AddRange(columnAttributes);

                foreach (var attribute in columnAttributes)
                {
                    Mapping.ColumnAliases[attribute] = $"{BaseType.Name}.{attribute.ColumnName}";
                    Mapping.ColumnPropertyNames[attribute] = attribute.GetPropertyName(BaseType);
                }

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{BaseType.Name}' from cache");
            }

            BaseType.GetProperties().ForEach(prop => MapPropertyColumns(prop));
        }

        private void MapPropertyColumns(PropertyInfo property)
        {
            Type type = property.PropertyType;
            List<ColumnMappingAttribute> columnAttributes;

            // If the property is a generic list, then it fits the profile of a child object
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgumentType = type.GetGenericArguments()[0];

                if (!Dictionaries.TypeMappingCache.ContainsKey(genericArgumentType))
                {
                    if (TryGetColumnMappingAttributes(genericArgumentType, out columnAttributes))
                    {
                        Mapping.TypeColumnMapping[genericArgumentType] = columnAttributes;
                        Mapping.Columns.AddRange(columnAttributes);

                        foreach (var attribute in columnAttributes)
                        {
                            Mapping.ColumnAliases[attribute] = $"{genericArgumentType.Name}.{attribute.ColumnName}";
                            Mapping.ColumnPropertyNames[attribute] = attribute.GetPropertyName(genericArgumentType);
                        }

                        Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{genericArgumentType.Name}'");
                    }
                    else
                        Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{genericArgumentType.Name}'");
                }
                else
                {
                    columnAttributes = Dictionaries.TypeMappingCache[genericArgumentType].Columns;

                    Mapping.TypeColumnMapping[genericArgumentType] = columnAttributes;
                    Mapping.Columns.AddRange(columnAttributes);

                    foreach (var attribute in columnAttributes)
                    {
                        Mapping.ColumnAliases[attribute] = $"{genericArgumentType.Name}.{attribute.ColumnName}";
                        Mapping.ColumnPropertyNames[attribute] = attribute.GetPropertyName(genericArgumentType);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
                }

                genericArgumentType.GetProperties().ForEach(prop => MapPropertyColumns(prop));
            }
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
