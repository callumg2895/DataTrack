using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class SelectStatement : Statement
	{
		private readonly List<Table> tables;
		private readonly List<Column> columns;

		private string selectInto;
		private bool fromStaging;

		private SelectStatement()
		{
			this.selectInto = string.Empty;
			this.fromStaging = false;
		}

		internal SelectStatement(Table table)
			: this()
		{
			this.tables = new List<Table>() { table };
			this.columns = table.Columns;
		}

		internal SelectStatement(List<Column> columns)
			: this()
		{
			this.tables = new List<Table>();

			foreach(Column column in columns)
			{
				if (tables.Contains(column.Table))
				{
					continue;
				}

				tables.Add(column.Table);
			}

			this.columns = columns;
		}

		internal SelectStatement(Column column)
			: this()
		{
			this.tables = new List<Table>() { column.Table };
			this.columns = new List<Column>() { column };
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
			sql.AppendLine()
				.Append($"select {(fromStaging ? columns[0].Name : columns[0].Alias)}");

			for (int i = 1; i < columns.Count; i++)
			{
				sql.Append(", ")
					.Append(fromStaging ? columns[i].Name : columns[i].Alias);
			}

			sql.AppendLine();

			if (!string.IsNullOrEmpty(selectInto))
			{
				sql.AppendLine($"into {selectInto}");
			}

			for (int i = 0; i < tables.Count; i++)
			{
				Table table = tables[i];

				if (i == 0)
				{
					sql.Append($"from {(fromStaging ? table.StagingName : table.Name)}") 
						.AppendLine(fromStaging ? "" : $" as {table.Alias}");
				}
			}

			return sql.ToString();
		}
	}
}
