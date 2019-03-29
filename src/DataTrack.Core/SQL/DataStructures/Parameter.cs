using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public struct Parameter
    {
        public static int Index = 0;
        
        public Parameter(Column column, object value)
        {
            Handle = column.GetParameterHandle(Index++);
            Value = value;
        }

        public string Handle { get; set; }
        public object Value {get; set;}
    }
}
