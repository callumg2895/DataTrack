using DataTrack.Core.Components.Data;
using DataTrack.Core.Enums;
using System.Collections.Generic;

namespace DataTrack.Core.Components.SQL
{
	internal class UpdateStatement : Statement
	{
		internal UpdateStatement(List<Column> columns)
			: base(columns)
		{

			/*
			 * We can only run an update query for columns that have a direct database mapping, i.e a column with that name actually
			 * appears in the database. Because of this, we use only the EntityColumn type.
			 */

			allowedColumnTypes = ColumnTypes.EntityColumn;

		}

		public override string ToString()
		{
			BuildUpdate();
			BuildFrom();
			BuildRestrictions();

			return sql.ToString();
		}

		private void BuildUpdate()
		{
			sql.AppendLine($"update {tables[0].Alias}");
			sql.AppendLine("set");

			for (int i = 0; i < columns.Count; i++)
			{
				if (!IsAllowedColumn(columns[i]))
				{
					continue;
				}
	
				sql.AppendLine($"\t{columns[i].Alias} = {columns[i].Parameters[0].Handle}{(i == columns.Count - 1 ? "" : ",")}");
			}
		}
	}
}
