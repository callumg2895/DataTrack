using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using System;
using System.Reflection;

namespace DataTrack.Core.Util.Extensions
{
	public static class TypeExtension
	{
		public static PropertyInfo GetChildProperty(this Type type, string tableName)
		{
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as TableAttribute)?.TableName == tableName)
					{
						return property;
					}
				}
			}

			throw new TableMappingException(type, tableName);
		}
	}
}
