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

        public bool TryGetPropertyName(Type type, out string? propertyName)
        {
            PropertyInfo[] properties = type.GetProperties();
            propertyName = null;

            // Try to find the property with a ColumnMappingAttribute that matches the one in the method call
            foreach (PropertyInfo property in properties)
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.ColumnName)
                    {
                        propertyName = property.Name;
                        return true;
                    }

            Logger.Error(MethodBase.GetCurrentMethod(), $"Could not find property '{propertyName}' in object with class '{type.Name}' with attached ColumnMappingAttribute");
            return false;
        }

        public string GetPropertyName(Type type)
        {
            // Try to find the property with a ColumnMappingAttribute that matches the one in the method call
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.ColumnName)
                        return property.Name;

            throw new ColumnMappingException(type, this.ColumnName);
        }

        public SqlDbType GetSqlDbType(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.ColumnName)
                        return Dictionaries.SQLDataTypes[property.PropertyType];

            // Technically the wrong exception to throw. The problem here is that the 'type' supplied
            // does not contain a property with ColumnMappingAttribute with a matching column name.
            throw new ColumnMappingException(type, this.ColumnName);
        }

        public bool IsForeignKey() => (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;
    }
}
