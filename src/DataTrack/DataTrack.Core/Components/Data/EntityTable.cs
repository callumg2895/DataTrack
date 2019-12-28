using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataTrack.Core.Components.Data
{
	public class EntityTable : Table, ICloneable
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public Type Type { get; set; }
		public Func<object> EntityActivator {get; set;}
		public string Alias { get; set; }
		public List<IEntity> Entities { get; set; }
		public StagingTable StagingTable { get; set; }
		public DataTable DataTable { get; set; }
		internal Mapping Mapping {get; set;}

		private readonly AttributeWrapper _attributes;
		private EntityColumn? primaryKeyColumn;
		private readonly Dictionary<string, EntityColumn?> foreignKeyColumnsDict;
		private readonly List<EntityColumn> foreignKeyColumns;
		private Dictionary<IEntity, DataRow> dataRows { get; set; }

		internal EntityTable(Type type, AttributeWrapper attributes)
			: base()
		{
			Type = type;
			EntityActivator = ReflectionUtil.GetActivator(Type);
			Name = attributes.TableAttribute?.TableName ?? throw new TableMappingException(type, "Unknown");
			Alias = type.Name;
			Entities = new List<IEntity>();
			DataTable = new DataTable(Name);

			_attributes = attributes;
			primaryKeyColumn = null;
			foreignKeyColumnsDict = new Dictionary<string, EntityColumn?>();
			foreignKeyColumns = new List<EntityColumn>();
			dataRows = new Dictionary<IEntity, DataRow>();

			InitialiseEntityColumns(attributes);
			InitiliaseFormulaColumns(attributes);

			StagingTable = new StagingTable(this);

			Logger.Trace($"Loaded database mapping for Entity '{Type.Name}' (Table '{Name}')");
		}

		public EntityColumn GetPrimaryKeyColumn()
		{
			if (primaryKeyColumn == null)
			{
				throw new TableMappingException(Type, Name);
			}

			return primaryKeyColumn;
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

		public void StageForInsertion(IEntity entity)
		{
			UpdateDataTable(entity);
		}

		public void UpdatePrimaryKey(dynamic primaryKey, int entityIndex)
		{
			Logger.Trace($"Updating primary key of '{Type.Name}' entity");

			IEntity entity = Entities[entityIndex];

			entity.SetID(primaryKey);
		}

		internal void UpdateForeignKeys(dynamic primaryKey, int primaryKeyIndex)
		{
			Logger.Trace($"Checking for child entities of '{Type.Name}' entity");

			bool hasChildren = Mapping.ParentChildMapping.TryGetValue(this, out List<EntityTable> childTables) && childTables.Count > 0;

			if (!hasChildren)
			{
				Logger.Trace($"No child tables found for '{Type.Name}' entity");
				return;
			}

			/*
			 * It is guaranteed that entities are processed by the bulk data builder in the same order that their respective
			 * primary keys are read out from the database after a bulk insert. Hence it is safe to assume that the index 
			 * provided by the query executor matches exactly with the position of that entity in the TableEntityMapping list.
			 */

			IEntity entity = Entities[primaryKeyIndex];

			if (!Mapping.ParentChildEntityMapping.ContainsKey(entity))
			{
				Logger.Trace($"No child entities found for '{Type.Name}' entity");
				return;
			}

			foreach (IEntity childEntity in Mapping.ParentChildEntityMapping[entity])
			{
				Type type = childEntity.GetType();
				EntityTable table = Mapping.TypeTableMapping[type];
				table.SetForeignKeyValue(childEntity, primaryKey);
			}
		}

		public void AddDataRow(IEntity item)
		{
			List<object> rowData = item.GetPropertyValues();
			DataRow dataRow = DataTable.NewRow();

			for (int i = 0; i < rowData.Count; i++)
			{
				EntityColumn column = EntityColumns[i];
				object data = rowData[i];

				column.UpdateDataRow(dataRow, data);
			}

			DataTable.Rows.Add(dataRow);
			dataRows.Add(item, dataRow);
		}

		public object Clone()
		{
			Logger.Trace($"Cloning database mapping for Entity '{Type.Name}' (Table '{Name}')");
			return new EntityTable(Type, _attributes);
		}

		private void InitialiseEntityColumns(AttributeWrapper attributes)
		{
			foreach (ColumnAttribute columnAttribute in attributes.ColumnAttributes)
			{
				EntityColumn column = new EntityColumn(columnAttribute, this);
				DataColumn dataColumn = column.DataColumn;

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

					primaryKeyColumn = column;
				}

				/*
				 * Bulk inserts can only be performed on data that is mapped to a physical column in the database. These are
				 * represented by instances of the EntityColumn class, and contain specific methods which determine key type
				 * etc.
				 */

				if (!column.IsPrimaryKey())
				{
					DataTable.Columns.Add(dataColumn);
				}

				EntityColumns.Add(column);
				Columns.Add(column);
			}
		}

		private void InitiliaseFormulaColumns(AttributeWrapper attributes)
		{
			foreach (FormulaAttribute formulaAttribute in attributes.FormulaAttributes)
			{
				FormulaColumn column = new FormulaColumn(formulaAttribute, this);

				FormulaColumns.Add(column);
				Columns.Add(column);
			}
		}

		private void SetForeignKeyValue(IEntity item, dynamic foreignKey)
		{
			EntityTable? parentTable = GetParentTable();

			if (parentTable != null)
			{
				Logger.Trace($"Updating foreign key value for '{Type.Name}' child entity of newly inserted '{parentTable.Type.Name}' entity");

				Column column = GetForeignKeyColumnFor(parentTable);

				dataRows[item][column.Name] = foreignKey;
			}
		}

		private void UpdateDataTable(IEntity item)
		{
			if (item == null)
			{
				return;
			}

			Logger.Trace($"Building DataTable for: {item.GetType().ToString()}");

			Entities.Add(item);
			AddDataRow(item);

			foreach (EntityTable childTable in Mapping.ParentChildMapping[this])
			{
				dynamic childItems = item.GetChildPropertyValues(childTable.Name);

				if (childItems == null)
				{
					continue;
				}

				if (!Mapping.ParentChildEntityMapping.ContainsKey(item))
				{
					Mapping.ParentChildEntityMapping[item] = new List<IEntity>();
				}

				foreach (dynamic childItem in childItems)
				{
					childTable.UpdateDataTable(childItem);
					Mapping.ParentChildEntityMapping[item].Add(childItem);
				}
			}

		}
	}
}
