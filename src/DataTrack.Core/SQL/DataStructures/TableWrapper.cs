using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class TableWrapper
    {
        public TableWrapper(TableMappingAttribute table, List<ColumnMappingAttribute> columns)
        {
            Table = table;
            Columns = columns;
        }

        public TableMappingAttribute Table { get; set; }
        public List<ColumnMappingAttribute> Columns { get; set; }
    }
}
