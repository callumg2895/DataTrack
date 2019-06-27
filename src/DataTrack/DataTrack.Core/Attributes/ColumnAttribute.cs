using System;

namespace DataTrack.Core.Attributes
{

	public class ColumnAttribute : Attribute
	{
		public string ColumnName { get; private set; }

		public ColumnAttribute(string columnName)
		{
			ColumnName = columnName;
		}
	}
}
