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

		internal EntityMapping()
			: base(typeof(TBase))
		{
			ParentChildEntityMapping = new Dictionary<IEntity, List<IEntity>>();
			EntityDataRowMapping = new Dictionary<IEntity, DataRow>();
			DataTableMapping = new Map<EntityTable, DataTable>();

			MapEntity(BaseType);
		}

		internal void UpdateDataTableForeignKeys(EntityTable table, List<dynamic> primaryKeys)
		{
			Type type = table.Type;
			int primaryKeyIndex = 0;

			bool hasChildren = ParentChildMapping.TryGetValue(table, out List<EntityTable> childTables);

			if (!hasChildren)
			{
				return;
			}

			foreach (IEntity entity in ParentChildEntityMapping.Keys)
			{
				if (entity.GetType() != type)
				{
					continue;
				}

				foreach (IEntity childEntity in ParentChildEntityMapping[entity])
				{
					SetForeignKeyValue(childEntity, primaryKeys?[primaryKeyIndex] ?? 0);
				}

				primaryKeyIndex++;
			}
		}

		private void SetForeignKeyValue(IEntity item, dynamic foreignKey)
		{
			EntityTable table = TypeTableMapping[item.GetType()];
			EntityTable? parentTable = table.GetParentTable();

			if (parentTable != null)
			{
				Column column = table.GetForeignKeyColumnFor(parentTable);

				EntityDataRowMapping[item][column.Name] = foreignKey;
			}
		}
	}
}
