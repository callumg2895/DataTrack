﻿using DataTrack.Core.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using DataTrack.Core.Enums;

namespace DataTrack.Core.Attributes
{

    public class ColumnMappingAttribute : Attribute
    {
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }
        public KeyTypes KeyType { get; private set; }
        public string ForeignKeyMapping { get; private set; }

        public ColumnMappingAttribute(string tableName, string columnName)
            : this(tableName, columnName, KeyTypes.None) { }

        public ColumnMappingAttribute(string tableName, string columnName, KeyTypes keyType, string foreignKeyMapping = null) 
        {
            TableName = tableName;
            ColumnName = columnName;
            KeyType = keyType;
            ForeignKeyMapping = foreignKeyMapping;

            if (KeyType == KeyTypes.ForeignKey && string.IsNullOrEmpty(ForeignKeyMapping))
                Logger.Warn(MethodBase.GetCurrentMethod(), $"Column '{columnName}' is a foreign key but is not mapped to a table");
        }

        public bool TryGetPropertyName(Type type, out string propertyName)
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
    }
}