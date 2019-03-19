using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
    public struct Parameter
    {
        public Parameter(string handle, object value)
        {
            Handle = handle;
            Value = value;
        }

        public string Handle { get; set; }
        public object Value {get; set;}
    }
}
