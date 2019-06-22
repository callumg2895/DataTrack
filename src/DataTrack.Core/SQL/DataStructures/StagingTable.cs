﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.DataStructures
{
	public class StagingTable : Table
	{
		public EntityTable EntityTable { get; set; }
		
		internal StagingTable(EntityTable table)
			: base()
		{
			Name = $"#{table.Name}_staging";
			EntityTable = table;

			foreach (Column column in table.Columns)
			{
				Column stagingColumn = (Column)column.Clone();

				stagingColumn.Alias = column.Name;

				Columns.Add(stagingColumn);
			}
		}

	}
}
