﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using DataTrack.Core.Logging;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class DeleteQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : Entity, new()
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
            Column primaryKeyColumn = Query.Mapping.TypeTableMapping[BaseType].GetPrimaryKeyColumn();

            // Find the name and value of the primary key property in the 'item' object
            if (primaryKeyColumn.TryGetPropertyName(BaseType, out string? primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumn.Name, RestrictionTypes.In, primaryKeyValue);
            }
        }

        public override Query<TBase> GetQuery()
        {
            if (Query.Mapping.Parameters.Count >= 1)
            {
                SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Query.Mapping);
                StringBuilder restrictionsBuilder = new StringBuilder();

                for (int i = 0; i < Query.Mapping.Tables.Count; i++)
                {
                    for (int j = 0; j < Query.Mapping.Tables[i].Columns.Count; j++)
                        if (Query.Mapping.Restrictions.ContainsKey(Query.Mapping.Tables[i].Columns[j]))
                    {
                        restrictionsBuilder.Append(restrictionsBuilder.Length == 0 ? "where " : "and ");
                        restrictionsBuilder.AppendLine(Query.Mapping.Restrictions[Query.Mapping.Tables[i].Columns[j]]);
                    }
                }

                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine($"delete {Query.Mapping.Tables[0].Alias} from {Query.Mapping.Tables[0].Name} {Query.Mapping.Tables[0].Alias}");
                sqlBuilder.Append(restrictionsBuilder.ToString());

                // For insert statements return the number of rows affected
                SelectRowCount(ref sqlBuilder);

                string sql = sqlBuilder.ToString();

                Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

                Query.QueryString = sql;
            }
            else
            {
                Query.QueryString = string.Empty;
            }

            return Query;
        }

        #endregion
    }
}
