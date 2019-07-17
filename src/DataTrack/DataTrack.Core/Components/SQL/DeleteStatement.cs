using DataTrack.Core.Components.Mapping;

namespace DataTrack.Core.Components.SQL
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
