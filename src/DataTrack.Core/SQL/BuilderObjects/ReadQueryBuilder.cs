using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Logging;
using DataTrack.Core.Util;
using System;
using System.Linq;
using System.Reflection;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class ReadQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {
        #region Members

        private int? ID;

        #endregion

        #region Constructors

        public ReadQueryBuilder() : this(null)
        {
        }

        public ReadQueryBuilder(int? id, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Read);

            this.ID = id;
            this.CurrentParameterIndex = parameterIndex;

            if (ID.HasValue)
                AddRestriction<int>("id", RestrictionTypes.EqualTo, ID.Value);
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {
            SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Query.Mapping);

            sqlBuilder.BuildSelectStatement();

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

        #endregion
    }
}