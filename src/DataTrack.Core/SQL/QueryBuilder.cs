using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL
{
    public abstract class QueryBuilder<TBase> : IQueryBuilder<TBase>
    {
        #region Members

        private protected Type BaseType { get => typeof(TBase); }
        private protected Dictionary<TableMappingAttribute, string> TableAliases = new Dictionary<TableMappingAttribute, string>();
        private protected Dictionary<ColumnMappingAttribute, string> ColumnAliases = new Dictionary<ColumnMappingAttribute, string>();
        private protected Dictionary<ColumnMappingAttribute, string> Restrictions = new Dictionary<ColumnMappingAttribute, string>();
        private protected Dictionary<ColumnMappingAttribute, (string Handle, object Value)> Parameters = new Dictionary<ColumnMappingAttribute, (string Handle, object Value)>();

        public List<TableMappingAttribute> Tables { get; private set; } = new List<TableMappingAttribute>();
        public List<ColumnMappingAttribute> Columns { get; private set; } = new List<ColumnMappingAttribute>();

        #endregion

        #region Methods

        private protected void GetTable(Type type)
        {
            TableMappingAttribute mappingAttribute;
            if (TryGetTableMappingAttribute(type, out mappingAttribute))
            {
                Tables.Add(mappingAttribute);
                TableAliases[mappingAttribute] = type.Name;
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded table mapping for class '{type.Name}'");
            }
            else
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load table mapping for class '{type.Name}'");
        }

        private protected void GetColumns(Type type)
        {
            List<ColumnMappingAttribute> attributes;
            if (TryGetColumnMappingAttributes(type, out attributes))
            {
                Columns.AddRange(attributes);
                foreach (var attribute in attributes)
                    ColumnAliases[attribute] = $"{type.Name}.{attribute.ColumnName}";
                Logger.Info(MethodBase.GetCurrentMethod(), $"Loaded column mapping for class '{type.Name}'");
            }
            else
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{type.Name}'");
        }

        private protected bool TryGetTableMappingAttribute(Type type, out TableMappingAttribute mappingAttribute)
        {
            mappingAttribute = null;

            foreach (Attribute attribute in type.GetCustomAttributes())
                mappingAttribute = attribute as TableMappingAttribute;

            return mappingAttribute != null;
        }

        private protected bool TryGetColumnMappingAttributes(Type type, out List<ColumnMappingAttribute> attributes)
        {
            attributes = new List<ColumnMappingAttribute>();

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
            TableMappingAttribute typeTable;
            typePKColumn = null;

            if (TryGetTableMappingAttribute(type, out typeTable))
                foreach (ColumnMappingAttribute column in Columns)
                    if (column.KeyType == KeyTypes.PrimaryKey && column.TableName == typeTable.TableName)
                    {
                        typePKColumn = column;
                        return true;
                    }

            return false;
        }

        private protected void UpdateParameters(TBase item)
        {
            // For each column mapping attribute, find the value of the property which is decorated by that column attribute
            // Then update the dictionary of parameters with this value.
            foreach (ColumnMappingAttribute columnAttribute in Columns)
            {
                string handle = $"@{columnAttribute.TableName}_{columnAttribute.ColumnName}";
                string propertyName;

                if (columnAttribute.TryGetPropertyName(BaseType, out propertyName))
                    Parameters[columnAttribute] = (handle, item.GetPropertyValue(propertyName));
            }
        }

        private protected void AddPrimaryKeyRestriction(TBase item)
        {
            // Find the name and value of the primary key property in the 'item' object
            ColumnMappingAttribute primaryKeyColumnAttribute;
            string primaryKeyColumnPropertyname;

            if (TryGetPrimaryKeyColumnForType(typeof(TBase), out primaryKeyColumnAttribute) && primaryKeyColumnAttribute.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<TBase, object>(primaryKeyColumnAttribute.ColumnName, RestrictionTypes.EqualTo, primaryKeyValue);
            }
        }

        abstract public override string ToString();

        public virtual IQueryBuilder<TBase> AddRestriction<T, TProp>(string property, RestrictionTypes rType, TProp value)
        {
            Type type = typeof(T);
            TableMappingAttribute tableAttribute;
            List<ColumnMappingAttribute> columnAttributes;
            ColumnMappingAttribute columnAttribute;
            StringBuilder restrictionBuilder = new StringBuilder();

            if (!TryGetTableMappingAttribute(type, out tableAttribute) || !TryGetColumnMappingAttributes(type, out columnAttributes))
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load column mapping for class '{type.Name}'");
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
            string handle = $"@{columnAttribute.TableName}_{columnAttribute.ColumnName}";

            // Store the value of the parameter against its handle, then store the SQL for the restriction clause
            // against the column attribute for the property.
            Parameters[columnAttribute] = (handle, value);

            restrictionBuilder.Append(ColumnAliases[columnAttribute] + " ");
            restrictionBuilder.Append(rType.ToSqlString() + " ");
            restrictionBuilder.Append(handle);

            Restrictions[columnAttribute] = restrictionBuilder.ToString();

            return this;
        }

        public virtual List<(string Handle, object Value)> GetParameters()
        {
            List<(string Handle, object Value)> parameters = new List<(string Handle, object Value)>();

            foreach (ColumnMappingAttribute column in Columns)
                if (Parameters.ContainsKey(column))
                    parameters.Add(Parameters[column]);

            return parameters;
        }

        #endregion
    }
}
