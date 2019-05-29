using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class DeleteStatement : Statement
	{

		private DeleteStatement()
		{

		}

		internal DeleteStatement(Table table)
			: this()
		{
			this.tables.Add(table);
			this.columns.AddRange(table.Columns);
		}

		public override string ToString()
		{
			BuildDelete();
			BuildRestrictions();

			return sql.ToString();
		}

		private void BuildDelete()
		{
			Table table = tables[0];

			sql.AppendLine($"delete {table.Alias} from {table.Name} {table.Alias}");
		}
	}
}
