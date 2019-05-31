using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class SelectStatement : Statement
	{
		private string selectInto = string.Empty;
		private bool fromStaging = false;

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

		internal SelectStatement FromStaging()
		{
			this.fromStaging = true;

			return this;
		}

		internal SelectStatement Into(string tableName)
		{
			selectInto = tableName;

			return this;
		}

		public override string ToString()
		{
			BuildSelect();

			if (!string.IsNullOrEmpty(selectInto))
			{
				sql.AppendLine($"into {selectInto}");
			}

			BuildFrom();
			BuildRestrictions();

			return sql.ToString();
		}

		private void BuildSelect()
		{
			sql.AppendLine("select");

			for (int i = 0; i < columns.Count; i++)
			{
				string columnName = fromStaging 
					? columns[i].Name 
					: columns[i].Alias;

				sql.AppendLine($"\t{columnName}{(i == columns.Count - 1 ? "" : ",")}");
			}
		}

		protected override void BuildFrom()
		{
			for (int i = 0; i < tables.Count; i++)
			{
				EntityTable table = tables[i];

				string tableName = fromStaging
					? table.StagingTable.Name
					: table.Name;

				if (i == 0)
				{
					sql.AppendLine($"from {tableName}{(fromStaging ? "" : $" as {table.Alias}")}");
				}
			}
		}
	}
}
