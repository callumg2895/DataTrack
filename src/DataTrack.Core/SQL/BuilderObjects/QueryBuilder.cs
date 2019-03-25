using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public abstract class QueryBuilder<TBase> : IQueryBuilder<TBase> where TBase : Entity, new()
    {
        #region Members

        private protected Type BaseType { get => typeof(TBase); }
        internal Query<TBase> Query = new Query<TBase>();

        // An integer which ensures that all parameter names are unique between queries and subqueries
        private protected int CurrentParameterIndex;

        #endregion

        #region Methods

        private protected void Init(CRUDOperationTypes opType)
        {
            // Define the operation type used for transactions
            Query.OperationType = opType;

            // Fetch the table and column names for TBase
            Query.Mapping = new Mapping<TBase>();

            // Check for valid Table/Columns
            if (Query.Mapping.Tables.Count == 0 || Query.Mapping.Tables.Any(t => t.Columns.Count == 0))
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }
        }

        private protected bool TryGetForeignKeyColumnForType(Type type, string table, out Column? typeFKColumn)
        {
            Table typeTable = Dictionaries.TypeMappingCache[type];

            foreach (Column column in Query.Mapping.TypeTableMapping[type].Columns)
                if (column.IsForeignKey() && column.Table.Name == typeTable.Name && column.ForeignKeyTableMapping == table)
                {
                    typeFKColumn = column;
                    return true;
                }

            typeFKColumn = null;
            return false;
        }

        private protected void UpdateParameters(TBase item)
        {
            Query.Mapping.Tables.ForEach(t => t.Columns.ForEach(
                column =>
                {
                    // For each column in the Query, find the value of the property which is decorated by that column attribute
                    // Then update the dictionary of parameters with this value.

                    string handle = $"@{t.Name}_{column.Name}_{CurrentParameterIndex}";

                    if (column.TryGetPropertyName(BaseType, out string? propertyName))
                    {
                        object propertyValue = item.GetPropertyValue(propertyName);

                        if (propertyValue == null || (column.IsPrimaryKey() && (int)propertyValue == 0))
                            return;

                        Query.AddParameter(column, new Parameter(handle, propertyValue));
                    }
                }));

            CurrentParameterIndex++;
        }

        private protected void UpdateParameters(List<TBase> items)
            => items.ForEach(item =>
            {
                UpdateParameters(item);
            });


        private protected void AddPrimaryKeyRestriction(TBase item)
        {
            Column primaryKeyColumn = Query.Mapping.TypeTableMapping[BaseType].GetPrimaryKeyColumn(); 

            // Find the name and value of the primary key property in the 'item' object
            if (primaryKeyColumn.TryGetPropertyName(BaseType, out string? primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumn.Name, RestrictionTypes.EqualTo, primaryKeyValue);
            }
        }

        private protected void AddForeignKeyRestriction(int value, string table)
        {
            // Find the name and value of the primary key property in the 'item' object
            Column foreignKeyColumn;
            string? primaryKeyColumnPropertyname;

            if (TryGetForeignKeyColumnForType(BaseType, table, out foreignKeyColumn) && 
                foreignKeyColumn.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                this.AddRestriction<int>(foreignKeyColumn.Name, RestrictionTypes.EqualTo, value);
            }
        }

        private protected void SelectRowCount(ref SQLBuilder<TBase> sqlBuilder) => sqlBuilder.AppendLine("select @@rowcount as affected_rows");

        abstract public Query<TBase> GetQuery();

        public virtual QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value)
        {
            Table table = Query.Mapping.TypeTableMapping[BaseType];
            Column column= table.Columns.Find(x => x.Name == property);
            StringBuilder restrictionBuilder = new StringBuilder();

            if (column == null)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find property '{property}' in table '{table.Name}'");
                return this;
            }

            if (!table.Columns.Contains(column))
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"'{property}' is not a property of '{table.Name}'");
                return this;
            }

            // Generate a handle for SQL parameter. This is in the form @[TableName]_[ColumnName]
            //      eg: @books_author
            string handle = $"@{table.Name}_{column.Name}_{CurrentParameterIndex}";

            // Generate the SQL for the restriction clause
            switch (rType)
            {
                case RestrictionTypes.NotIn:
                case RestrictionTypes.In:
                    restrictionBuilder.Append(column.Alias + " ");
                    restrictionBuilder.Append(rType.ToSqlString() + " (");
                    restrictionBuilder.Append(handle);
                    restrictionBuilder.Append(")");
                    break;

                case RestrictionTypes.LessThan:
                case RestrictionTypes.MoreThan:

                    if (Dictionaries.SQLDataTypes[value?.GetType()] == SqlDbType.VarChar)
                    {
                        Logger.Error(MethodBase.GetCurrentMethod(), $"Cannot apply '{rType.ToSqlString()}' operator to values of type VarChar");
                        return this;
                    }
                    else
                    {
                        restrictionBuilder.Append(column.Alias + " ");
                        restrictionBuilder.Append(rType.ToSqlString() + " ");
                        restrictionBuilder.Append(handle);
                        break;
                    }

                case RestrictionTypes.EqualTo:
                case RestrictionTypes.NotEqualTo:
                default:
                    restrictionBuilder.Append(column.Alias + " ");
                    restrictionBuilder.Append(rType.ToSqlString() + " ");
                    restrictionBuilder.Append(handle);
                    break;
            }

            // Store the SQL for the restriction clause against the column attribute for the 
            // property, then store the value of the parameter against its handle if no error occurs.
            Query.Mapping.Restrictions[column] = restrictionBuilder.ToString();
            Query.AddParameter(column, new Parameter(handle, value));

            return this;
        }

        #endregion
    }
}
