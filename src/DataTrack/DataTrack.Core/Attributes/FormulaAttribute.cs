using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
	public class FormulaAttribute : Attribute
	{
		public string Query { get; set; }

		public string Property { get; set; }

		public string Alias {get; set;}

		public FormulaAttribute(string alias, string query)
		{
			Alias = alias;
			Query = query;
		}
	}
}
