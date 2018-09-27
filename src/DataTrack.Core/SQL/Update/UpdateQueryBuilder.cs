using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.Update
{
    public class UpdateQueryBuilder<TBase> : QueryBuilder<TBase>
    {

        public UpdateQueryBuilder(TBase item)
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Update;

            // Fetch the table and column names for TBase
            GetTable(BaseType);
            GetColumns(BaseType);

            // Check for valid Table/Columns
            if (Tables.Count < 0 || Columns.Count < 0)
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }

            UpdateParameters(item);
            AddPrimaryKeyRestriction(item);
        }

        public override string ToString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder setBuilder = new StringBuilder();
            StringBuilder restrictionsBuilder = new StringBuilder();

            for (int i = 0; i < Columns.Count; i++)
            {
                setBuilder.Append(Columns[i].ColumnName + " = " + Parameters[Columns[i]].Handle);
                setBuilder.AppendLine(i == Columns.Count - 1 ? "" : ",");

                if (Restrictions.ContainsKey(Columns[i]))
                {
                    restrictionsBuilder.Append(restrictionsBuilder.Length == 0 ? "where " : "and ");
                    restrictionsBuilder.AppendLine(Restrictions[Columns[i]]);
                }
            }

            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine($"update {Tables[0].TableName} as {TableAliases[Tables[0]]}");
            sqlBuilder.Append("set ");
            sqlBuilder.Append(setBuilder.ToString());
            sqlBuilder.Append(restrictionsBuilder.ToString());

            // For update statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

    }
}
