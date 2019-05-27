using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class SelectStatement : Statement
	{
		private readonly Table table;

		internal SelectStatement(Table table)
		{
			this.table = table;
		}

		public override string ToString()
		{
			List<Column> columns = table.Columns;

			sql.AppendLine()
				.Append($"select {columns[0].Alias}");

			for (int i = 1; i < columns.Count; i++)
			{
				sql.Append(", ")
					.Append(columns[i].Alias);
			}

			return sql.ToString();
		}
	}
}
