using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.Insert
{
    public class InsertQueryBuilder<TBase> : QueryBuilder<TBase>
    {
        #region Constructors

        public InsertQueryBuilder(TBase item)
        {
            // Define the operation type used for transactions
            OperationType = CRUDOperationTypes.Create;

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
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            StringBuilder insertBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();

            for (int i = 1; i <= Columns.Count; i++)
                if (i == Columns.Count)
                {
                    insertBuilder.Append(Columns[i - 1].ColumnName + ")");
                    valuesBuilder.Append(Parameters[Columns[i - 1]].Handle + ")");
                }
                else
                {
                    insertBuilder.Append(Columns[i - 1].ColumnName + ", ");
                    valuesBuilder.Append(Parameters[Columns[i - 1]].Handle + ", ");
                }  

            sqlBuilder.AppendLine();
            sqlBuilder.Append("insert into " + Tables[0].TableName + " (");
            sqlBuilder.AppendLine(insertBuilder.ToString());
            sqlBuilder.Append("values (");
            sqlBuilder.AppendLine(valuesBuilder.ToString());
            
            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

        #endregion Methods
    }
}
