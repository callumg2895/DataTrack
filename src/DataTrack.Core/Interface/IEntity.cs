using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface IEntity
    {
        object GetID();

        object GetPropertyValue(string propertyName);

        List<object> GetPropertyValues();

        dynamic GetChildPropertyValues(string tableName);

        void InstantiateChildProperties();

        void AddChildPropertyValue(string tableName, IEntity entity);
    }
}
