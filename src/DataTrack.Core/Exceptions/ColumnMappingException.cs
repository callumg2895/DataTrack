using DataTrack.Core.Util;
using System;

namespace DataTrack.Core.Exceptions
{
    public class ColumnMappingException : Exception
    {
        public ColumnMappingException(Type type, string columnName)
            : base($"No property of type {type.Name} is mapped to column '{columnName}'")
        {
            Logger.Error(this.TargetSite, Message);
        }
    }
}
