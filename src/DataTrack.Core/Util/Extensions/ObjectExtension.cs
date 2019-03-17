using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Util.Extensions
{
    public static class ObjectExtension
    {

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj.GetType()
                      .GetProperty(propertyName)
                      .GetValue(obj);
        }

        public static List<object> GetPropertyValues(this object obj)
        {
            List<object> values = new List<object>();
            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute) != null)
                        values.Add(obj.GetPropertyValue(property.Name));

            return values;
        }

        public static dynamic GetChildPropertyValues(this object obj, string tableName)
        {
            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return obj.GetPropertyValue(property.Name);

            throw new TableMappingException(type, tableName);
        }
    }
}
