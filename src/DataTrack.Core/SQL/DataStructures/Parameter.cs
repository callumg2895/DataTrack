using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public struct Parameter
    {
        public static int Index = 0;

        public static Dictionary<Type, SqlDbType> SQLDataTypes = new Dictionary<Type, SqlDbType>()
        {
            { typeof(bool), SqlDbType.Bit },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(string), SqlDbType.VarChar }           
        };

        public Parameter(Column column, object value)
        {
            Handle = GetParameterHandle(column);
            Value = value;
            DatabaseType = SQLDataTypes[value.GetType()];
        }

        public string Handle { get; set; }
        public object Value {get; set;}
        public SqlDbType DatabaseType { get; set; }

        private static string GetParameterHandle(Column column)
        {
            return $"@{column.Table.Name}_{column.Name}_{Index++}";
        }

        public SqlParameter ToSqlParameter()
        {
            return new SqlParameter(Handle, DatabaseType) { Value = Value };
        }
    }
}
