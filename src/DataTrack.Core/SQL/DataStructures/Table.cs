using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public class Table
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string StagingName { get; set; }
        public string Alias { get; set; }
        public List<Column> Columns { get; set; }

        public Table(Type type, TableMappingAttribute tableAttribute, List<ColumnMappingAttribute> columnAttributes)
        {
            Type = type;
            Name = tableAttribute.TableName;
            StagingName = $"#{Name}_staging";
            Alias = type.Name;
            Columns = new List<Column>();

            foreach (ColumnMappingAttribute columnAttribute in columnAttributes)
            {
                Columns.Add(new Column(columnAttribute, this));
            }
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

        public Table Clone()
        {
            return (Table)this.MemberwiseClone();
        }
    }
}
