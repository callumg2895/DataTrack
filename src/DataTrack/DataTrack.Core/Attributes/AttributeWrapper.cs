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
		public TableAttribute? TableAttribute { get; private set; }
		public List<EntityAttribute> EntityAttributes { get; private set; }
		public List<FormulaAttribute> FormulaAttributes { get; private set; }
		public List<ColumnAttribute> ColumnAttributes { get; private set; }
		public Dictionary<ColumnAttribute, ForeignKeyAttribute> ColumnForeignKeys { get; private set; }
		public Dictionary<ColumnAttribute, PrimaryKeyAttribute> ColumnPrimaryKeys { get; private set; }

		private Type entityType;

		public AttributeWrapper(Type type)
		{
			TableAttribute = null;
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
			EntityAttribute? entityAttribute = null;
			FormulaAttribute? formulaAttribute = null;
			ForeignKeyAttribute? foreignKeyAttribute = null;
			PrimaryKeyAttribute? primaryKeyAttribute = null;
			ColumnAttribute? columnAttribute = null;

			foreach (Attribute attribute in property.GetCustomAttributes())
			{
				entityAttribute = attribute as EntityAttribute ?? entityAttribute;
				formulaAttribute = attribute as FormulaAttribute ?? formulaAttribute;
				foreignKeyAttribute = attribute as ForeignKeyAttribute ?? foreignKeyAttribute;
				primaryKeyAttribute = attribute as PrimaryKeyAttribute ?? primaryKeyAttribute;
				columnAttribute = attribute as ColumnAttribute ?? columnAttribute;
			}

			if (entityAttribute != null)
			{
				EntityAttributes.Add(entityAttribute);
			}

			if (formulaAttribute != null)
			{
				FormulaAttributes.Add(formulaAttribute);
			}

			if (columnAttribute != null)
			{
				if (!ColumnAttributes.Contains(columnAttribute))
				{
					ColumnAttributes.Add(columnAttribute);
				}

				if (foreignKeyAttribute != null)
				{
					ColumnForeignKeys[columnAttribute] = foreignKeyAttribute;
				}

				if (primaryKeyAttribute != null)
				{
					ColumnPrimaryKeys[columnAttribute] = primaryKeyAttribute;
				}
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
