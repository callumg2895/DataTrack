using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Table
    {
        public Table(TableMappingAttribute table, List<ColumnMappingAttribute> columns)
        {
            TableAttribute = table;
            ColumnAttributes = columns;
        }

        public TableMappingAttribute TableAttribute { get; set; }
        public List<ColumnMappingAttribute> ColumnAttributes { get; set; }
    }
}
