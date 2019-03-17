using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public abstract class Entity
    {
        public object GetPropertyValue(string propertyName)
        {
            return this.GetType()
                      .GetProperty(propertyName)
                      .GetValue(this);
        }

        public List<object> GetPropertyValues()
        {
            List<object> values = new List<object>();
            Type type = this.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnMappingAttribute) != null)
                        values.Add(this.GetPropertyValue(property.Name));

            return values;
        }

        public dynamic GetChildPropertyValues(string tableName)
        {
            Type type = this.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableMappingAttribute)?.TableName == tableName)
                        return this.GetPropertyValue(property.Name);

            throw new TableMappingException(type, tableName);
        }
    }
}
