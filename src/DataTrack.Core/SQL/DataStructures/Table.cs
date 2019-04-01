using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Table : ICloneable
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string StagingName { get; set; }
        public string Alias { get; set; }
        public List<Column> Columns { get; set; }

        private TableMappingAttribute tableMappingAttribute;
        private List<ColumnMappingAttribute> columnMappingAttributes;

        public Table(Type type, TableMappingAttribute tableAttribute, List<ColumnMappingAttribute> columnAttributes)
        {
            Type = type;
            Name = tableAttribute.TableName;
            StagingName = $"#{Name}_staging";
            Alias = type.Name;
            Columns = new List<Column>();

            tableMappingAttribute = tableAttribute;
            columnMappingAttributes = columnAttributes;

            foreach (ColumnMappingAttribute columnAttribute in columnAttributes)
            {
                Columns.Add(new Column(columnAttribute, this));
            }

            Logger.Trace($"Loaded database mapping for Entity '{Type.Name}' (Table '{Name}')");
        }

        public Column GetPrimaryKeyColumn()
        {
            foreach (Column column in Columns)
            {
                if (column.IsPrimaryKey())
                    return column;
            }

            throw new TableMappingException(Type, Name);
        }

        public Column GetForeignKeyColumn(string foreignTableName)
        {
            foreach (Column column in Columns)
            { 
                if (column.IsForeignKey() && column.ForeignKeyTableMapping == foreignTableName)
                    return column;
            }

            throw new TableMappingException(Type, Name);
        }

        public object Clone()
        {
            Logger.Trace($"Cloning database mapping for Entity '{Type.Name}' (Table '{Name}')");
            return new Table(Type, tableMappingAttribute, columnMappingAttributes);
        }
    }
}
