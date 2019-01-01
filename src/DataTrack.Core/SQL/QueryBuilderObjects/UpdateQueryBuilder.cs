using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryBuilderObjects
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
            SQLBuilder sqlBuilder = new SQLBuilder(Query.Parameters, Query.TableAliases, Query.ColumnAliases, Restrictions);

            sqlBuilder.AppendLine();
            sqlBuilder.BuildUpdateStatement(Query.Columns, Query.Tables[0]);

            // For update statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

    }
}
