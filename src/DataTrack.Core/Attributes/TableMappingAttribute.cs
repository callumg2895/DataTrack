using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Attributes
{
    public class TableMappingAttribute : Attribute
    {

        public TableMappingAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }

        public PropertyInfo GetChildProperty(Type type, string tableName)
        {
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property;

            return null;
        }

        public dynamic GetChildPropertyValues(object instance, string tableName)
        {
            Type type = instance.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property.GetValue(instance);

            return null;
        }

        public List<object> GetPropertyValues(object instance)
        {
            List<object> values = new List<object>();
            Type type = instance.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute) != null)
                        break;

                values.Add(property.GetValue(instance));
            }

            return values;
        }

    }
}
