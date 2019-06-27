using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal class DeleteStatement : Statement
	{
		internal DeleteStatement(EntityTable table)
			: base(table)
		{

		}

		public override string ToString()
		{
			BuildDelete();
			BuildRestrictions();

			return sql.ToString();
		}

		private void BuildDelete()
		{
			EntityTable table = tables[0];

			sql.AppendLine($"delete {table.Alias} from {table.Name} {table.Alias}");
		}
	}
}
