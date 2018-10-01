using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
                foreach(Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property;

            return null;
        }

        public object GetChildPropertyValues(object instance, string tableName)
        {
            Type type = instance.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property.GetValue(instance);

            return null;
        }

    }
}
