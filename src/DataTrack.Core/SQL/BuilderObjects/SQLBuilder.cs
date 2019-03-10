using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.BuilderObjects
{
    public class SQLBuilder<TBase> where TBase : new()
    {
        #region Members

        private Type BaseType = typeof(TBase);
        private Mapping<TBase> Mapping;
        private StringBuilder sql;

        #endregion

        #region Constructors

        public SQLBuilder(Mapping<TBase> mapping)
        {
            Mapping = mapping;
            sql = new StringBuilder();
        }

        #endregion

        #region Methods

        public void CreateStagingTable(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            sql.AppendLine($"create table {table.StagingTableName}");
            sql.AppendLine("(");

            for (int i = 0; i < columns.Count; i++)
            {
                ColumnMappingAttribute column = columns[i];
                SqlDbType sqlDbType = column.GetSqlDbType(BaseType);

                if (column.IsPrimaryKey())
                {
                    sql.AppendLine($"{column.ColumnName} {sqlDbType.ToSqlString()} not null identity(1,1),");
                    sql.Append($"primary key ({column.ColumnName})");
                }
                else
                {
                    sql.Append($"{column.ColumnName} {sqlDbType.ToSqlString()} not null");
                }

                sql.AppendLine(i == columns.Count - 1 ? "" : ",");
            }

            sql.AppendLine(")");
        }

        public void BuildInsertStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0) return;

            columns = columns.Where(c => !c.IsPrimaryKey()).ToList();

            sql.Append("insert into " + table.TableName + " (");

            sql.Append(columns[0].ColumnName);

            for (int i = 1; i < columns.Count; i++)
            {
                sql.Append(", " + columns[i].ColumnName);
            }

            sql.AppendLine(")");
        }

        public void BuildUpdateStatement()
        {
            StringBuilder setBuilder = new StringBuilder();
            StringBuilder restrictionBuilder = new StringBuilder();

            TableMappingAttribute table = Mapping.TypeTableMapping[BaseType];
            List<ColumnMappingAttribute> columns = Mapping.TypeColumnMapping[BaseType].Where(c => !c.IsPrimaryKey()).ToList(); 

            int processedRestrictions = 0;
            int totalColumns = columns.Count;

            sql.AppendLine($"update {Mapping.TableAliases[table]}");
            sql.Append("set ");

            for (int i = 0; i < totalColumns; i++)
            {
                setBuilder.Append(Mapping.TableAliases[table]);
                setBuilder.Append(".");
                setBuilder.Append(columns[i].ColumnName);
                setBuilder.Append(" = ");
                setBuilder.Append(Mapping.Parameters[columns[i]][0].Handle);
                setBuilder.AppendLine(i == totalColumns - 1
                    ? ""
                    : ",");
            }

            restrictionBuilder.AppendLine($"from {table.TableName} {Mapping.TableAliases[table]}");

            foreach (ColumnMappingAttribute column in columns)
            {
                if (Mapping.Restrictions.ContainsKey(column))
                {
                    if (processedRestrictions++ == 0)
                    {

                        restrictionBuilder.Append("where ");
                    }
                    else
                    {
                        restrictionBuilder.Append("and ");
                    }

                    restrictionBuilder.AppendLine(Mapping.Restrictions[column]);
                }
            }


            sql.Append(setBuilder.ToString());
            sql.Append(restrictionBuilder.ToString());
        }

        public void BuildValuesStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0) return;

            columns = columns.Where(c => !c.IsPrimaryKey() && Mapping.Parameters.Keys.Contains(c)).ToList();

            // Assert that all colums for a given table have the same number of parameters
            int paramCount = columns.Where(c => Mapping.Parameters.Keys.Contains(c)).Select(c => Mapping.Parameters[c].Count).Max();

            sql.Append("values ");

            // For each set of parameters, we create a seperate set of values:
            // eg: values (set 1), (set 2), (set 3)
            for (int j = 0; j < paramCount; j++)
            {
                sql.Append("(");
                sql.Append(Mapping.Parameters[columns[0]][j].Handle);

                for (int i = 1; i < columns.Count; i++)
                {
                    sql.Append(", " + Mapping.Parameters[columns[i]][j].Handle);
                }

                sql.Append(")");

                if (j < paramCount - 1)
                    sql.Append(",");
            }

            sql.AppendLine();
        }

        public void BuildSelectStatement()
        {
            foreach (TableMappingAttribute table in Mapping.Tables)
            {
                List<ColumnMappingAttribute> columns = Dictionaries.TableMappingCache[table];
                int RestrictionCount = 0;

                sql.AppendLine();
                sql.Append("select ");
                sql.Append(Mapping.ColumnAliases[columns[0]]);

                for (int i = 1; i < columns.Count; i++)
                {
                    sql.Append(", ");
                    sql.Append(Mapping.ColumnAliases[columns[i]]);
                }

                sql.AppendLine();

                sql.Append("into ")
                   .Append(table.StagingTableName);

                sql.Append(" from ")
                   .Append(table.TableName)
                   .Append(" as ")
                   .AppendLine(Mapping.TableAliases[table]);        

                if (Mapping.TypeTableMapping[table] != BaseType && columns.Where(c => c.IsForeignKey()).Count() > 0)
                {
                    List<ColumnMappingAttribute> foreignKeyColumns = columns.Where(c => c.IsForeignKey()).ToList();

                    foreach (ColumnMappingAttribute column in foreignKeyColumns)
                    {
                        TableMappingAttribute foreignTable = Mapping.Tables.Where(t => t.TableName == column.ForeignKeyTableMapping).First();
                        ColumnMappingAttribute foreignColumn = Dictionaries.TableMappingCache[foreignTable].Where(c => c.IsPrimaryKey()).First();

                        sql.Append($"{GetRestrictionKeyWord(RestrictionCount++)} ")
                           .AppendLine($"{Mapping.ColumnAliases[column]} in (select {foreignColumn.ColumnName} from {foreignTable.StagingTableName})");

                    }             
                }

                foreach (ColumnMappingAttribute column in columns)
                {
                    if (Mapping.Restrictions.ContainsKey(column))
                    {
                        sql.Append($"{GetRestrictionKeyWord(RestrictionCount++)} ")
                           .AppendLine(Mapping.Restrictions[column]);
                    }
                }

                sql.AppendLine();
                sql.AppendLine($"select * from {table.StagingTableName}");
            }
        }

        private string GetRestrictionKeyWord(int restrictionCount)
        {
            return restrictionCount > 0 ? "and" : "where";
        }

        public void Append(string text) => sql.Append(text);

        public void AppendLine() => sql.AppendLine();

        public void AppendLine(string text) => sql.AppendLine(text);

        public override string ToString() => sql.ToString();

        #endregion
    }
}
