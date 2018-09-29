using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Sql.Read
{
    public class ReadQueryBuilder<TBase> : QueryBuilder<TBase>
    {
        #region Constructors

        public ReadQueryBuilder()
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Read;

            // Fetch the table and column names for TBase
            GetTable();
            GetColumns();

            // Check for valid Table/Columns
            if (Tables.Count < 0 || Columns.Count < 0)
            {
                string message = $"Mapping data for class '{BaseType.Name}' was incomplete/empty";
                Logger.Error(MethodBase.GetCurrentMethod(), message);
                throw new Exception(message);
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder childSqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.Append("select ");

            for (int i = 0; i < Columns.Count; i++)
                sqlBuilder.Append(string.Concat(ColumnAliases[Columns[i]], i == Columns.Count - 1 ? " " : ", "));

            sqlBuilder.AppendLine();

            for (int i = 0; i < Tables.Count; i++)
            {
                if (i == 0)
                    sqlBuilder.AppendLine($"from {Tables[0].TableName} as {TableAliases[Tables[0]]} ");
                else
                {
                    dynamic queryBuilder = Activator.CreateInstance(typeof(ReadQueryBuilder<>).MakeGenericType(TableTypeMapping[Tables[i]]));
                    childSqlBuilder.Append(queryBuilder.ToString());
                }
            }

            bool first = true;
            for (int i = 0; i < Columns.Count; i++)
                if (Restrictions.ContainsKey(Columns[i]))
                {
                    sqlBuilder.AppendLine($"{(first ? "where" : "and")} {Restrictions[Columns[i]]}");
                    first = false;
                }


            sqlBuilder.Append(childSqlBuilder.ToString());
            string sql = sqlBuilder.ToString();


            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

        #endregion
    }
}