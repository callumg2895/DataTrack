using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
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
