using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class DeleteQueryBuilder<TBase> : QueryBuilder<TBase>
    {
        #region Constructors

        public DeleteQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Delete);

            CurrentParameterIndex = parameterIndex;
            AddPrimaryKeyDeleteRestriction(item);
        }

        #endregion

        #region Methods

        private void AddPrimaryKeyDeleteRestriction(TBase item)
        {
            // Find the name and value of the primary key property in the 'item' object
            ColumnMappingAttribute primaryKeyColumnAttribute;
            string primaryKeyColumnPropertyname;

            if (TryGetPrimaryKeyColumnForType(typeof(TBase), out primaryKeyColumnAttribute) && primaryKeyColumnAttribute.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumnAttribute.ColumnName, RestrictionTypes.In, primaryKeyValue);
            }
        }

        public override string ToString()
        {
            if (Parameters.Count >= 1)
            {
                SQLBuilder sqlBuilder = new SQLBuilder(Parameters);
                StringBuilder restrictionsBuilder = new StringBuilder();

                for (int i = 0; i < Columns.Count; i++)
                {
                    if (Restrictions.ContainsKey(Columns[i]))
                    {
                        restrictionsBuilder.Append(restrictionsBuilder.Length == 0 ? "where " : "and ");
                        restrictionsBuilder.AppendLine(Restrictions[Columns[i]]);
                    }
                }

                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine($"delete {TableAliases[Tables[0]]} from {Tables[0].TableName} {TableAliases[Tables[0]]}");
                sqlBuilder.Append(restrictionsBuilder.ToString());

                string sql = sqlBuilder.ToString();

                Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

                return sql;
            }

            return string.Empty;
        }

        #endregion
    }
}
