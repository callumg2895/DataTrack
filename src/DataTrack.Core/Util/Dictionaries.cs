using DataTrack.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataTrack.Core.Util
{
    public static class Dictionaries
    {
        public static Dictionary<Type, SqlDbType> SQLDataTypes = new Dictionary<Type, SqlDbType>()
        {
            { typeof(bool), SqlDbType.Bit },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(int), SqlDbType.Int },
            { typeof(string), SqlDbType.VarChar }
        };

        public static Dictionary<Type, (TableMappingAttribute Table, List<ColumnMappingAttribute> Columns)> MappingCache = new Dictionary<Type, (TableMappingAttribute Table, List<ColumnMappingAttribute> Columns)>();
    }
}
