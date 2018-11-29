using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class InsertQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {

        #region Members

        public TBase Item { get; private set; }

        #endregion

        #region Constructors

        public InsertQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Create);

            Item = item;
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Item);
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {
            SQLBuilder sqlBuilder = new SQLBuilder(Query.Parameters);
            StringBuilder childSqlBuilder = new StringBuilder();

            sqlBuilder.AppendLine();

            for (int i = 0; i < Query.Tables.Count; i++)
            {
                int maxParameterCount = Query.Columns.Select(c => Query.Parameters[c].Count).Max();

                // The case when i == 0 corresponds to the table for the TBase object
                if (i == 0)
                {
                    sqlBuilder.BuildInsertStatement(Query.Columns, Query.Tables[i]);
                    sqlBuilder.BuildValuesStatement(Query.Columns, Query.Tables[i]);

                    // For insert statements return the number of rows affected
                    SelectRowCount(ref sqlBuilder);
                }
                else
                {
                    dynamic childItems = Query.Tables[0].GetChildPropertyValues(Item, Query.Tables[i].TableName) ?? new List<object>();

                    if (childItems.Count > 0)
                    {
                        CurrentParameterIndex++;
                        dynamic queryBuilder = Activator.CreateInstance(typeof(InsertListQueryBuilder<>).MakeGenericType(Query.TypeTableMapping[Query.Tables[i]]), childItems, CurrentParameterIndex);

                        foreach (ColumnMappingAttribute column in queryBuilder.Query.Columns)
                        {
                            if (queryBuilder.Query.Parameters.ContainsKey(column))
                            {
                                if (Query.Parameters.ContainsKey(column))
                                    Query.Parameters[column].AddRange(queryBuilder.Query.Parameters[column]);
                                else
                                {
                                    Query.Parameters[column] = new List<(string Handle, object Value)>();
                                    Query.Parameters[column].AddRange(queryBuilder.Query.Parameters[column]);
                                }
                            }

                            if (queryBuilder.Query.ColumnPropertyNames.ContainsKey(column))
                                Query.ColumnPropertyNames.TryAdd(column, queryBuilder.Query.ColumnPropertyNames[column]);

                            Query.Columns.Add(column);
                        }

                        sqlBuilder.Append(queryBuilder.GetQuery().QueryString);
                    }
                }
            }

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

        #endregion Methods
    }
}
