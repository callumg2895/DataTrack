using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class InsertListQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {

        #region Members

        public List<TBase> Items { get; private set; }

        #endregion

        #region Constructors

        public InsertListQueryBuilder(List<TBase> items, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Create);

            Items = items;
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Items);
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {
            SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Query.Mapping);

            sqlBuilder.AppendLine();

            for (int i = 0; i < Query.Mapping.Tables.Count; i++)
            {
                if (i == 0)
                {
                    sqlBuilder.BuildInsertStatement(Query.Mapping.Columns, Query.Mapping.Tables[i]);
                    sqlBuilder.BuildValuesStatement(Query.Mapping.Columns, Query.Mapping.Tables[i]);
                }
            }

            // For insert statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

        #endregion Methods
    }
}
