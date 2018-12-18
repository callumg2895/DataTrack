using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class InsertQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {

        #region Members

        private Mapping<TableMappingAttribute, DataTable> DataMap;

        public TBase Item { get; private set; }

        #endregion

        #region Constructors

        public InsertQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Create);

            Item = item;
            DataMap = new Mapping<TableMappingAttribute, DataTable>();
            CurrentParameterIndex = parameterIndex;

            UpdateParameters(Item);
        }

        #endregion

        #region Methods

        private void ConstructData()
        {
            // For inserts, we build a list of DataTables, where each 'table' in the list corresponds to the data for a table in the Query object
            Query.Tables.ForEach(table => BuildDataFor(table));       
        }

        private void BuildDataFor(TableMappingAttribute table)
        {
            // Currently all this method does is log the values contained within an object, which is mapped to the table parameter
            // These values will be used to build a DataTable for the current table

            if (Query.TypeTableMapping[table] == BaseType)
            {
                Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {Item.GetType().ToString()}");
                List<object> items = table.GetPropertyValues(Item);
                items.ForEach(item => Logger.Info(MethodBase.GetCurrentMethod(), item?.ToString() ?? "NULL"));
            }
            else
            {
                if (Query.Tables[0].GetChildPropertyValues(Item, table.TableName) != null)
                {

                    dynamic childItems = Activator.CreateInstance(typeof(List<>).MakeGenericType(Query.TypeTableMapping[table]));

                    foreach( var item in Query.Tables[0].GetChildPropertyValues(Item, table.TableName))
                    {
                        childItems.Add(item);
                    }
                    
                    foreach(var i in childItems)
                    {
                        Logger.Info(MethodBase.GetCurrentMethod(), $"Building DataTable for: {i.GetType().ToString()}");
                        foreach(var j in table.GetPropertyValues(i))
                        {
                            Logger.Info(MethodBase.GetCurrentMethod(), j?.ToString() ?? "NULL");
                        }
                    }
                }
            }
        }

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

            ConstructData();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            Query.QueryString = sql;

            return Query;
        }

        #endregion Methods
    }
}
