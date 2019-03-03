using DataTrack.Core.Util;
using System;

namespace DataTrack.Core.Exceptions
{
    public class TableMappingException : Exception
    {
        public TableMappingException(Type type, string tableName)
            : base($"no child property of type {type.Name} is mapped to table '{tableName}'")
        {
            Logger.Error(this.TargetSite, Message);
        }
    }
}
