using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public abstract class QueryBuilder<TBase> : IQueryBuilder<TBase>
    {
        #region Members

        private protected Type BaseType { get => typeof(TBase); }
        private protected Dictionary<TableMappingAttribute, string> TableAliases = new Dictionary<TableMappingAttribute, string>();
        private protected Dictionary<ColumnMappingAttribute, string> ColumnAliases = new Dictionary<ColumnMappingAttribute, string>();
        private protected Dictionary<ColumnMappingAttribute, string> Restrictions = new Dictionary<ColumnMappingAttribute, string>();
        public Query<TBase> Query { get; private protected set; } = new Query<TBase>();

        // An integer which ensures that all parameter names are unique between queries and subqueries
        private protected int CurrentParameterIndex;

        #endregion

        #region Methods

        private protected void Init(CRUDOperationTypes opType)
        {
            // Define the operation type used for transactions
            Query.OperationType = opType;

            // Fetch the table and column names for TBase
            GetTable();
            GetColumns();
            CacheMappingData();

            // Check for valid Table/Columns
            if (Query.Tables.Count < 0 || Query.Columns.Count < 0)
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }
        }

        private void GetTable()
        {
            Type type = typeof(TBase);
            TableMappingAttribute mappingAttribute;

            // Get the table mapping for TBase
            if (!Dictionaries.MappingCache.ContainsKey(type))
            {
                if (TryGetTableMappingAttribute(type, out mappingAttribute))
                {
                    Query.TypeTableMapping[type] = mappingAttribute;
                    Query.Tables.Add(mappingAttribute);
                    TableAliases[mappingAttribute] = type.Name;
                    Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
                }
                else
                    Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
            }
            else
            {
                mappingAttribute = Dictionaries.MappingCache[type].Table;
                Query.TypeTableMapping[type] = mappingAttribute;
                Query.Tables.Add(mappingAttribute);
                TableAliases[mappingAttribute] = type.Name;
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
            }

            // Get the table mapping for all child objects
            foreach (PropertyInfo property in type.GetProperties())
            {
                Type propertyType = property.PropertyType;

                // If the property is a generic list, then it fits the profile of a child object
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type genericArgumentType = propertyType.GetGenericArguments()[0];

                    if (!Dictionaries.MappingCache.ContainsKey(type))
                    {
                        if (TryGetTableMappingAttribute(genericArgumentType, out mappingAttribute))
                        {
                            Query.TypeTableMapping[genericArgumentType] = mappingAttribute;
                            Query.Tables.Add(mappingAttribute);
                        }
                    }
                    else
                    {
                        mappingAttribute = Dictionaries.MappingCache[genericArgumentType].Table;
                        Query.TypeTableMapping[genericArgumentType] = mappingAttribute;
                        Query.Tables.Add(mappingAttribute);
                    }

                }
            }
        }

        private void GetColumns()
        {
            Type type = typeof(TBase);
            List<ColumnMappingAttribute> columnAttributes;

            if (!Dictionaries.MappingCache.ContainsKey(type))
            {
                if (TryGetColumnMappingAttributes(type, out columnAttributes))
                {
                    Query.TypeColumnMapping[type] = columnAttributes;
                    Query.Columns.AddRange(columnAttributes);

                    foreach (var attribute in columnAttributes)
                    {
                        ColumnAliases[attribute] = $"{type.Name}.{attribute.ColumnName}";
                        Query.ColumnPropertyNames[attribute] = attribute.GetPropertyName(BaseType);
                    }

                    Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
                }
                else
                    Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{type.Name}'");
            }
            else
            {
                columnAttributes = Dictionaries.MappingCache[type].Columns;

                Query.TypeColumnMapping[type] = columnAttributes;
                Query.Columns.AddRange(columnAttributes);

                foreach (var attribute in columnAttributes)
                {
                    ColumnAliases[attribute] = $"{type.Name}.{attribute.ColumnName}";
                    Query.ColumnPropertyNames[attribute] = attribute.GetPropertyName(BaseType);
                }

                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
            }
        }

        private void CacheMappingData()
        {
            if (!Dictionaries.MappingCache.ContainsKey(BaseType))
            {
                Dictionaries.MappingCache[BaseType] = (Query.TypeTableMapping[BaseType], Query.TypeColumnMapping[BaseType]);
            }
        }

        private protected bool TryGetTableMappingAttribute(Type type, out TableMappingAttribute mappingAttribute)
        {
            mappingAttribute = null;

            // Check the dictionary first to save using reflection
            if (Query.TypeTableMapping.ContainsKey(type))
            {
                mappingAttribute = Query.TypeTableMapping[type];
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
            if (Query.TypeColumnMapping.ContainsKey(type))
            {
                attributes = Query.TypeColumnMapping[type];
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

        private protected bool TryGetPrimaryKeyColumnForType(Type type, out ColumnMappingAttribute typePKColumn)
        {
            typePKColumn = null;

            foreach (ColumnMappingAttribute column in Query.TypeColumnMapping[type])
                if (column.IsPrimaryKey() )
                {
                    typePKColumn = column;
                    return true;
                }

            return false;
        }

        private protected bool TryGetForeignKeyColumnForType(Type type, string table, out ColumnMappingAttribute typeFKColumn)
        {
            TableMappingAttribute typeTable;
            typeFKColumn = null;

            if (TryGetTableMappingAttribute(type, out typeTable))
                foreach (ColumnMappingAttribute column in Query.TypeColumnMapping[type])
                    if (column.IsForeignKey() && column.TableName == typeTable.TableName && column.ForeignKeyMapping == table)
                    {
                        typeFKColumn = column;
                        return true;
                    }

            return false;
        }

        private protected void UpdateParameters(TBase item)
        {
            // For each column mapping attribute, find the value of the property which is decorated by that column attribute
            // Then update the dictionary of parameters with this value.
            foreach (ColumnMappingAttribute columnAttribute in Query.Columns)
            {
                string handle = $"@{columnAttribute.TableName}_{columnAttribute.ColumnName}_{CurrentParameterIndex}";
                string propertyName;

                if (columnAttribute.TryGetPropertyName(BaseType, out propertyName))
                    AddParameter(columnAttribute, (handle, item.GetPropertyValue(propertyName)));
            }
        }

        private protected void UpdateParameters(List<TBase> items)
        {
            foreach(TBase item in items)
            {
                UpdateParameters(item);
                CurrentParameterIndex++;
            }
        }

        private protected void AddPrimaryKeyRestriction(TBase item)
        {
            // Find the name and value of the primary key property in the 'item' object
            ColumnMappingAttribute primaryKeyColumnAttribute;
            string primaryKeyColumnPropertyname;

            if (TryGetPrimaryKeyColumnForType(BaseType, out primaryKeyColumnAttribute) && primaryKeyColumnAttribute.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumnAttribute.ColumnName, RestrictionTypes.EqualTo, primaryKeyValue);
            }
        }

        private protected void AddForeignKeyRestriction(int value, string table)
        {
            // Find the name and value of the primary key property in the 'item' object
            ColumnMappingAttribute foreignKeyColumnAttribute;
            string primaryKeyColumnPropertyname;

            if (TryGetForeignKeyColumnForType(BaseType, table, out foreignKeyColumnAttribute) && foreignKeyColumnAttribute.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                this.AddRestriction<int>(foreignKeyColumnAttribute.ColumnName, RestrictionTypes.EqualTo, value);
            }
        }

        private protected void SelectRowCount(ref SQLBuilder sqlBuilder) => sqlBuilder.AppendLine("select @@rowcount as affected_rows");

        private protected void AddParameter(ColumnMappingAttribute column, (string Handle, object Value) parameter)
        {
            if (Query.Parameters.ContainsKey(column))
                Query.Parameters[column].Add(parameter);
            else
                Query.Parameters[column] = new List<(string Handle, object Value)>() { parameter };
        }

        abstract public override string ToString();

        public virtual QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value)
        {
            TableMappingAttribute tableAttribute;
            List<ColumnMappingAttribute> columnAttributes;
            ColumnMappingAttribute columnAttribute;
            StringBuilder restrictionBuilder = new StringBuilder();

            if (!TryGetTableMappingAttribute(BaseType, out tableAttribute) || !TryGetColumnMappingAttributes(BaseType, out columnAttributes))
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{BaseType.Name}'");
                return this;
            }

            columnAttribute = columnAttributes.Find(x => x.ColumnName == property);

            if (columnAttribute == null)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find property '{property}' in table '{tableAttribute.TableName}'");
                return this;
            }

            // Generate a handle for SQL parameter. This is in the form @[TableName]_[ColumnName]
            //      eg: @books_author
            string handle = $"@{columnAttribute.TableName}_{columnAttribute.ColumnName}_{CurrentParameterIndex}";

            // Generate the SQL for the restriction clause
            switch (rType)
            {
                case RestrictionTypes.NotIn:
                case RestrictionTypes.In:
                    restrictionBuilder.Append(ColumnAliases[columnAttribute] + " ");
                    restrictionBuilder.Append(rType.ToSqlString() + " (");
                    restrictionBuilder.Append(handle);
                    restrictionBuilder.Append(")");
                    break;

                case RestrictionTypes.LessThan:
                case RestrictionTypes.MoreThan:

                    if (Dictionaries.SQLDataTypes[value.GetType()] == SqlDbType.VarChar)
                    {
                        Logger.Error(MethodBase.GetCurrentMethod(), $"Cannot apply '{rType.ToSqlString()}' operator to values of type VarChar");
                        return this;
                    }
                    else
                    {
                        restrictionBuilder.Append(ColumnAliases[columnAttribute] + " ");
                        restrictionBuilder.Append(rType.ToSqlString() + " ");
                        restrictionBuilder.Append(handle);
                        break;
                    }

                case RestrictionTypes.EqualTo:
                case RestrictionTypes.NotEqualTo:
                default:
                    restrictionBuilder.Append(ColumnAliases[columnAttribute] + " ");
                    restrictionBuilder.Append(rType.ToSqlString() + " ");
                    restrictionBuilder.Append(handle);
                    break;
            }

            // Store the SQL for the restriction clause against the column attribute for the 
            // property, then store the value of the parameter against its handle if no error occurs.
            Restrictions[columnAttribute] = restrictionBuilder.ToString();
            AddParameter(columnAttribute, (handle, value));

            return this;
        }

        #endregion
    }
}
