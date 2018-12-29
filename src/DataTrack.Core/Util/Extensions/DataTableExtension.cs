﻿using DataTrack.Core.Attributes;
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
                dataTable.Columns.Add(column.ColumnName);
            }
        }

        public static void AddRow(this DataTable dataTable, List<ColumnMappingAttribute> columns, List<object> rowData)
        {
            DataRow dataRow = dataTable.NewRow();

            for (int i = 0; i < rowData.Count; i++)
            {
                dataRow[columns[i].ColumnName] = rowData[i];
            }

            dataTable.Rows.Add(dataRow);
        }

    }
}
