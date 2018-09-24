using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Util.Extensions
{
    public static class ObjectExtension
    {

        public static object GetPropertyValue(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName).GetValue(obj);

    }
}
