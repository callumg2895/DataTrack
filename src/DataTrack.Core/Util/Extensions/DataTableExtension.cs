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
                DataColumn[] keys = new DataColumn[1];

                dataTable.Columns.Add(dataColumn);
                
                if (column.IsPrimaryKey())
                {
                    keys[0] = dataColumn;
                    dataTable.PrimaryKey = keys;
                }
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
