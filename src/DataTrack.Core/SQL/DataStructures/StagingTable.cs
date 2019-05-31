using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
	public class StagingTable : Table
	{
		public List<string> Columns { get; set; }
		public EntityTable EntityTable { get; set; }
		
		internal StagingTable(EntityTable table)
		{
			Name = $"#{table.Name}_staging";
			Columns = new List<string>();
			EntityTable = table;
				
			Columns.AddRange(table.Columns.Select(c => c.Name));
		}

	}
}
