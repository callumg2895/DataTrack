﻿using System;

namespace DataTrack.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{

		public TableAttribute(string tableName)
		{
			TableName = tableName;
		}

		public string TableName { get; private set; }
	}
}
