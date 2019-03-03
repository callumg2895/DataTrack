using DataTrack.Core.Util;
using System;

namespace DataTrack.Core.Exceptions
{
    public class TableMappingException : Exception
    {
        public TableMappingException(Type type, string tableName)
            : base($"No child property of type {type.Name} is mapped to table '{tableName}'")
        {
            Logger.ErrorFatal(this.TargetSite, Message);
        }
    }
}
