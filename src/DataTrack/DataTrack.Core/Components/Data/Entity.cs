using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Cache;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTrack.Core.Components.Data
{
	public abstract class Entity<TIdentity> : IEntity where TIdentity : struct
	{
		private static Logger Logger = DataTrackConfiguration.Logger;
		private ChildPropertyCache childPropertyCache = ChildPropertyCache.Instance;
		private NativePropertyCache nativePropertyCache = NativePropertyCache.Instance;
		private CompiledActivatorCache compiledActivatorCache = CompiledActivatorCache.Instance;
		private Mapping mapping = null;

		[Column("id")]
		[PrimaryKey]
		public virtual TIdentity ID { get; set; } = default;

		Mapping IEntity.Mapping
		{
			set 
			{
				mapping = value;
			}
		}

		List<IEntity> IEntity.GetChildren()
		{
			return mapping.ParentChildEntityMapping[this];
		}

		void IEntity.MapChild(IEntity entity)
		{
			Type type = this.GetType();

			Logger.Trace($"Mapping '{entity.GetType()}' child entity for '{type.Name}' entity");
			mapping.ParentChildEntityMapping[this].Add(entity);
		}


		public object GetID()
		{
			return ID;
		}

		public void SetID(dynamic value)
		{
			ID = value;
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
			Dictionary<string, PropertyInfo> nativeProperties = nativePropertyCache.RetrieveItem(type) ?? GetNativeProperties();

			foreach (PropertyInfo property in nativeProperties.Values)
			{
				values.Add(ReflectionUtil.GetPropertyValue(this, property));
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
			Dictionary<string, PropertyInfo> childProperties = childPropertyCache.RetrieveItem(type) ?? GetChildProperties();

			foreach (PropertyInfo property in childProperties.Values)
			{
				Func<object> activator = compiledActivatorCache.RetrieveItem(property.PropertyType) ?? GetActivator(property);

				object instance = activator();

				property.SetValue(this, instance);
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
			Dictionary<string, PropertyInfo> childProperties = childPropertyCache.RetrieveItem(type) ?? GetChildProperties();

			return childProperties[tableName];
		}

		private Dictionary<string, PropertyInfo> GetNativeProperties()
		{
			Type type = GetType();
			Dictionary<string, PropertyInfo>  nativeProperties = new Dictionary<string, PropertyInfo>();

			Logger.Trace($"Loading native properties for Entity '{type.Name}'.");
			foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ColumnAttribute)))
			{
				nativeProperties.Add(property.Name, property);
			}

			nativePropertyCache.CacheItem(type, nativeProperties);

			return nativeProperties;
		}

		private Dictionary<string, PropertyInfo> GetChildProperties()
		{
			Type type = GetType();
			Dictionary<string, PropertyInfo> childProperties = new Dictionary<string, PropertyInfo>();

			Logger.Trace($"Loading child properties for Entity '{type.Name}'.");
			foreach (PropertyInfo property in ReflectionUtil.GetProperties(this, typeof(ChildAttribute)))
			{
				Func<object> activator = ReflectionUtil.GetActivator(property.PropertyType);
				ChildAttribute childAttribute = (ChildAttribute)property.GetAttribute(typeof(ChildAttribute));
				string tableName = childAttribute.TableName;

				compiledActivatorCache.CacheItem(property.PropertyType, activator);

				childProperties.Add(tableName, property);
			}

			childPropertyCache.CacheItem(type, childProperties);

			return childProperties;
		}

		private Func<object> GetActivator(PropertyInfo property)
		{
			Func<object> activator = ReflectionUtil.GetActivator(property.PropertyType);

			compiledActivatorCache.CacheItem(property.PropertyType, activator);

			return activator;
		}
	}
}
