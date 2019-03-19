using DataTrack.Core.Attributes;
using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Data;

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

        public static Dictionary<Type, Table> TypeMappingCache = new Dictionary<Type, Table>();
    }
}
