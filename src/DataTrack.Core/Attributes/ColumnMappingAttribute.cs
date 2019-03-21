using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Util;
using System;
using System.Reflection;
using DataTrack.Core.Logging;
using System.Data;

namespace DataTrack.Core.Attributes
{

    public class ColumnMappingAttribute : Attribute
    {
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }
        public byte KeyType { get; private set; }
        public string? ForeignKeyTableMapping { get; private set; }
        public string? ForeignKeyColumnMapping { get; private set; }

        public ColumnMappingAttribute(string tableName, string columnName)
            : this(tableName, columnName, 0) { }

        public ColumnMappingAttribute(string tableName, string columnName, byte keyType, string? foreignKeyTableMapping = null, string? foreignKeyColumnMapping = null)
        {
            TableName = tableName;
            ColumnName = columnName;
            KeyType = keyType;
            ForeignKeyTableMapping = foreignKeyTableMapping;
            ForeignKeyColumnMapping = foreignKeyColumnMapping;

            if (this.IsForeignKey() && string.IsNullOrEmpty(ForeignKeyTableMapping))
                Logger.Warn(MethodBase.GetCurrentMethod(), $"Column '{columnName}' is a foreign key but is not mapped to a table");
        }

        public bool IsForeignKey() => (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;
    }
}
