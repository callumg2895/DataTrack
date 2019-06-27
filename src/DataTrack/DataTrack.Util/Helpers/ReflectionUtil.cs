﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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

		public static bool IsGenericList(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
		}
	}
}