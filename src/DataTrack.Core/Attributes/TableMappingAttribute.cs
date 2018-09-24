using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
    public class TableMappingAttribute : Attribute
    {

        public TableMappingAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }

    }
}
