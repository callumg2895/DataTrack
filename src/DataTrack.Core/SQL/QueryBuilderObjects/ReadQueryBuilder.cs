using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using System;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class ReadQueryBuilder<TBase> : QueryBuilder<TBase>
    {
        #region Members

        private int? ID;

        #endregion

        #region Constructors

        public ReadQueryBuilder() : this(null)
        {
        }

        public ReadQueryBuilder(int? id, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Read);

            this.ID = id;
            this.CurrentParameterIndex = parameterIndex;

            if (ID.HasValue)
                AddRestriction<int>("ID", RestrictionTypes.EqualTo, ID.Value);
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {
            SQLBuilder sqlBuilder = new SQLBuilder(Query.Parameters, TableAliases, ColumnAliases, Restrictions);

            sqlBuilder.AppendLine();
            sqlBuilder.BuildSelectStatement(Query.Columns);

            for (int i = 0; i < Query.Tables.Count; i++)
            {
                if (i == 0)
                    sqlBuilder.BuildFromStatement(Query.Columns, Query.Tables[0]);
                else
                {
                    dynamic queryBuilder = Activator.CreateInstance(typeof(ReadQueryBuilder<>).MakeGenericType(Query.TypeTableMapping[Query.Tables[i]]));

                    if (ID.HasValue)
                    {
                        // Make sure that only those child items with a foreign key matching the primary key of TBase are retrieved
                        MethodInfo addForeignKeyRestriction = queryBuilder.GetType().GetMethod("AddForeignKeyRestriction", BindingFlags.Instance | BindingFlags.NonPublic);
                        addForeignKeyRestriction.Invoke(queryBuilder, new object[] { ID, Query.Tables[0].TableName });

                        foreach (ColumnMappingAttribute column in queryBuilder.Query.Columns)
                        {
                            if (queryBuilder.Query.Parameters.ContainsKey(column))
                                Query.Parameters.TryAdd(column, queryBuilder.Query.Parameters[column]);

                            if (queryBuilder.Query.ColumnPropertyNames.ContainsKey(column))
                                Query.ColumnPropertyNames.TryAdd(column, queryBuilder.Query.ColumnPropertyNames[column]);

                            Query.Columns.Add(column);
                        }
                    }

                    sqlBuilder.Append(queryBuilder.GetQuery().QueryString);
                }
            }

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

        #endregion
    }
}