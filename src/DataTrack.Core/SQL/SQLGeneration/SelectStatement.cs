using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class SelectStatement : Statement
	{
		private StagingTable into = null;
		private StagingTable from = null;

		internal SelectStatement(EntityTable table)
			: base(table)
		{

		}

		internal SelectStatement(List<Column> columns)
			: base(columns)
		{

		}

		internal SelectStatement(Column column)
			: base(new List<Column>() { column })
		{

		}

		internal SelectStatement From(StagingTable stagingTable)
		{
			this.from = stagingTable;

			return this;
		}

		internal SelectStatement Into(StagingTable stagingTable)
		{
			this.into = stagingTable;

			return this;
		}

		public override string ToString()
		{
			BuildSelect();

			if (into != null)
			{
				sql.AppendLine($"into {into.Name}");
			}

			BuildFrom();
			BuildRestrictions();

			return sql.ToString();
		}

		private void BuildSelect()
		{
			sql.AppendLine("select");

			List<string> fromColumns = from != null
				? from.Columns.Where(c => columns.Contains(c)).Select(c => c.Alias).ToList()
				: columns.Select(c => c.Alias).ToList();

			for (int i = 0; i < fromColumns.Count; i++)
			{
				sql.AppendLine($"\t{fromColumns[i]}{(i == fromColumns.Count - 1 ? "" : ",")}");
			}
		}

		protected override void BuildFrom()
		{
			for (int i = 0; i < tables.Count; i++)
			{
				EntityTable table = tables[i];

				string tableName = from != null
					? table.StagingTable.Name
					: table.Name;

				if (i == 0)
				{
					sql.AppendLine($"from {tableName}{(from != null ? "" : $" as {table.Alias}")}");
				}
			}
		}
	}
}
