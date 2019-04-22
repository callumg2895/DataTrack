using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Util;
using System;
using System.Reflection;
using DataTrack.Core.Logging;
using System.Data;

namespace DataTrack.Core.Attributes
{

    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public byte KeyType { get; private set; }
        public string? ForeignKeyTableMapping { get; private set; }
        public string? ForeignKeyColumnMapping { get; private set; }

        public ColumnAttribute(string columnName)
            : this(columnName, 0) { }

        public ColumnAttribute(string columnName, byte keyType, string? foreignKeyTableMapping = null, string? foreignKeyColumnMapping = null)
        {
            ColumnName = columnName;
            KeyType = keyType;
            ForeignKeyTableMapping = foreignKeyTableMapping;
            ForeignKeyColumnMapping = foreignKeyColumnMapping;
        }
    }
}
