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
        #region Members

        private Dictionary<TableMappingAttribute, string> Joins = new Dictionary<TableMappingAttribute, string>();

        #endregion

        #region Constructors

        public ReadQueryBuilder()
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
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine();
            sqlBuilder.Append("select ");

            for (int i = 0; i < Columns.Count; i++)
                sqlBuilder.Append(string.Concat(ColumnAliases[Columns[i]], i == Columns.Count - 1 ? " " : ", "));

            sqlBuilder.AppendLine();

            for (int i = 0; i < Tables.Count; i++)
                if (i == 0)
                    sqlBuilder.AppendLine($"from {Tables[i].TableName} as {TableAliases[Tables[i]]} ");
                else
                    sqlBuilder.AppendLine($"{Joins[Tables[i]]} ");

            bool first = true;
            for (int i = 0; i < Columns.Count; i++)
                if (Restrictions.ContainsKey(Columns[i]))
                {
                    sqlBuilder.AppendLine($"{(first ? "where" : "and")} {Restrictions[Columns[i]]}");
                    first = false;
                }

            string sql = sqlBuilder.ToString();

            Logger.Info(MethodBase.GetCurrentMethod(), "Generated SQL: " + sql);

            return sql;
        }

        public IQueryBuilder<TBase> AddJoin<T1,T2>()
        {
            Type type = typeof(T1);
            Type joinType = typeof(T2);
            TableMappingAttribute table1Attribute;
            TableMappingAttribute table2Attribute;
            ColumnMappingAttribute column1Attribute;
            ColumnMappingAttribute column2Attribute;
            List<ColumnMappingAttribute> column1Attributes;
            List<ColumnMappingAttribute> column2Attributes;

            if (!TryGetTableMappingAttribute(type, out table1Attribute) || !TryGetTableMappingAttribute(joinType, out table2Attribute) || 
                !TryGetColumnMappingAttributes(type, out column1Attributes) || !TryGetColumnMappingAttributes(joinType, out column2Attributes))
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Could not join class '{joinType.Name}' to class '{type.Name}' due to invalid mapping attributes");
                return this;
            }

            column1Attribute = column1Attributes.Find(x => x.KeyType == KeyTypes.ForeignKey && x.ForeignKeyMapping == table2Attribute.TableName);
            column2Attribute = column2Attributes.Find(x => x.KeyType == KeyTypes.ForeignKey && x.ForeignKeyMapping == table1Attribute.TableName);

            if (column1Attribute == null || column2Attribute == null)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find foreign key relationship between class '{type.Name}' and class '{joinType.Name}'");
                return this;
            }

            GetTable(type);
            GetColumns(type);

            // Store the SQL for the join clause against the table attribute for T1 (the class representing the table that is being joined)
            Joins[table1Attribute] = $"inner join {table1Attribute.TableName} as {TableAliases[table1Attribute]} on {ColumnAliases[column1Attribute]} = {ColumnAliases[column2Attribute]}";

            return this;
        }

        #endregion
    }
}