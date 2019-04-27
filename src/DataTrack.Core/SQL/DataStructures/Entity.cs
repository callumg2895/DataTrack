﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
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
            Type type = this.GetType();

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableAttribute)?.TableName == tableName)
                        return this.GetPropertyValue(property.Name);

            throw new TableMappingException(type, tableName);
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
            Type type = this.GetType();
            PropertyInfo childProperty = null;
            dynamic entityList = null;

            foreach (PropertyInfo property in type.GetProperties())
                foreach (Attribute attribute in property.GetCustomAttributes())
                    if ((attribute as TableAttribute)?.TableName == tableName)
                    {
                        entityList = this.GetPropertyValue(property.Name);
                        childProperty = property;
                    }

            MethodInfo addItem = entityList.GetType().GetMethod("Add");
            addItem.Invoke(entityList, new object[] { entity });
        }
    }
}
