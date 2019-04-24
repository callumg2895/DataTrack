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

        private AttributeWrapper _attributes = null;

        internal Table(Type type, AttributeWrapper attributes)
        {
            Type = type;
            Name = attributes.TableAttribute.TableName;
            StagingName = $"#{Name}_staging";
            Alias = type.Name;
            Columns = new List<Column>();
            Entities = new List<IEntity>();

            _attributes = attributes;

            foreach (ColumnAttribute columnAttribute in attributes.ColumnAttributes)
            {
                Column column = new Column(columnAttribute, this);

                if (attributes.ColumnForeignKeys.ContainsKey(columnAttribute))
                {
                    ForeignKeyAttribute key = attributes.ColumnForeignKeys[columnAttribute];

                    column.ForeignKeyTableMapping = key.ForeignTable;
                    column.KeyType = (byte)KeyTypes.ForeignKey;
                }

                if (attributes.ColumnPrimaryKeys.ContainsKey(columnAttribute))
                {
                    PrimaryKeyAttribute key = attributes.ColumnPrimaryKeys[columnAttribute];

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
            return new Table(Type, _attributes);
        }
    }
}
