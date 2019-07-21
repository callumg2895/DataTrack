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
		public List<ColumnAttribute> ColumnAttributes { get; private set; }
		public Dictionary<ColumnAttribute, ForeignKeyAttribute> ColumnForeignKeys { get; private set; }
		public Dictionary<ColumnAttribute, PrimaryKeyAttribute> ColumnPrimaryKeys { get; private set; }
		public MappingTypes MappingType { get; set; }

		private Type entityType;

		public AttributeWrapper(Type type)
		{
			TableAttribute = null;
			ColumnAttributes = new List<ColumnAttribute>();
			EntityAttributes = new List<EntityAttribute>();
			ColumnForeignKeys = new Dictionary<ColumnAttribute, ForeignKeyAttribute>();
			ColumnPrimaryKeys = new Dictionary<ColumnAttribute, PrimaryKeyAttribute>();

			entityType = type;

			Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity");

			foreach (Attribute attribute in type.GetCustomAttributes())
			{
				TableAttribute = attribute as TableAttribute;
			}

			foreach (PropertyInfo property in type.GetProperties())
			{
				LoadAttributes(property);
			}

			MappingType = GetMappingType();
		}

		internal bool IsValid()
		{
			return TableAttribute != null && ColumnAttributes.Count > 0;
		}

		private void LoadAttributes(PropertyInfo property)
		{
			EntityAttribute? entityAttribute = null;
			ForeignKeyAttribute? foreignKeyAttribute = null;
			PrimaryKeyAttribute? primaryKeyAttribute = null;
			ColumnAttribute? columnAttribute = null;

			foreach (Attribute attribute in property.GetCustomAttributes())
			{
				entityAttribute = attribute as EntityAttribute ?? entityAttribute;
				foreignKeyAttribute = attribute as ForeignKeyAttribute ?? foreignKeyAttribute;
				primaryKeyAttribute = attribute as PrimaryKeyAttribute ?? primaryKeyAttribute;
				columnAttribute = attribute as ColumnAttribute ?? columnAttribute;
			}

			if (entityAttribute != null)
			{
				EntityAttributes.Add(entityAttribute);
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

		private MappingTypes GetMappingType()
		{
			if (TableAttribute == null && ColumnAttributes.Count == 0 && EntityAttributes.Count == 0)
			{
				throw new MappingException(entityType);
			}

			if (TableAttribute != null && EntityAttributes.Count > 0)
			{
				throw new MappingException($"Cannot define both Table and Entity based mapping for {entityType.Name}");
			}

			if (EntityAttributes.Count > 0)
			{
				return MappingTypes.EntityBased;
			}

			if (TableAttribute != null && ColumnAttributes.Count == 0)
			{
				throw new MappingException($"Cannot find column mappings for {entityType.Name}");
			}

			return MappingTypes.TableBased;
		}
	}
}
