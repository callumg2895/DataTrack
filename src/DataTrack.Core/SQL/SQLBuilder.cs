using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL
{
    public class SQLBuilder
    {
        #region Members

        private Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters;
        private StringBuilder sql;

        #endregion

        #region Constructors

        public SQLBuilder(Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters)
        {
            this.parameters = parameters;
            this.sql = new StringBuilder();
        }

        #endregion

        #region Methods

        public void BuildInsertStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0)
                return;

            sql.Append("insert into " + table.TableName + " (");

            sql.Append(columns[0].ColumnName);

            for (int i = 1; i < columns.Count; i++)
            {
                sql.Append(", " + columns[i].ColumnName);
            }

            sql.AppendLine(")");
        }

        public void BuildValuesStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            if (columns.Count == 0)
                return;

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

        public void Append(string text) => sql.Append(text);

        public void AppendLine() => sql.AppendLine();

        public void AppendLine(string text) => sql.AppendLine(text);

        public override string ToString() => sql.ToString();
        

        #endregion
    }
}
