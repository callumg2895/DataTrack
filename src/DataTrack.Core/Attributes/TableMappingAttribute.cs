using DataTrack.Core.Exceptions;
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
    }
}
