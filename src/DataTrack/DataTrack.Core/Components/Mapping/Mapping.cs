﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	internal abstract class Mapping
	{
		internal Type BaseType { get; set; }
		internal List<EntityTable> Tables { get; set; }
		internal Dictionary<Type, EntityTable> TypeTableMapping { get; set; }
		internal Dictionary<EntityTable, List<EntityTable>> ParentChildMapping { get; set; }
		internal Dictionary<EntityTable, EntityTable> ChildParentMapping { get; set; }

		internal Mapping(Type type)
		{
			BaseType = type;

			Tables = new List<EntityTable>();

			TypeTableMapping = new Dictionary<Type, EntityTable>();
			ParentChildMapping = new Dictionary<EntityTable, List<EntityTable>>();
			ChildParentMapping = new Dictionary<EntityTable, EntityTable>();
		}

		protected void MapEntity(Type type)
		{
			if (!TypeTableMapping.ContainsKey(type))
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
		}

		protected void MapTablesByProperty(PropertyInfo property, EntityTable parentTable)
		{
			Type propertyType = property.PropertyType;

			// If the property is a generic list, then it fits the profile of a child object
			if (ReflectionUtil.IsGenericList(propertyType))
			{
				Type genericArgumentType = propertyType.GetGenericArguments()[0];

				MapEntity(genericArgumentType);

				EntityTable mappedTable = TypeTableMapping[genericArgumentType];

				ChildParentMapping[mappedTable] = parentTable;
				ParentChildMapping[parentTable].Add(mappedTable);
			}
		}

		protected EntityTable GetTableByType(Type type)
		{
			return Dictionaries.TypeMappingCache.ContainsKey(type)
				? LoadTableMappingFromCache(type)
				: LoadTableMapping(type);
		}

		protected EntityTable LoadTableMapping(Type type)
		{
			if (TryGetTable(type, out EntityTable? table) && table != null)
			{
				Logger.Trace($"Caching database mapping for Entity '{type.Name}'");
				Dictionaries.TypeMappingCache[type] = table;

				return (EntityTable)table.Clone(this);
			}

			Logger.Error(MethodBase.GetCurrentMethod(), $"Failed to load Table object for '{type.Name}' entity");
			throw new TableMappingException(type, string.Empty);
		}

		protected EntityTable LoadTableMappingFromCache(Type type)
		{
			Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity from cache");
			return (EntityTable)Dictionaries.TypeMappingCache[type].Clone(this);
		}

		protected bool TryGetTable(Type type, out EntityTable? table)
		{
			AttributeWrapper attributes = new AttributeWrapper(type);

			if (attributes.IsValid())
			{
				table = new EntityTable(type, attributes, this);
				return true;
			}

			table = null;
			return false;
		}
	}
}