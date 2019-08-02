using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Enums;

namespace DataTrack.Core.Components.SQL
{
	internal class DeleteStatement : Statement
	{
		internal DeleteStatement(EntityTable table)
			: base(table)
		{
			/*
			 * We can only run an delete query for columns that have a direct database mapping, i.e a column with that name actually
			 * appears in the database. Because of this, we use only the EntityColumn type.
			 */

			allowedColumnTypes = ColumnTypes.EntityColumn;
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
