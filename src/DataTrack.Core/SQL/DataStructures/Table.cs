using DataTrack.Core.Attributes;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using System;
using System.Collections.Generic;

namespace DataTrack.Core.SQL.DataStructures
{
    public abstract class Table 
    {
        public string Name { get; set; }
		public List<Column> Columns { get; set; }
	}
}
