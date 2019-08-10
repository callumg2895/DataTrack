using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Mapping
{
	internal class EntityMapping<TBase> : Mapping where TBase : IEntity
	{
		internal Dictionary<IEntity, List<IEntity>> ParentChildEntityMapping { get; set; }
		internal Dictionary<IEntity, DataRow> EntityDataRowMapping { get; set; }
		internal Map<EntityTable, DataTable> DataTableMapping { get; set; }
		internal Dictionary<EntityTable, List<IEntity>> TableEntityMapping { get; set; }

		internal EntityMapping()
			: base(typeof(TBase))
		{
			ParentChildEntityMapping = new Dictionary<IEntity, List<IEntity>>();
			EntityDataRowMapping = new Dictionary<IEntity, DataRow>();
			DataTableMapping = new Map<EntityTable, DataTable>();
			TableEntityMapping = new Dictionary<EntityTable, List<IEntity>>();

			MapEntity(BaseType);
		}

		internal void UpdateTableEntities(EntityTable table, IEntity entity)
		{
			if (TableEntityMapping.ContainsKey(table))
			{
				TableEntityMapping[table].Add(entity);
			}
			else
			{
				TableEntityMapping[table] = new List<IEntity>() { entity };
			}
		}

		internal void UpdateDataTableForeignKeys(EntityTable table, dynamic primaryKey, int primaryKeyIndex)
		{
			Logger.Trace($"Checking for child entities of '{table.Type.Name}' entity");

			bool hasChildren = ParentChildMapping.TryGetValue(table, out List<EntityTable> childTables) && childTables.Count > 0;

			if (!hasChildren)
			{
				Logger.Trace($"No child tables found for '{table.Type.Name}' entity");
				return;
			}

			/*
			 * It is guaranteed that entities are processed by the bulk data builder in the same order that their respective
			 * primary keys are read out from the database after a bulk insert. Hence it is safe to assume that the index 
			 * provided by the query executor matches exactly with the position of that entity in the TableEntityMapping list.
			 */

			IEntity entity = TableEntityMapping[table][primaryKeyIndex];

			if (!ParentChildEntityMapping.ContainsKey(entity))
			{
				Logger.Trace($"No child entities found for '{table.Type.Name}' entity");
				return;
			}

			foreach (IEntity childEntity in ParentChildEntityMapping[entity])
			{
				SetForeignKeyValue(childEntity, primaryKey);
			}
		}

		private void SetForeignKeyValue(IEntity item, dynamic foreignKey)
		{
			EntityTable table = TypeTableMapping[item.GetType()];
			EntityTable? parentTable = table.GetParentTable();

			if (parentTable != null)
			{
				Logger.Trace($"Updating foreign key value for '{table.Type.Name}' child entity of newly inserted '{parentTable.Type.Name}' entity");

				Column column = table.GetForeignKeyColumnFor(parentTable);

				EntityDataRowMapping[item][column.Name] = foreignKey;
			}
		}
	}
}
