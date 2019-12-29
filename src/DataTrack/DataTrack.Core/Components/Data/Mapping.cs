using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Cache;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Data
{
	internal abstract class Mapping
	{
		private static Logger Logger = DataTrackConfiguration.Logger;
		private EntityTableCache entityTableCache = EntityTableCache.Instance;

		internal Type BaseType { get; set; }
		internal List<EntityTable> Tables { get; set; }
		internal Dictionary<Type, EntityTable> TypeTableMapping { get; set; }
		internal Dictionary<IEntity, List<IEntity>> ParentChildEntityMapping { get; set; }

		internal Mapping(Type type)
		{
			BaseType = type;
			Tables = new List<EntityTable>();
			TypeTableMapping = new Dictionary<Type, EntityTable>();
			ParentChildEntityMapping = new Dictionary<IEntity, List<IEntity>>();
		}

		protected void MapType(Type type)
		{
			if (!TypeTableMapping.ContainsKey(type))
			{
				EntityTable table = GetTableByType(type);

				table.Mapping = this;

				Tables.Add(table);
				TypeTableMapping.Add(type, table);

				foreach (PropertyInfo prop in type.GetProperties())
				{
					MapTablesByProperty(prop, table);
				}
			}
		}

		protected void MapTablesByProperty(PropertyInfo property, EntityTable parentTable)
		{
			Type propertyType = property.PropertyType;

			// If the property is a generic list, then it fits the profile of a child object
			if (ReflectionUtil.IsGenericList(propertyType))
			{
				Type genericArgumentType = propertyType.GetGenericArguments()[0];

				MapType(genericArgumentType);

				EntityTable mappedTable = TypeTableMapping[genericArgumentType];

				mappedTable.ParentTable = parentTable;
				parentTable.ChildTables.Add(mappedTable);
			}
		}

		protected EntityTable GetTableByType(Type type)
		{
			EntityTable? entityTable = entityTableCache.RetrieveItem(type);

			if (entityTable != null)
			{
				return (EntityTable)entityTable.Clone();
			}
			else
			{
				return LoadTableMapping(type);
			}
		}

		protected EntityTable LoadTableMapping(Type type)
		{
			if (TryGetTable(type, out EntityTable? table) && table != null)
			{
				entityTableCache.CacheItem(type, (EntityTable)table.Clone());

				return (EntityTable)table.Clone();
			}

			Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load Table object for '{type.Name}' entity");
			throw new TableMappingException(type, string.Empty);
		}

		protected bool TryGetTable(Type type, out EntityTable? table)
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
