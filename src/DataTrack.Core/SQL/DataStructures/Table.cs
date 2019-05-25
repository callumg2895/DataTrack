﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using System;
using System.Collections.Generic;

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

        private readonly AttributeWrapper _attributes;
        private Column? primaryKeyColumn;
        private readonly Dictionary<string, Column?> foreignKeyColumnsDict;
		private readonly List<Column> foreignKeyColumns;

        internal Table(Type type, AttributeWrapper attributes)
        {
            Type = type;
            Name = attributes.TableAttribute.TableName;
            StagingName = $"#{Name}_staging";
            Alias = type.Name;
            Columns = new List<Column>();
            Entities = new List<IEntity>();

            _attributes = attributes;
            primaryKeyColumn = null;
            foreignKeyColumnsDict = new Dictionary<string, Column?>();
			foreignKeyColumns = new List<Column>();

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
            if (primaryKeyColumn == null)
            {
                foreach (Column column in Columns)
                {
                    if (column.IsPrimaryKey())
                    {
                        primaryKeyColumn = column;
                        break;
                    }

                }
            }

            return primaryKeyColumn ?? throw new TableMappingException(Type, Name);
        }

		public List<Column> GetForeignKeyColumns()
		{
			return foreignKeyColumns;
		}

		public Column GetForeignKeyColumn(string foreignTableName)
		{
			if (!foreignKeyColumnsDict.ContainsKey(foreignTableName))
			{
				foreignKeyColumnsDict[foreignTableName] = null;

				foreach (Column column in Columns)
				{
					if (column.IsForeignKey() && column.ForeignKeyTableMapping == foreignTableName)
					{
						foreignKeyColumnsDict[foreignTableName] = column;
					}
				}
			}

			return foreignKeyColumnsDict[foreignTableName] ?? throw new TableMappingException(Type, Name);
		}

		public object Clone()
		{
			Logger.Trace($"Cloning database mapping for Entity '{Type.Name}' (Table '{Name}')");
			return new Table(Type, _attributes);
		}
	}
}
