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

        #endregion

        #region Constructors

        public SQLBuilder(Dictionary<ColumnMappingAttribute, List<(string Handle, object Value)>> parameters)
        {
            this.parameters = parameters;
        }

        #endregion

        #region Methods

        public StringBuilder BuildInsertStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            if (columns.Count == 0)
                return sqlBuilder;

            sqlBuilder.Append("insert into " + table.TableName + " (");

            sqlBuilder.Append(columns[0].ColumnName);

            for (int i = 1; i < columns.Count; i++)
            {
                sqlBuilder.Append(", " + columns[i].ColumnName);
            }

            sqlBuilder.Append(")");

            return sqlBuilder;
        }

        public StringBuilder BuildValuesStatement(List<ColumnMappingAttribute> columns, TableMappingAttribute table)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            if (columns.Count == 0)
                return sqlBuilder;

            // Assert that all colums for a given table have the same number of parameters
            int paramCount = columns.Select(c => parameters[c].Count).Max();

            sqlBuilder.Append("values ");
            
            // For each set of parameters, we create a seperate set of values:
            // eg: values (set 1), (set 2), (set 3)
            for (int j = 0; j < paramCount; j++)
            {
                sqlBuilder.Append("(");
                sqlBuilder.Append(parameters[columns[0]][j].Handle);

                for (int i = 1; i < columns.Count; i++)
                {
                    sqlBuilder.Append(", " + parameters[columns[i]][j].Handle);
                }

                sqlBuilder.Append(")");

                if (j < paramCount - 1)
                    sqlBuilder.Append(",");
            }

            return sqlBuilder;
        }

        #endregion
    }
}
