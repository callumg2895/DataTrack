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

        public UpdateQueryBuilder(TBase item, int parameterIndex = 1)
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Update;

            // Fetch the table and column names for TBase
            GetTable();
            GetColumns();

            if (!Dictionaries.MappingCache.ContainsKey(typeof(TBase)))
            {
                Dictionaries.MappingCache[typeof(TBase)] = (TypeTableMapping[typeof(TBase)], TypeColumnMapping[typeof(TBase)]);
            }

            // Check for valid Table/Columns
            if (Tables.Count < 0 || Columns.Count < 0)
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }

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
