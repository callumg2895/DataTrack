using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTrack.Util.Helpers
{
	public static class ReflectionUtil
	{
		public static object GetPropertyValue(object instance, string propertyName)
		{
			return instance.GetType().GetProperty(propertyName).GetValue(instance);
		}

		public static IEnumerable<PropertyInfo> GetProperties(object instance, Type? attributeFilter = null)
		{
			Type type = instance.GetType();

			foreach (PropertyInfo property in GetProperties(type, attributeFilter))
			{
				yield return property;
			}
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type, Type? attributeFilter)
		{
			foreach (PropertyInfo property in type.GetProperties())
			{
				if (attributeFilter == null)
				{
					yield return property;
				}

				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if (attribute.GetType() == attributeFilter)
					{
						yield return property;
					}
				}
			}
		}

		public static PropertyInfo? GetProperty(Type type, Attribute attributeFilter)
		{
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					var a = Convert.ChangeType(attribute, attributeFilter.GetType());

					if (a.Equals(attributeFilter))
					{
						return property;
					}
				}
			}

			return null;
		}

		public static bool IsGenericList(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
		}

		public static Func<object> GetActivator(Type type)
		{
			return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
		}
	}
}
