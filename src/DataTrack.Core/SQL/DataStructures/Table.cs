using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Table
    {
        public TableMappingAttribute TableAttribute { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public List<ColumnMappingAttribute> ColumnAttributes { get; set; }

        public Table(Type type, TableMappingAttribute tableAttribute, List<ColumnMappingAttribute> columnAttributes)
        {
            TableAttribute = tableAttribute;
            Name = tableAttribute.TableName;
            Alias = type.Name;
            ColumnAttributes = columnAttributes;
        }
    }
}
