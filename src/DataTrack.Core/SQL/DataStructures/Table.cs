using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
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
        public List<IEntity> Entities { get; set; }

        private TableAttribute tableMappingAttribute;
        private List<ColumnAttribute> columnMappingAttributes;
        private Dictionary<ColumnAttribute, ForeignKeyAttribute> _columnForeignKeys;
        private Dictionary<ColumnAttribute, PrimaryKeyAttribute> _columnPrimaryKeys;

        public Table(Type type, TableAttribute tableAttribute, List<ColumnAttribute> columnAttributes, Dictionary<ColumnAttribute, ForeignKeyAttribute> columnForeignKeys, Dictionary<ColumnAttribute, PrimaryKeyAttribute> columnPrimaryKeys)
        {
            Type = type;
            Name = tableAttribute.TableName;
            StagingName = $"#{Name}_staging";
            Alias = type.Name;
            Columns = new List<Column>();
            Entities = new List<IEntity>();

            tableMappingAttribute = tableAttribute;
            columnMappingAttributes = columnAttributes;
            _columnForeignKeys = columnForeignKeys;
            _columnPrimaryKeys = columnPrimaryKeys;

            foreach (ColumnAttribute columnAttribute in columnAttributes)
            {
                Column column = new Column(columnAttribute, this);

                if (columnForeignKeys.ContainsKey(columnAttribute))
                {
                    ForeignKeyAttribute key = columnForeignKeys[columnAttribute];

                    column.ForeignKeyTableMapping = key.ForeignTable;
                    column.KeyType = (byte)KeyTypes.ForeignKey;
                }

                if (columnPrimaryKeys.ContainsKey(columnAttribute))
                {
                    PrimaryKeyAttribute key = columnPrimaryKeys[columnAttribute];

                    column.KeyType = (byte)KeyTypes.PrimaryKey;
                }

                Columns.Add(column);
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
            return new Table(Type, tableMappingAttribute, columnMappingAttributes, _columnForeignKeys, _columnPrimaryKeys);
        }
    }
}
