﻿using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal abstract class Statement
	{
		protected readonly StringBuilder sql;
		protected readonly List<EntityTable> tables;
		protected readonly List<Column> columns;

		private int restrictionCount;

		internal Statement()
		{
			this.tables = new List<EntityTable>();
			this.columns = new List<Column>();
			this.sql = new StringBuilder();

			this.restrictionCount = 0;
		}

		internal Statement(EntityTable table)
			: this()
		{
			this.tables.Add(table);
			this.columns.AddRange(table.Columns);
		}

		internal Statement(List<Column> columns)
			: this()
		{
			HashSet<EntityTable> visitedTables = new HashSet<EntityTable>();

			foreach (Column column in columns)
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

		public abstract override string ToString();

		protected virtual void BuildFrom()
		{
			for (int i = 0; i < tables.Count; i++)
			{
				EntityTable table = tables[i];

				if (i == 0)
				{
					sql.AppendLine($"from {table.Name} as {table.Alias}");
				}
			}
		}

		protected void BuildRestrictions()
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

		private string GetRestrictionKeyWord()
		{
			return restrictionCount++ > 0 ? "and" : "where";
		}
	}
}
