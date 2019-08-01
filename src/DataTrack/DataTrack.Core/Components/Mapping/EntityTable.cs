using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using System;
using System.Collections.Generic;

namespace DataTrack.Core.Components.Mapping
{
	public class EntityTable : Table, ICloneable
	{
		public Type Type { get; set; }
		public string Alias { get; set; }
		public List<IEntity> Entities { get; set; }
		public StagingTable StagingTable { get; set; }
		internal Mapping Mapping {get; set;}

		private readonly AttributeWrapper _attributes;
		private EntityColumn? primaryKeyColumn;
		private readonly Dictionary<string, EntityColumn?> foreignKeyColumnsDict;
		private readonly List<EntityColumn> foreignKeyColumns;

		internal EntityTable(Type type, AttributeWrapper attributes, Mapping mapping)
			: base()
		{
			Type = type;
			Name = attributes.TableAttribute?.TableName ?? throw new TableMappingException(type, "Unknown");
			Alias = type.Name;
			Entities = new List<IEntity>();

			_attributes = attributes;
			primaryKeyColumn = null;
			foreignKeyColumnsDict = new Dictionary<string, EntityColumn?>();
			foreignKeyColumns = new List<EntityColumn>();

			foreach (ColumnAttribute columnAttribute in attributes.ColumnAttributes)
			{
				EntityColumn column = new EntityColumn(columnAttribute, this);

				if (attributes.ColumnForeignKeys.ContainsKey(columnAttribute))
				{
					ForeignKeyAttribute key = attributes.ColumnForeignKeys[columnAttribute];

					column.ForeignKeyTableMapping = key.ForeignTable;
					column.KeyType = (byte)KeyTypes.ForeignKey;

					foreignKeyColumns.Add(column);
					foreignKeyColumnsDict.Add(column.ForeignKeyTableMapping, column);
				}

				if (attributes.ColumnPrimaryKeys.ContainsKey(columnAttribute))
				{
					PrimaryKeyAttribute key = attributes.ColumnPrimaryKeys[columnAttribute];

					column.KeyType = (byte)KeyTypes.PrimaryKey;
				}

				EntityColumns.Add(column);
				Columns.Add(column);
			}

			foreach (FormulaAttribute formulaAttribute in attributes.FormulaAttributes)
			{
				FormulaColumn column = new FormulaColumn(formulaAttribute, this);

				FormulaColumns.Add(column);
				Columns.Add(column);
			}

			StagingTable = new StagingTable(this);
			Mapping = mapping;

			Logger.Trace($"Loaded database mapping for Entity '{Type.Name}' (Table '{Name}')");
		}

		public EntityColumn GetPrimaryKeyColumn()
		{
			if (primaryKeyColumn == null)
			{
				foreach (Column column in Columns)
				{
					if (column.IsPrimaryKey())
					{
						primaryKeyColumn = (EntityColumn)column;
						break;
					}

				}
			}

			return primaryKeyColumn ?? throw new TableMappingException(Type, Name);
		}

		public List<EntityColumn> GetForeignKeyColumns()
		{
			return foreignKeyColumns;
		}

		public EntityColumn GetForeignKeyColumnFor(EntityTable foreignTable)
		{
			return foreignKeyColumnsDict[foreignTable.Name] ?? throw new TableMappingException(Type, Name);
		}

		public EntityTable? GetParentTable()
		{
			return Mapping.ChildParentMapping.ContainsKey(this) 
				? Mapping.ChildParentMapping[this] 
				: null;
		}

		public object Clone()
		{
			return this.Clone(Mapping);
		}

		internal object Clone(Mapping mapping)
		{
			Logger.Trace($"Cloning database mapping for Entity '{Type.Name}' (Table '{Name}')");
			return new EntityTable(Type, _attributes, mapping);
		}
	}
}
