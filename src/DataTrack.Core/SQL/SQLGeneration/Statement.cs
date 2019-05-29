using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal abstract class Statement
	{
		protected readonly StringBuilder sql;

		private int restrictionCount;

		internal Statement()
		{
			sql = new StringBuilder();
			restrictionCount = 0;
		}

		public abstract override string ToString();

		protected string GetRestrictionKeyWord()
		{
			return restrictionCount++ > 0 ? "and" : "where";
		}
	}
}
