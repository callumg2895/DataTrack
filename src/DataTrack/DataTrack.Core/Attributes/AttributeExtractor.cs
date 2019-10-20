using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Attributes
{
	public class AttributeExtractor
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public TableAttribute? TableAttribute { get; set; }
		public EntityAttribute? EntityAttribute { get; set; }
		public FormulaAttribute? FormulaAttribute { get; set; }
		public ForeignKeyAttribute? ForeignKeyAttribute { get; set; }
		public PrimaryKeyAttribute? PrimaryKeyAttribute { get; set; }
		public ColumnAttribute? ColumnAttribute { get; set; }
		public UnmappedAttribute? UnmappedAttribute { get; set; }
		public ChildAttribute? ChildAttribute { get; set; }
		public AttributeExtractor(PropertyInfo property)
		{
			Logger.Debug($"Extracting attributes from property '{property.Name}' from class '{property.ReflectedType.Name}'");

			TableAttribute = null;
			EntityAttribute = null;
			FormulaAttribute = null;
			ForeignKeyAttribute = null;
			PrimaryKeyAttribute = null;
			ColumnAttribute = null;
			UnmappedAttribute = null;
			ChildAttribute = null;

			foreach (Attribute attribute in property.GetCustomAttributes())
			{
				TableAttribute = attribute as TableAttribute ?? TableAttribute;
				EntityAttribute = attribute as EntityAttribute ?? EntityAttribute;
				FormulaAttribute = attribute as FormulaAttribute ?? FormulaAttribute;
				ForeignKeyAttribute = attribute as ForeignKeyAttribute ?? ForeignKeyAttribute;
				PrimaryKeyAttribute = attribute as PrimaryKeyAttribute ?? PrimaryKeyAttribute;
				ColumnAttribute = attribute as ColumnAttribute ?? ColumnAttribute;
				UnmappedAttribute = attribute as UnmappedAttribute ?? UnmappedAttribute;
				ChildAttribute = attribute as ChildAttribute ?? ChildAttribute;
			}
		}

		public bool PropertyIsMapped()
		{
			return !(ColumnAttribute == null 
				&& FormulaAttribute == null 
				&& EntityAttribute == null 
				&& TableAttribute == null 
				&& ChildAttribute == null
				&& UnmappedAttribute == null);
		}
	}
}
