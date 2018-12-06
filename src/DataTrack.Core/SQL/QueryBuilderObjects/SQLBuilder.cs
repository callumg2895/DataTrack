using DataTrack.Core.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.QueryBuilderObjects
{
    public class SQLBuilder
    {
        #region Members

        private Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters;
        private Dictionary<TableMappingAttribute, string> tableAliases;
        private Dictionary<ColumnMappingAttribute, string> columnAliases;
        private Dictionary<ColumnMappingAttribute, string> restrictions;

        private StringBuilder sql;

        #endregion

        #region Constructors

        public SQLBuilder(Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters)
            : this(parameters, null, null, null) { }

        public SQLBuilder(
            Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters,
            Dictionary<TableMappingAttribute, string> tableAliases,
            Dictionary<ColumnMappingAttribute, string> columnAliases,
            Dictionary<ColumnMappingAttribute, string> restrictions)
        {
            this.parameters = parameters;
            this.tableAliases = tableAliases ?? new Dictionary<TableMappingAttribute, string>();
            this.columnAliases = columnAliases ?? new Dictionary<ColumnMappingAttribute, string>();
            this.restrictions = restrictions ?? new Dictionary<ColumnMappingAttribute, string>();
            this.sql = new StringBuilder();
        }

        #endregion

        #region Methods

        public void BuildInsertStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0) return;

            sql.Append("insert into " + table.TableName + " (");

            sql.Append(columns[0].ColumnName);

            for (int i = 1; i < columns.Count; i++)
            {
                sql.Append(", " + columns[i].ColumnName);
            }

            sql.AppendLine(")");
        }

        public void BuildUpdateStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            StringBuilder setBuilder = new StringBuilder();
            StringBuilder restrictionBuilder = new StringBuilder();

            int processedRestrictions = 0;
            int totalColumns = columns.Count;

            sql.AppendLine($"update {tableAliases[table]}");
            sql.Append("set ");

            for (int i = 0; i < totalColumns; i++)
            {
                setBuilder.Append(tableAliases[table]);
                setBuilder.Append(".");
                setBuilder.Append(columns[i].ColumnName);
                setBuilder.Append(" = ");
                setBuilder.Append(parameters[columns[i]][0].Handle);
                setBuilder.AppendLine(i == totalColumns - 1
                    ? ""
                    : ",");

                if (restrictions.ContainsKey(columns[i]))
                {
                    if (processedRestrictions++ == 0)
                    {
                        restrictionBuilder.AppendLine($"from {table.TableName} {tableAliases[table]}");
                        restrictionBuilder.Append("where ");
                    }
                    else
                    {
                        restrictionBuilder.Append("and ");
                    }

                    restrictionBuilder.AppendLine(restrictions[columns[i]]);
                }
            }

            sql.Append(setBuilder.ToString());
            sql.Append(restrictionBuilder.ToString());
        }

        public void BuildValuesStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0) return;

            // Assert that all colums for a given table have the same number of parameters
            int paramCount = columns.Select(c => parameters[c].Count).Max();

            sql.Append("values ");

            // For each set of parameters, we create a seperate set of values:
            // eg: values (set 1), (set 2), (set 3)
            for (int j = 0; j < paramCount; j++)
            {
                sql.Append("(");
                sql.Append(parameters[columns[0]][j].Handle);

                for (int i = 1; i < columns.Count; i++)
                {
                    sql.Append(", " + parameters[columns[i]][j].Handle);
                }

                sql.Append(")");

                if (j < paramCount - 1)
                    sql.Append(",");
            }

            sql.AppendLine();
        }

        public void BuildSelectStatement(List<ColumnMappingAttribute> columns)
        {
            if (columns.Count == 0) return;

            sql.Append("select ");
            sql.Append(columnAliases[columns[0]]);

            for (int i = 1; i < columns.Count; i++)
            {
                sql.Append(", ");
                sql.Append(columnAliases[columns[i]]);
            }

            sql.AppendLine();
        }

        public void BuildFromStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (string.IsNullOrEmpty(table.TableName) || columns.Count == 0) return;

            sql.Append("from ")
               .Append(table.TableName)
               .Append(" as ")
               .AppendLine(tableAliases[table]);

            bool first = true;

            for (int i = 1; i < columns.Count; i++)
            {
                if (restrictions.ContainsKey(columns[i]))
                {
                    if (first)
                    {
                        sql.Append("where ");
                        first = false;
                    }
                    else sql.Append("and ");

                    sql.AppendLine(restrictions[columns[i]]);
                    first = false;
                }
            }
        }

        public void Append(string text) => sql.Append(text);

        public void AppendLine() => sql.AppendLine();

        public void AppendLine(string text) => sql.AppendLine(text);

        public override string ToString() => sql.ToString();

        #endregion
    }
}
