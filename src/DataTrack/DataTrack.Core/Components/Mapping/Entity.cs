﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Cache;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTrack.Core.Components.Mapping
{
	public abstract class Entity<TIdentity> : IEntity where TIdentity : struct
	{
		private static Logger Logger = DataTrackConfiguration.Logger;
		private ChildPropertyCache childPropertyCache = ChildPropertyCache.Instance;
		private NativePropertyCache nativePropertyCache = NativePropertyCache.Instance;
		private CompiledActivatorCache compiledActivatorCache = CompiledActivatorCache.Instance;

		[Column("id")]
		[PrimaryKey]
		public virtual TIdentity ID { get; set; } = default;

		private static readonly Dictionary<(Type, string), PropertyInfo> properties = new Dictionary<(Type, string), PropertyInfo>();
		private static readonly Dictionary<string, PropertyInfo> propertiesByName = new Dictionary<string, PropertyInfo>();

		public object GetID()
		{
			return ID;
		}

		public object GetPropertyValue(string propertyName)
		{
			Type type = GetType();
			Dictionary<string, PropertyInfo> nativeProperties = nativePropertyCache.RetrieveItem(type);

			if (nativeProperties != null && nativeProperties.ContainsKey(propertyName))
			{
				Logger.Trace($"Loading value for property '{propertyName}' for Entity '{type.Name}' from cache.");
				return ReflectionUtil.GetPropertyValue(this, nativeProperties[propertyName]);
			}

			Logger.Trace($"Loading value for property '{propertyName}' for Entity '{type.Name}'.");
			return ReflectionUtil.GetPropertyValue(this, propertyName);
		}

		public List<object> GetPropertyValues()
		{
			List<object> values = new List<object>();
			Type type = GetType();
			Dictionary<string, PropertyInfo> nativeProperties = nativePropertyCache.RetrieveItem(type);

			if (nativeProperties != null)
			{
				Logger.Trace($"Loading native properties for Entity '{type.Name}' from cache.");
				foreach (PropertyInfo property in nativeProperties.Values)
				{
					values.Add(ReflectionUtil.GetPropertyValue(this, property));
				}
			}
			else
			{
				nativeProperties = new Dictionary<string, PropertyInfo>();

				Logger.Trace($"Loading native properties for Entity '{type.Name}'.");
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ColumnAttribute)))
				{
					values.Add(ReflectionUtil.GetPropertyValue(this, property));
					nativeProperties.Add(property.Name, property);
				}

				nativePropertyCache.CacheItem(type, nativeProperties);
			}

			return values;
		}

		public dynamic GetChildPropertyValues(string tableName)
		{
			PropertyInfo property = GetChildProperty(tableName);

			return ReflectionUtil.GetPropertyValue(this, property);
		}

		public void InstantiateChildProperties()
		{
			Type type = GetType();
			List<PropertyInfo> childProperties = childPropertyCache.RetrieveItem(type);


			if (childProperties != null)
			{
				Logger.Trace($"Instantiating child properties for Entity '{type.Name}' from cache.");
				foreach (PropertyInfo property in childProperties)
				{
					Func<object> activator = compiledActivatorCache.RetrieveItem(property.PropertyType);

					if (activator == null)
					{
						activator = ReflectionUtil.GetActivator(property.PropertyType);
						compiledActivatorCache.CacheItem(property.PropertyType, activator);
					}

					object instance = activator();
					property.SetValue(this, instance);
				}
			}
			else
			{
				childProperties = new List<PropertyInfo>();

				Logger.Trace($"Instantiating child properties for Entity '{type.Name}'.");
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ChildAttribute)))
				{
					Func<object> activator = ReflectionUtil.GetActivator(property.PropertyType);

					object instance = activator();
					property.SetValue(this, instance);
					compiledActivatorCache.CacheItem(property.PropertyType, activator);
					childProperties.Add(property);
				}

				childPropertyCache.CacheItem(type, childProperties);
			}
		}

		public void AddChildPropertyValue(string tableName, IEntity entity)
		{
			PropertyInfo property = GetChildProperty(tableName);
			dynamic entityList = ReflectionUtil.GetPropertyValue(this, property);
			MethodInfo addItem = entityList.GetType().GetMethod("Add");

			addItem.Invoke(entityList, new object[] { entity });
		}

		private PropertyInfo GetChildProperty(string tableName)
		{
			Type type = GetType();

			if (properties.ContainsKey((type, tableName)))
			{
				PropertyInfo property = properties[(type, tableName)];
				Logger.Trace($"Loading property '{property.Name}' for Entity '{type.Name}' from cache. ");
				return property;
			}
			else
			{
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ChildAttribute)))
				{
					properties[(type, tableName)] = property;
					Logger.Trace($"Loading property '{property.Name}' for Entity '{type.Name}'. ");
					return property;
				}
			}

			throw new TableMappingException(type, tableName);
		}
	}
}
