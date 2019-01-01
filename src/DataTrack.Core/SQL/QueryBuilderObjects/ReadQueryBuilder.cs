using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using System;
using System.Linq;
using System.Reflection;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class ReadQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
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
                AddRestriction<int>("id", RestrictionTypes.EqualTo, ID.Value);
        }

        #endregion

        #region Methods

        public override Query<TBase> GetQuery()
        {
            SQLBuilder sqlBuilder = new SQLBuilder(Query.Mapping.Parameters, Query.Mapping.TableAliases, Query.Mapping.ColumnAliases, Restrictions);

            sqlBuilder.AppendLine();
            sqlBuilder.BuildSelectStatement(Query.Mapping.Columns.Where(c => Query.Mapping.TypeColumnMapping[BaseType].Contains(c)).ToList());

            for (int i = 0; i < Query.Mapping.Tables.Count; i++)
            {
                if (i == 0)
                    sqlBuilder.BuildFromStatement(Query.Mapping.Columns.Where(c => Query.Mapping.TypeColumnMapping[BaseType].Contains(c)).ToList(), Query.Mapping.Tables[0]);
                else
                {
                    dynamic queryBuilder = Activator.CreateInstance(typeof(ReadQueryBuilder<>).MakeGenericType(Query.Mapping.TypeTableMapping[Query.Mapping.Tables[i]]));

                    if (ID.HasValue)
                    {
                        // Make sure that only those child items with a foreign key matching the primary key of TBase are retrieved
                        MethodInfo addForeignKeyRestriction = queryBuilder.GetType().GetMethod("AddForeignKeyRestriction", BindingFlags.Instance | BindingFlags.NonPublic);
                        addForeignKeyRestriction.Invoke(queryBuilder, new object[] { ID, Query.Mapping.Tables[0].TableName });

                        foreach (ColumnMappingAttribute column in queryBuilder.Query.Columns)
                        {
                            if (queryBuilder.Query.Parameters.ContainsKey(column))
                                Query.Mapping.Parameters.TryAdd(column, queryBuilder.Query.Parameters[column]);

                            if (queryBuilder.Query.ColumnPropertyNames.ContainsKey(column))
                                Query.Mapping.ColumnPropertyNames.TryAdd(column, queryBuilder.Query.ColumnPropertyNames[column]);

                            Query.Mapping.Columns.Add(column);
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