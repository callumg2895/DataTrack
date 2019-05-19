using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Util;
using System;
using System.Reflection;
using DataTrack.Logging;
using System.Data;

namespace DataTrack.Core.Attributes
{

    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }

        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
