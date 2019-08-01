using DataTrack.Core.Components.Mapping;
using System.Collections.Generic;

namespace DataTrack.Core.Components.SQL
{
	internal class UpdateStatement : Statement
	{
		internal UpdateStatement(List<Column> columns)
			: base(columns)
		{

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
				sql.AppendLine($"\t{columns[i].Alias} = {columns[i].Parameters[0].Handle}{(i == columns.Count - 1 ? "" : ",")}");
			}
		}
	}
}
