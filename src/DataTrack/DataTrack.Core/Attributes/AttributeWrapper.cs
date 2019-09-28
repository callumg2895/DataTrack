using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Attributes
{
	public class AttributeWrapper
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public TableAttribute? TableAttribute { get; private set; }
		public List<TableAttribute> ChildTableAttributes { get; private set; }
		public List<EntityAttribute> EntityAttributes { get; private set; }
		public List<FormulaAttribute> FormulaAttributes { get; private set; }
		public List<ColumnAttribute> ColumnAttributes { get; private set; }
		public Dictionary<ColumnAttribute, ForeignKeyAttribute> ColumnForeignKeys { get; private set; }
		public Dictionary<ColumnAttribute, PrimaryKeyAttribute> ColumnPrimaryKeys { get; private set; }

		private Type entityType;

		public AttributeWrapper(Type type)
		{
			TableAttribute = null;
			ChildTableAttributes = new List<TableAttribute>();
			ColumnAttributes = new List<ColumnAttribute>();
			EntityAttributes = new List<EntityAttribute>();
			FormulaAttributes = new List<FormulaAttribute>();
			ColumnForeignKeys = new Dictionary<ColumnAttribute, ForeignKeyAttribute>();
			ColumnPrimaryKeys = new Dictionary<ColumnAttribute, PrimaryKeyAttribute>();

			entityType = type;

			Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Attribute mappings for class '{type.Name}'");

			foreach (Attribute attribute in type.GetCustomAttributes())
			{
				TableAttribute = attribute as TableAttribute;
			}

			foreach (PropertyInfo property in type.GetProperties())
			{
				LoadAttributes(property);
			}

			Validate();
		}

		internal bool IsValid()
		{
			return TableAttribute != null && ColumnAttributes.Count > 0;
		}

		private void LoadAttributes(PropertyInfo property)
		{
			AttributeExtractor extractor = new AttributeExtractor(property);

			if (extractor.UnmappedAttribute != null)
			{
				return;
			}

			if (extractor.TableAttribute != null)
			{
				ChildTableAttributes.Add(extractor.TableAttribute);
			}

			if (extractor.EntityAttribute != null)
			{
				EntityAttributes.Add(extractor.EntityAttribute);
			}

			if (extractor.FormulaAttribute != null)
			{
				FormulaAttributes.Add(extractor.FormulaAttribute);
			}

			if (extractor.ColumnAttribute != null)
			{
				if (!ColumnAttributes.Contains(extractor.ColumnAttribute))
				{
					ColumnAttributes.Add(extractor.ColumnAttribute);
				}

				if (extractor.ForeignKeyAttribute != null)
				{
					ColumnForeignKeys[extractor.ColumnAttribute] = extractor.ForeignKeyAttribute;
				}

				if (extractor.PrimaryKeyAttribute != null)
				{
					ColumnPrimaryKeys[extractor.ColumnAttribute] = extractor.PrimaryKeyAttribute;
				}
			}

			if (!extractor.PropertyIsMapped())
			{
				/* 
				 * We should warn the user if they have a property in this class that is not mapped, as 
				 * this may have been unintentional. They should used an UnmappedAttribute if they wish to 
				 * express that the property was left unmapped on purpose.
				 */

				Logger.Warn(MethodBase.GetCurrentMethod(), $"Property {property.Name} of class {entityType.Name} is not mapped explicitly, use an UnmappedAttribute if this was intentional.");
			}
		}

		private void Validate()
		{
			if (TableAttribute == null && ColumnAttributes.Count == 0 && EntityAttributes.Count == 0 && FormulaAttributes.Count == 0)
			{
				throw new MappingException($"Connot find any mapping information for class {entityType.Name}");
			}

			if (FormulaAttributes.Count > 0 && EntityAttributes.Count > 0)
			{
				throw new MappingException($"Cannot define Formula columns for an Entity based mapping for class {entityType.Name}");
			}

			if (TableAttribute != null && EntityAttributes.Count > 0)
			{
				throw new MappingException($"Cannot define both Table and Entity based mapping for class {entityType.Name}");
			}

			if (TableAttribute != null && ColumnAttributes.Count == 0)
			{
				throw new MappingException($"Cannot find column mappings for class {entityType.Name}");
			}
		}
	}
}
