using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Interface
{
    public interface IEntity
    {
        object GetPropertyValue(string propertyName);

        List<object> GetPropertyValues();

        dynamic GetChildPropertyValues(string tableName);
    }
}
