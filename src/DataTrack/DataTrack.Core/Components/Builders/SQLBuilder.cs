using DataTrack.Core.Components.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Components.Builders
{
	internal abstract class SQLBuilder
	{
		private protected readonly Type _baseType;
		private protected readonly Mapping _mapping;
		private protected readonly StringBuilder _sql;

		internal SQLBuilder(Type type, Mapping mapping)
		{
			_baseType = type;
			_mapping = mapping;
			_sql = new StringBuilder();
		}

		public void Append(string text)
		{
			_sql.Append(text);
		}

		public void AppendLine()
		{
			_sql.AppendLine();
		}

		public void AppendLine(string text)
		{
			_sql.AppendLine(text);
		}

		public override string ToString()
		{
			return _sql.ToString();
		}

		public void SelectRowCount()
		{
			_sql.AppendLine("select @@rowcount as affected_rows");
		}
	}
}
