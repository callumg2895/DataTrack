using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
	public class StagingTable : Table
	{
		public EntityTable EntityTable { get; set; }
		
		internal StagingTable(EntityTable table)
		{
			Name = $"#{table.Name}_staging";
			Columns = table.Columns;
			EntityTable = table;
		}

	}
}
