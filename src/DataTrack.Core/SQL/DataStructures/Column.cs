﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
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
        public Column(ColumnAttribute columnAttribute, Table table)
        {
            Table = table;
            Restrictions = new List<Restriction>();
            Parameters = new List<Parameter>();
            Name = columnAttribute.ColumnName;
            Alias = $"{table.Type.Name}.{Name}";
            PropertyName = GetPropertyName(table.Type);

            Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
        }

        public Table Table { get; set; }
        public List<Restriction> Restrictions { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string PropertyName { get; set; }
        public byte KeyType { get; set; }
        public string? ForeignKeyTableMapping { get; set; }

        public string GetPropertyName(Type type)
        {
            if (Name == "id")
                return "ID";
            
            // Try to find the property with a ColumnMappingAttribute that matches the one in the method call
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnAttribute)?.ColumnName == Name)
                        return property.Name;
            

            throw new ColumnMappingException(type, this.Name);
        }

        public SqlDbType GetSqlDbType(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (Name == "id" && property.Name == "ID")
                    return Parameter.SQLDataTypes[property.PropertyType];

                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnAttribute)?.ColumnName == this.Name)
                        return Parameter.SQLDataTypes[property.PropertyType];
            }
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
