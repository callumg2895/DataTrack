using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public string ForeignTable { get; private set; }

        public ForeignKeyAttribute(string foreignTable)
        {
            ForeignTable = foreignTable;
        }
    }
}
