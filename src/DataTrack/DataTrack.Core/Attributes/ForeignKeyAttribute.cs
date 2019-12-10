using System;

namespace DataTrack.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ForeignKeyAttribute : Attribute
	{
		public string ForeignTable { get; private set; }

		public ForeignKeyAttribute(string foreignTable)
		{
			ForeignTable = foreignTable;
		}
	}
}
