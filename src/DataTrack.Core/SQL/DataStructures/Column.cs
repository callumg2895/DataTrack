using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Column
    {
        public Column(ColumnMappingAttribute columnAttribute, Table table)
        {
            ColumnMappingAttribute = columnAttribute;
            Table = table;
            Name = columnAttribute.ColumnName;
            KeyType = columnAttribute.KeyType;
            ForeignKeyColumnMapping = columnAttribute.ForeignKeyColumnMapping;
            ForeignKeyTableMapping = columnAttribute.ForeignKeyTableMapping;
        }

        public ColumnMappingAttribute ColumnMappingAttribute { get; set; }
        public Table Table { get; set; }
        public string Name { get; set; }
        public byte KeyType { get; set; }
        public string? ForeignKeyTableMapping { get; set; }
        public string? ForeignKeyColumnMapping { get; set; }

        public bool IsForeignKey() => (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;

        public bool IsPrimaryKey() => (KeyType & (byte)KeyTypes.PrimaryKey) == (byte)KeyTypes.PrimaryKey;
    }
}
