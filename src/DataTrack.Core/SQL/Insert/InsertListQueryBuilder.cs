using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.Insert
{
    public class InsertListQueryBuilder<TBase> : QueryBuilder<TBase>
    {

        #region Members

        public List<TBase> Items { get; private set; }

        #endregion

        #region Constructors

        public InsertListQueryBuilder(List<TBase> items, int parameterIndex = 1)
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Create;

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

            Items = items;
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Items);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder childSqlBuilder = new StringBuilder();
            StringBuilder insertBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();

            for (int i = 0; i < Tables.Count; i++)
            {
                int maxParameterCount = Columns.Select(c => Parameters[c].Count).Max();

                if (i == 0)
                {
                    for (int j = 1; j <= maxParameterCount; j++)
                    {
                        for (int k = 1; k <= Columns.Count; k++)
                        {
                            if (j == 1)
                            {
                                if (k == Columns.Count)
                                {
                                    insertBuilder.Append(Columns[k - 1].ColumnName + ")");
                                }
                                else
                                {
                                    insertBuilder.Append(Columns[k - 1].ColumnName + ", ");
                                }
                            }

                            if (k == 1)
                            {
                                valuesBuilder.Append("(" + Parameters[Columns[k - 1]][j - 1].Handle + ", ");
                            }
                            else if (k == Columns.Count)
                            {
                                valuesBuilder.Append(Parameters[Columns[k - 1]][j - 1].Handle + ")" + (j == maxParameterCount ? "" : ","));
                            }
                            else
                            {
                                valuesBuilder.Append(Parameters[Columns[k - 1]][j - 1].Handle + ", ");
                            }
                        }
                    }
                }
            }


            sqlBuilder.AppendLine();
            sqlBuilder.Append("insert into " + Tables[0].TableName + " (");
            sqlBuilder.AppendLine(insertBuilder.ToString());
            sqlBuilder.Append("values ");
            sqlBuilder.AppendLine(valuesBuilder.ToString());

            // For insert statements return the number of rows affected
            SelectRowCount(ref sqlBuilder);

            sqlBuilder.Append(childSqlBuilder.ToString());

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

        #endregion Methods
    }
}
