using DataTrack.Core.SQL.QueryBuilders;
using DataTrack.Core.Enums;
using DataTrack.Core.Util;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryBuilders
{
    public class UpdateQueryBuilder<TBase> : QueryBuilder<TBase>
    {

        public UpdateQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Update);

            CurrentParameterIndex = parameterIndex;
            UpdateParameters(item);
            AddPrimaryKeyRestriction(item);
        }

        public override string ToString()
        {
            SQLBuilder sqlBuilder = new SQLBuilder(Parameters, TableAliases, ColumnAliases, Restrictions);

            sqlBuilder.AppendLine();
            sqlBuilder.BuildUpdateStatement(Columns, Tables[0]);

            // For update statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

    }
}
