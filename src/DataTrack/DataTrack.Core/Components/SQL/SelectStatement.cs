﻿using DataTrack.Core.Components.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace DataTrack.Core.Components.SQL
{
	internal class SelectStatement : Statement
	{
		private StagingTable? into = null;
		private StagingTable? from = null;
		private Dictionary<string, EntityTable> tableNameMapping;

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

		internal SelectStatement(List<EntityTable> tables, List<Column> columns)
			: base()
		{
			this.tables.AddRange(tables);
			this.columns.AddRange(columns);
		}

		internal SelectStatement From(StagingTable stagingTable)
		{
			from = stagingTable;

			return this;
		}

		internal SelectStatement Into(StagingTable stagingTable)
		{
			into = stagingTable;

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
			HashSet<EntityTable> writtenTables = new HashSet<EntityTable>();

			foreach (EntityTable table in tables)
			{
				string tableName = from != null
					? table.StagingTable.Name
					: table.Name;

				BuildFromSection(table, ref writtenTables);			
			}
		}

		private void BuildFromSection(EntityTable table, ref HashSet<EntityTable> writtenTables)
		{
			if (writtenTables.Contains(table))
			{
				return;
			}

			EntityTable? parentTable = table.GetParentTable();
			string tableName = from != null
				? table.StagingTable.Name
				: table.Name;

			if (parentTable == null || !tables.Contains(parentTable))
			{
				sql.AppendLine($"from {tableName}{(from != null ? "" : $" as {table.Alias}")}");
				writtenTables.Add(table);
			}
			else if (writtenTables.Contains(parentTable))
			{
				sql.AppendLine($"inner join {tableName} as {table.Alias} on {parentTable.Alias}.{parentTable.GetPrimaryKeyColumn().Name} = {table.Alias}.{table.GetForeignKeyColumnFor(parentTable).Name}");
				writtenTables.Add(table);
			}
			else
			{
				BuildFromSection(parentTable, ref writtenTables);
			}
		}
	}
}