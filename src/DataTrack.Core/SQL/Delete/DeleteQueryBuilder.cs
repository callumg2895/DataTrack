using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.Delete
{
    public class DeleteQueryBuilder<TBase> : QueryBuilder<TBase>
    {

        #region Constructors

        public DeleteQueryBuilder(TBase item)
        {
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

            AddPrimaryKeyRestriction(item);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Parameters.Count >= 1)
            {
                StringBuilder sqlBuilder = new StringBuilder();
                StringBuilder restrictionsBuilder = new StringBuilder();

                for (int i = 0; i < Columns.Count; i++)
                {
                    if (Restrictions.ContainsKey(Columns[i]))
                    {
                        restrictionsBuilder.Append(restrictionsBuilder.Length == 0 ? "where " : "and ");
                        restrictionsBuilder.AppendLine(Restrictions[Columns[i]]);
                    }
                }

                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine($"delete * from {Tables[0].TableName} as {TableAliases[Tables[0]]}");
                sqlBuilder.Append(restrictionsBuilder.ToString());

                string sql = sqlBuilder.ToString();

                Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

                return sql;
            }

            return string.Empty;
        }

        #endregion
    }
}
