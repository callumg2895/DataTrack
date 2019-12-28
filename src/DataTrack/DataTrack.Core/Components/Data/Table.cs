using System.Collections.Generic;

namespace DataTrack.Core.Components.Data
{
	public abstract class Table
	{
		protected Table()
		{
			Columns = new List<Column>();
			EntityColumns = new List<EntityColumn>();
			FormulaColumns = new List<FormulaColumn>();
			Name = string.Empty;
		}

		public string Name { get; set; }
		public List<Column> Columns { get; set; }
		public List<EntityColumn> EntityColumns { get; set; }
		public List<FormulaColumn> FormulaColumns { get; set; }
	}
}
