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
		private readonly List<Restriction> restrictions;

		private string selectInto;
		private bool fromStaging;

		private SelectStatement()
		{
			this.selectInto = string.Empty;
			this.fromStaging = false;
			this.tables = new List<Table>();
			this.columns = new List<Column>();
		}

		internal SelectStatement(Table table)
			: this()
		{
			this.tables.Add(table);
			this.columns.AddRange(table.Columns);
		}

		internal SelectStatement(List<Column> columns)
			: this()
		{
			HashSet<Table> visitedTables = new HashSet<Table>();

			foreach(Column column in columns)
			{
				if (visitedTables.Contains(column.Table))
				{
					continue;
				}

				visitedTables.Add(column.Table);

				this.tables.Add(column.Table);
			}

			this.columns.AddRange(columns);
		}

		internal SelectStatement(Column column)
			: this()
		{
			this.tables.Add(column.Table);
			this.columns.Add(column);
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

		private void BuildFrom()
		{
			for (int i = 0; i < tables.Count; i++)
			{
				Table table = tables[i];

				string tableName = fromStaging
					? table.StagingName
					: table.Name;

				if (i == 0)
				{
					sql.AppendLine($"from {tableName}{(fromStaging ? "" : $" as {table.Alias}")}");
				}
			}
		}

		private void BuildRestrictions()
		{
			foreach (Column column in columns)
			{
				foreach (Restriction restriction in column.Restrictions)
				{
					sql.Append($"{GetRestrictionKeyWord()} ")
					   .AppendLine(restriction.ToString());
				}

				column.Restrictions.Clear();
			}
		}
	}
}
