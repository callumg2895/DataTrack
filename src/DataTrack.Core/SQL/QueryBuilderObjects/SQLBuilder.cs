using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.QueryObjects;
using DataTrack.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
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
            List<ColumnMappingAttribute> columns = Mapping.TypeColumnMapping[BaseType]; 

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

                if (Mapping.Restrictions.ContainsKey(columns[i]))
                {
                    if (processedRestrictions++ == 0)
                    {
                        restrictionBuilder.AppendLine($"from {table.TableName} {Mapping.TableAliases[table]}");
                        restrictionBuilder.Append("where ");
                    }
                    else
                    {
                        restrictionBuilder.Append("and ");
                    }

                    restrictionBuilder.AppendLine(Mapping.Restrictions[columns[i]]);
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

                        sql.Append("where ")
                           .AppendLine($"{Mapping.ColumnAliases[column]} in (select {foreignColumn.ColumnName} from {foreignTable.StagingTableName})");

                    }
                        
                }

                sql.AppendLine($"select * from {table.StagingTableName}");
            }
        }

        public void Append(string text) => sql.Append(text);

        public void AppendLine() => sql.AppendLine();

        public void AppendLine(string text) => sql.AppendLine(text);

        public override string ToString() => sql.ToString();

        #endregion
    }
}
