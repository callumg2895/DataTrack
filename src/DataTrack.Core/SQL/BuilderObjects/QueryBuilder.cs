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

        private protected Type BaseType;
        internal Query<TBase> Query { get; set; }

        // An integer which ensures that all parameter names are unique between queries and subqueries
        private protected int CurrentParameterIndex;

        #endregion

        #region Methods

        private protected void Init(CRUDOperationTypes opType)
        {
            BaseType = typeof(TBase);

            // Define the operation type used for transactions
            Query = new Query<TBase>(opType);
        }

        private protected void UpdateParameters(TBase item)
        {
            Query.Mapping.Tables.ForEach(t => t.Columns.ForEach(
                column =>
                {
                    // For each column in the Query, find the value of the property which is decorated by that column attribute
                    // Then update the dictionary of parameters with this value.
                    if (column.TryGetPropertyName(BaseType, out string? propertyName))
                    {
                        object propertyValue = item.GetPropertyValue(propertyName);

                        if (propertyValue == null || (column.IsPrimaryKey() && (int)propertyValue == 0))
                            return;

                        Query.AddParameter(column, new Parameter(column.GetParameterHandle(CurrentParameterIndex), propertyValue));
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
            Column foreignKeyColumn = Query.Mapping.TypeTableMapping[BaseType].GetForeignKeyColumn(table);
            this.AddRestriction<int>(foreignKeyColumn.Name, RestrictionTypes.EqualTo, value);    
        }

        private protected void SelectRowCount(ref SQLBuilder<TBase> sqlBuilder) => sqlBuilder.AppendLine("select @@rowcount as affected_rows");

        abstract public Query<TBase> GetQuery();

        public virtual QueryBuilder<TBase> AddRestriction<TProp>(string property, RestrictionTypes rType, TProp value)
        {
            Table table = Query.Mapping.TypeTableMapping[BaseType];
            Column column= table.Columns.Single(x => x.Name == property);
            string handle = column.GetParameterHandle(CurrentParameterIndex);
            Parameter parameter = new Parameter(handle, value);

            // Store the SQL for the restriction clause against the column attribute for the 
            // property, then store the value of the parameter against its handle if no error occurs.
            Query.Mapping.Restrictions[column] = new Restriction(column, parameter, rType);
            Query.AddParameter(column, parameter);

            return this;
        }

        #endregion
    }
}
