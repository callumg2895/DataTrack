using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Table
    {
        public Table(Type type, TableMappingAttribute table, List<ColumnMappingAttribute> columns)
        {
            TableAttribute = table;
            Alias = type.Name;
            ColumnAttributes = columns;
        }

        public TableMappingAttribute TableAttribute { get; set; }
        public string Alias { get; set; }
        public List<ColumnMappingAttribute> ColumnAttributes { get; set; }
    }
}
