using DataTrack.Core.Attributes;
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
		[Column("id")]
		[PrimaryKey]
		public virtual TIdentity ID { get; set; } = default;

		private static readonly Dictionary<(Type, string), PropertyInfo> properties = new Dictionary<(Type, string), PropertyInfo>();

		public object GetID()
		{
			return ID;
		}

		public object GetPropertyValue(string propertyName)
		{
			return ReflectionUtil.GetPropertyValue(this, propertyName);
		}

		public List<object> GetPropertyValues()
		{
			List<object> values = new List<object>();
			Type type = GetType();
			List<PropertyInfo> nativeProperties = NativePropertyCache.RetrieveItem(type);

			if (nativeProperties != null)
			{
				Logger.Trace($"Loading native properties for Entity '{type.Name}' from cache.");
				foreach (PropertyInfo property in nativeProperties)
				{
					values.Add(ReflectionUtil.GetPropertyValue(this, property.Name));
				}
			}
			else
			{
				nativeProperties = new List<PropertyInfo>();

				Logger.Trace($"Loading native properties for Entity '{type.Name}'.");
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ColumnAttribute)))
				{
					values.Add(ReflectionUtil.GetPropertyValue(this, property.Name));
					nativeProperties.Add(property);
				}

				NativePropertyCache.CacheItem(type, nativeProperties);
			}

			return values;
		}

		public dynamic GetChildPropertyValues(string tableName)
		{
			PropertyInfo property = GetChildProperty(tableName);

			return GetPropertyValue(property.Name);
		}

		public void InstantiateChildProperties()
		{
			Type type = GetType();
			List<PropertyInfo> childProperties = ChildPropertyCache.RetrieveItem(type);


			if (childProperties != null)
			{
				Logger.Trace($"Instantiating child properties for Entity '{type.Name}' from cache.");
				foreach (PropertyInfo property in childProperties)
				{
					Func<object> activator = CompiledActivatorCache.RetrieveItem(property.PropertyType);

					if (activator == null)
					{
						activator = ReflectionUtil.GetActivator(property.PropertyType);
						CompiledActivatorCache.CacheItem(property.PropertyType, activator);
					}

					object instance = activator();
					property.SetValue(this, instance);
				}
			}
			else
			{
				childProperties = new List<PropertyInfo>();

				Logger.Trace($"Instantiating child properties for Entity '{type.Name}'.");
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(TableAttribute)))
				{
					Func<object> activator = ReflectionUtil.GetActivator(property.PropertyType);

					object instance = activator();
					property.SetValue(this, instance);
					CompiledActivatorCache.CacheItem(property.PropertyType, activator);
					childProperties.Add(property);
				}

				ChildPropertyCache.CacheItem(type, childProperties);
			}
		}

		public void AddChildPropertyValue(string tableName, IEntity entity)
		{
			PropertyInfo property = GetChildProperty(tableName);
			dynamic entityList = GetPropertyValue(property.Name);
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
				foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(TableAttribute)))
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
