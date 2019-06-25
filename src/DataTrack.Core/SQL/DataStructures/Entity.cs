using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.SQL.DataStructures
{
	public abstract class Entity<TIdentity> : IEntity
	{
		[Column("id")]
		[PrimaryKey]
		public virtual TIdentity ID { get; set; }

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

			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as ColumnAttribute) != null)
					{
						values.Add(GetPropertyValue(property.Name));
					}
				}
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

			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as TableAttribute) != null)
					{
						property.SetValue(this, Activator.CreateInstance(property.PropertyType));
					}
				}
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
				foreach (PropertyInfo property in type.GetProperties())
				{
					foreach (Attribute attribute in property.GetCustomAttributes())
					{
						if ((attribute as TableAttribute)?.TableName == tableName)
						{
							properties[(type, tableName)] = property;
							Logger.Trace($"Loading property '{property.Name}' for Entity '{type.Name}'. ");
							return property;
						}
					}
				}
			}

			throw new TableMappingException(type, tableName);
		}
	}
}
