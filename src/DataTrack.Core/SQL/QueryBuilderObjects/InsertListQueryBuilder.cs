using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
            SQLBuilder sqlBuilder = new SQLBuilder(Query.Parameters);

            sqlBuilder.AppendLine();

            for (int i = 0; i < Query.Tables.Count; i++)
            {
                if (i == 0)
                {
                    sqlBuilder.BuildInsertStatement(Query.Columns, Query.Tables[i]);
                    sqlBuilder.BuildValuesStatement(Query.Columns, Query.Tables[i]);
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
