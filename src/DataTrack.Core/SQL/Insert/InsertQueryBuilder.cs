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

        #region Members

        private TBase Item;

        #endregion

        #region Constructors

        public InsertQueryBuilder(TBase item, int parameterIndex = 1)
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

            Item = item;
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Item);
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
                if (i == 0)
                    for (int j = 1; j <= Columns.Count; j++)
                        if (j == Columns.Count)
                        {
                            insertBuilder.Append(Columns[j - 1].ColumnName + ")");
                            valuesBuilder.Append(Parameters[Columns[j - 1]].Handle + ")");
                        }
                        else
                        {
                            insertBuilder.Append(Columns[j - 1].ColumnName + ", ");
                            valuesBuilder.Append(Parameters[Columns[j - 1]].Handle + ", ");
                        }
                else
                {              
                    dynamic childItems = Tables[0].GetChildPropertyValues(Item, Tables[i].TableName) ?? new List<object>();

                    foreach( object item in childItems)
                    {
                        CurrentParameterIndex++;
                        dynamic queryBuilder = Activator.CreateInstance(typeof(InsertQueryBuilder<>).MakeGenericType(TableTypeMapping[Tables[i]]), item, CurrentParameterIndex);

                        foreach (ColumnMappingAttribute column in queryBuilder.Columns)
                        {
                            if (queryBuilder.Parameters.ContainsKey(column))
                                Parameters.TryAdd(column, queryBuilder.Parameters[column]);
                            if (queryBuilder.ColumnPropertyNames.ContainsKey(column))
                                ColumnPropertyNames.TryAdd(column, queryBuilder.ColumnPropertyNames[column]);
                            Columns.Add(column);
                        }

                        childSqlBuilder.Append(queryBuilder.ToString());
                    }
                }
            }


            sqlBuilder.AppendLine();
            sqlBuilder.Append("insert into " + Tables[0].TableName + " (");
            sqlBuilder.AppendLine(insertBuilder.ToString());
            sqlBuilder.Append("values (");
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
