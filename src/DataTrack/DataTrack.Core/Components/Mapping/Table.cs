using System.Collections.Generic;

namespace DataTrack.Core.Components.Mapping
{
	public abstract class Table
	{
		protected Table()
		{
			Columns = new List<Column>();
			Name = string.Empty;
		}

		public string Name { get; set; }
		public List<Column> Columns { get; set; }
	}
}
