using DataTrack.Core.Util;
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
            StagingTableName = $"#{tableName}_staging";
        }

        public string TableName { get; private set; }
        public string StagingTableName { get; private set; }

        public PropertyInfo GetChildProperty(Type type, string tableName)
        {
            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property;

            // Fatal
            Logger.Error(MethodBase.GetCurrentMethod(), $"FATAL - no child property of type {type.Name} is mapped to table '{tableName}'");
            throw new ArgumentException($"FATAL - no child property of type {type.Name} is mapped to table '{tableName}'", nameof(type));
        }

        public dynamic GetChildPropertyValues(object instance, string tableName)
        {
            Type type = instance.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return property.GetValue(instance);

            // Fatal
            Logger.Error(MethodBase.GetCurrentMethod(), $"FATAL - no child property of type {type.Name} is mapped to table '{tableName}'");
            throw new ArgumentException($"FATAL - no child property of type {type.Name} is mapped to table '{tableName}'", nameof(type));
        }

        public List<object> GetPropertyValues(object instance)
        {
            List<object> values = new List<object>();
            Type type = instance.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute) != null)
                        values.Add(property.GetValue(instance));
            }

            return values;
        }

    }
}
