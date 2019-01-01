using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class DeleteQueryBuilder<TBase> : QueryBuilder<TBase> where TBase : new()
    {
        #region Constructors

        public DeleteQueryBuilder(TBase item, int parameterIndex = 1)
        {
            Init(CRUDOperationTypes.Delete);

            CurrentParameterIndex = parameterIndex;
            AddPrimaryKeyDeleteRestriction(item);
        }

        #endregion

        #region Methods

        private void AddPrimaryKeyDeleteRestriction(TBase item)
        {
            // Find the name and value of the primary key property in the 'item' object
            ColumnMappingAttribute primaryKeyColumnAttribute;
            string primaryKeyColumnPropertyname;

            if (TryGetPrimaryKeyColumnForType(typeof(TBase), out primaryKeyColumnAttribute) && primaryKeyColumnAttribute.TryGetPropertyName(BaseType, out primaryKeyColumnPropertyname))
            {
                var primaryKeyValue = item.GetPropertyValue(primaryKeyColumnPropertyname);
                this.AddRestriction<object>(primaryKeyColumnAttribute.ColumnName, RestrictionTypes.In, primaryKeyValue);
            }
        }

        public override Query<TBase> GetQuery()
        {
            if (Query.Parameters.Count >= 1)
            {
                SQLBuilder sqlBuilder = new SQLBuilder(Query.Parameters);
                StringBuilder restrictionsBuilder = new StringBuilder();

                for (int i = 0; i < Query.Columns.Count; i++)
                {
                    if (Restrictions.ContainsKey(Query.Columns[i]))
                    {
                        restrictionsBuilder.Append(restrictionsBuilder.Length == 0 ? "where " : "and ");
                        restrictionsBuilder.AppendLine(Restrictions[Query.Columns[i]]);
                    }
                }

                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine($"delete {Query.TableAliases[Query.Tables[0]]} from {Query.Tables[0].TableName} {Query.TableAliases[Query.Tables[0]]}");
                sqlBuilder.Append(restrictionsBuilder.ToString());

                // For insert statements return the number of rows affected
                SelectRowCount(ref sqlBuilder);

                string sql = sqlBuilder.ToString();

                Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

                Query.QueryString = sql;
            }
            else
            {
                Query.QueryString = string.Empty;
            }

            return Query;
        }

        #endregion
    }
}
