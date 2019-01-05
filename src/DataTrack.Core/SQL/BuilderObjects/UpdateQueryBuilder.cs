﻿using DataTrack.Core.Enums;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using System.Reflection;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class UpdateQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {

        public UpdateQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Update);

            CurrentParameterIndex = parameterIndex;
            UpdateParameters(item);
            AddPrimaryKeyRestriction(item);
        }

        public override Query<TBase> GetQuery()
        {
            SQLBuilder<TBase> sqlBuilder = new SQLBuilder<TBase>(Query.Mapping);

            sqlBuilder.AppendLine();
            sqlBuilder.BuildUpdateStatement();

            // For update statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

    }
}