using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
	public class ChildAttribute : Attribute
	{
		public string TableName { get; set; }

		public ChildAttribute(string tableName)
		{
			TableName = tableName;
		}
	}
}
