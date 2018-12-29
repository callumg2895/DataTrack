using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataTrack.Core.Util.Extensions
{
    public static class DataTableExtension
    {
        public static void SetColumns(this DataTable dataTable, List<ColumnMappingAttribute> columns)
        {
            foreach (ColumnMappingAttribute column in columns)
            {
                DataColumn dataColumn = new DataColumn(column.ColumnName);
                List<DataColumn> primaryKeys = new List<DataColumn>();

                dataTable.Columns.Add(dataColumn);
                
                if (column.IsPrimaryKey())
                    primaryKeys.Add(dataColumn);

                dataTable.PrimaryKey = primaryKeys.ToArray();
            }
        }

        public static void AddRow(this DataTable dataTable, List<ColumnMappingAttribute> columns, List<object> rowData)
        {
            DataRow dataRow = dataTable.NewRow();

            for (int i = 0; i < rowData.Count; i++)
            {
                ColumnMappingAttribute column = columns[i];
                dataRow[column.ColumnName] = rowData[i];
            }

            dataTable.Rows.Add(dataRow);
        }

    }
}
