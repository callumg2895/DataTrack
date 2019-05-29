using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal abstract class Statement
	{
		protected readonly StringBuilder sql;
		protected readonly List<Table> tables;
		protected readonly List<Column> columns;

		private int restrictionCount;

		internal Statement()
		{
			this.tables = new List<Table>();
			this.columns = new List<Column>();
			this.sql = new StringBuilder();

			this.restrictionCount = 0;
		}

		public abstract override string ToString();

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
