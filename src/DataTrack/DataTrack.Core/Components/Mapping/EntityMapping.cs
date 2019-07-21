using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Mapping
{
	internal class EntityMapping<TBase> where TBase : IEntity
	{
		internal Type BaseType { get; set; }
		internal List<EntityTable> Tables { get; set; }
		internal Dictionary<Type, EntityTable> TypeTableMapping { get; set; }
		internal Dictionary<EntityTable, List<EntityTable>> ParentChildMapping { get; set; }
		internal Dictionary<EntityTable, EntityTable> ChildParentMapping { get; set; }
		internal Dictionary<IEntity, List<IEntity>> ParentChildEntityMapping { get; set; }
		internal Dictionary<IEntity, DataRow> EntityDataRowMapping { get; set; }
		internal Map<EntityTable, DataTable> DataTableMapping { get; set; }

		internal EntityMapping()
		{
			BaseType = typeof(TBase);

			Tables = new List<EntityTable>();

			TypeTableMapping = new Dictionary<Type, EntityTable>();
			ParentChildMapping = new Dictionary<EntityTable, List<EntityTable>>();
			ChildParentMapping = new Dictionary<EntityTable, EntityTable>();
			ParentChildEntityMapping = new Dictionary<IEntity, List<IEntity>>();
			EntityDataRowMapping = new Dictionary<IEntity, DataRow>();
			DataTableMapping = new Map<EntityTable, DataTable>();

			MapTable(BaseType);
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
					SetForeignKeyValue(childEntity, table.Name, primaryKeys?[primaryKeyIndex] ?? 0);
				}

				primaryKeyIndex++;
			}
		}

		private void SetForeignKeyValue(IEntity item, string foreignTable, dynamic foreignKey)
		{
			EntityTable table = TypeTableMapping[item.GetType()];
			Column column = table.GetForeignKeyColumn(foreignTable);

			EntityDataRowMapping[item][column.Name] = foreignKey;
		}

		private void MapTable(Type type)
		{
			EntityTable table = GetTableByType(type);

			Tables.Add(table);
			TypeTableMapping.Add(type, table);
			ParentChildMapping.Add(table, new List<EntityTable>());

			foreach (PropertyInfo prop in type.GetProperties())
			{
				MapTablesByProperty(prop, table);
			}
		}

		private void MapTablesByProperty(PropertyInfo property, EntityTable parentTable)
		{
			Type propertyType = property.PropertyType;

			// If the property is a generic list, then it fits the profile of a child object
			if (ReflectionUtil.IsGenericList(propertyType))
			{
				Type genericArgumentType = propertyType.GetGenericArguments()[0];

				MapTable(genericArgumentType);

				EntityTable mappedTable = TypeTableMapping[genericArgumentType];

				ChildParentMapping[mappedTable] = parentTable;
				ParentChildMapping[parentTable].Add(mappedTable);
			}
		}

		private EntityTable GetTableByType(Type type)
		{
			return Dictionaries.TypeMappingCache.ContainsKey(type)
				? LoadTableMappingFromCache(type)
				: LoadTableMapping(type);
		}

		private EntityTable LoadTableMapping(Type type)
		{
			if (TryGetTable(type, out EntityTable? table) && table != null)
			{
				Logger.Trace($"Caching database mapping for Entity '{type.Name}'");
				Dictionaries.TypeMappingCache[type] = table;

				return (EntityTable)table.Clone();
			}

			Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load Table object for '{type.Name}' entity");
			throw new TableMappingException(type, string.Empty);
		}

		private EntityTable LoadTableMappingFromCache(Type type)
		{
			Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity from cache");
			return (EntityTable)Dictionaries.TypeMappingCache[type].Clone();
		}

		private protected bool TryGetTable(Type type, out EntityTable? table)
		{
			AttributeWrapper attributes = new AttributeWrapper(type);

			if (attributes.IsValid())
			{
				table = new EntityTable(type, attributes);
				return true;
			}

			table = null;
			return false;
		}
	}
}
