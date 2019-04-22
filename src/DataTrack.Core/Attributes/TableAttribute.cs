using DataTrack.Core.Exceptions;
using DataTrack.Core.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Attributes
{
    public class TableAttribute : Attribute
    {

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }
    }
}
