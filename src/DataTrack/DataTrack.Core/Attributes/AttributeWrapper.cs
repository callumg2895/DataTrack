using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Attributes
{
	public class AttributeWrapper
	{
		public TableAttribute? TableAttribute { get; private set; }
		public List<ColumnAttribute> ColumnAttributes { get; private set; }
		public Dictionary<ColumnAttribute, ForeignKeyAttribute> ColumnForeignKeys { get; private set; }
		public Dictionary<ColumnAttribute, PrimaryKeyAttribute> ColumnPrimaryKeys { get; private set; }

		public AttributeWrapper(Type type)
		{
			TableAttribute = null;
			ColumnAttributes = new List<ColumnAttribute>();
			ColumnForeignKeys = new Dictionary<ColumnAttribute, ForeignKeyAttribute>();
			ColumnPrimaryKeys = new Dictionary<ColumnAttribute, PrimaryKeyAttribute>();

			Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity");

			foreach (Attribute attribute in type.GetCustomAttributes())
			{
				TableAttribute = attribute as TableAttribute;
			}

			if (TableAttribute == null)
			{
				throw new NullReferenceException($"Could not find TableMappingAttribute for type {type.Name}");
			}

			foreach (PropertyInfo property in type.GetProperties())
			{
				ForeignKeyAttribute? foreignKeyAttribute = null;
				PrimaryKeyAttribute? primaryKeyAttribute = null;
				ColumnAttribute? columnAttribute = null;

				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					foreignKeyAttribute = attribute as ForeignKeyAttribute ?? foreignKeyAttribute;
					primaryKeyAttribute = attribute as PrimaryKeyAttribute ?? primaryKeyAttribute;
					columnAttribute = attribute as ColumnAttribute ?? columnAttribute;
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
		}

		internal bool IsValid()
		{
			return TableAttribute != null && ColumnAttributes.Count > 0;
		}
	}
}
