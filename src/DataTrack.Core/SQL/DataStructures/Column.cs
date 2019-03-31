using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Logging;
using DataTrack.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Column
    {
        public Column(ColumnMappingAttribute columnAttribute, Table table)
        {
            Table = table;
            Restrictions = new List<Restriction>();
            Parameters = new List<Parameter>();
            Name = columnAttribute.ColumnName;
            Alias = $"{table.Type.Name}.{Name}";
            PropertyName = GetPropertyName(table.Type);
            KeyType = columnAttribute.KeyType;
            ForeignKeyColumnMapping = columnAttribute.ForeignKeyColumnMapping;
            ForeignKeyTableMapping = columnAttribute.ForeignKeyTableMapping;

            if (this.IsForeignKey() && string.IsNullOrEmpty(ForeignKeyTableMapping))
                Logger.Warn(MethodBase.GetCurrentMethod(), $"Column '{Name}' is a foreign key but is not mapped to a table");
        }

        public Table Table { get; set; }
        public List<Restriction> Restrictions { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string PropertyName { get; set; }
        public byte KeyType { get; set; }
        public string? ForeignKeyTableMapping { get; set; }
        public string? ForeignKeyColumnMapping { get; set; }

        public bool TryGetPropertyName(Type type, out string? propertyName)
        {
            PropertyInfo[] properties = type.GetProperties();
            propertyName = null;

            // Try to find the property with a ColumnMappingAttribute that matches the one in the method call
            foreach (PropertyInfo property in properties)
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.Name)
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
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.Name)
                        return property.Name;

            throw new ColumnMappingException(type, this.Name);
        }

        public SqlDbType GetSqlDbType(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute)?.ColumnName == this.Name)
                        return Parameter.SQLDataTypes[property.PropertyType];

            // Technically the wrong exception to throw. The problem here is that the 'type' supplied
            // does not contain a property with ColumnMappingAttribute with a matching column name.
            throw new ColumnMappingException(type, this.Name);
        }

        public bool IsForeignKey() => (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;

        public bool IsPrimaryKey() => (KeyType & (byte)KeyTypes.PrimaryKey) == (byte)KeyTypes.PrimaryKey;

        public void AddRestriction(RestrictionTypes type, object value)
        {
            Parameter parameter = new Parameter(this, value);
            Restriction restriction = new Restriction(this, parameter, type);

            Parameters.Add(parameter);
            Restrictions.Add(restriction);
        }

        public void AddParameter(object value)
        {
            Parameter parameter = new Parameter(this, value);
            Parameters.Add(parameter);
        }
    }
}
