﻿using DataTrack.Core.Attributes;
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

        public static Dictionary<Type, (TableMappingAttribute Table, List<ColumnMappingAttribute> Columns)> TypeMappingCache = new Dictionary<Type, (TableMappingAttribute Table, List<ColumnMappingAttribute> Columns)>();

        public static Dictionary<TableMappingAttribute, List<ColumnMappingAttribute>> TableMappingCache = new Dictionary<TableMappingAttribute, List<ColumnMappingAttribute>>();

    }
}
