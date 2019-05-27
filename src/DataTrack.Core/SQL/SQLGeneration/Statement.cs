using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.SQL.SQLGeneration
{
	internal abstract class Statement
	{
		protected readonly StringBuilder sql;

		internal Statement()
		{
			sql = new StringBuilder();
		}

		public abstract override string ToString();
	}
}
