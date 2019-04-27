using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public abstract class Entity<TIdentity> : IEntity
    {
        [Column("id")]
        [PrimaryKey]
        public TIdentity ID { get; set; }

        private static Dictionary<(Type, string), PropertyInfo> properties = new Dictionary<(Type, string), PropertyInfo>(); 

        public object GetID() => ID;

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
            {
                if (property.Name == "ID")
                {
                    values.Add(this.GetPropertyValue(property.Name));
                    continue;
                }

                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as ColumnAttribute) != null)
                        values.Add(this.GetPropertyValue(property.Name));
            }

            return values;
        }

        public dynamic GetChildPropertyValues(string tableName)
        {
            PropertyInfo property = GetChildProperty(tableName);

            return this.GetPropertyValue(property.Name);
        }

        public void InstantiateChildProperties()
        {
            Type type = this.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableAttribute) != null)
                    {
                        property.SetValue(this, Activator.CreateInstance(property.PropertyType));
                    }
        }

        public void AddChildPropertyValue(string tableName, IEntity entity)
        {
            PropertyInfo property = GetChildProperty(tableName);
            dynamic entityList = this.GetPropertyValue(property.Name);
            MethodInfo addItem = entityList.GetType().GetMethod("Add");

            addItem.Invoke(entityList, new object[] { entity });
        }

        private PropertyInfo GetChildProperty(string tableName)
        {
            Type type = this.GetType();

            if (properties.ContainsKey((type, tableName)))
            {
                PropertyInfo property = properties[(type, tableName)];
                Logger.Trace($"Loading property '{property.Name}' for Entity '{type.Name}' from cache. ");
                return property;
            }
            else
            {
                foreach (PropertyInfo property in type.GetProperties())
                    foreach (Attribute attribute in property.GetCustomAttributes())
                        if ((attribute as TableAttribute)?.TableName == tableName)
                        {
                            properties[(type, tableName)] = property;
                            Logger.Trace($"Loading property '{property.Name}' for Entity '{type.Name}'. ");
                            return property;
                        }
            }

            throw new TableMappingException(type, tableName);
        }
    }
}
