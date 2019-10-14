using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
	public class ParentAttribute : Attribute
	{
		public string TableName { get; set; }

		public ParentAttribute(string tableName)
		{
			TableName = tableName;
		}
	}
}
