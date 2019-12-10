using System;

namespace DataTrack.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		public string ColumnName { get; private set; }

		public ColumnAttribute(string columnName)
		{
			ColumnName = columnName;
		}
	}
}
