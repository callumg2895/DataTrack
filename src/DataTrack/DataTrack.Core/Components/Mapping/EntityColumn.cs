using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	public class EntityColumn : Column
	{
		public EntityColumn(ColumnAttribute columnAttribute, EntityTable table)
			: base(table, columnAttribute.ColumnName)
		{
			ColumnAttribute = columnAttribute;

			PropertyName = GetPropertyName();

			Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
		}

		public byte KeyType { get; set; }
		public string? ForeignKeyTableMapping { get; set; }

		private readonly ColumnAttribute ColumnAttribute;

		private string GetPropertyName()
		{
			Type type = Table.Type;

			// Try to find the property with a ColumnMappingAttribute that matches the one in the method call
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as ColumnAttribute)?.ColumnName == Name)
					{
						return property.Name;
					}
				}
			}

			throw new ColumnMappingException(type, Name);
		}

		public override SqlDbType GetSqlDbType()
		{
			Type type = Table.Type;

			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as ColumnAttribute)?.ColumnName == Name)
					{
						return Parameter.SQLDataTypes[property.PropertyType];
					}
				}
			}
			// Technically the wrong exception to throw. The problem here is that the 'type' supplied
			// does not contain a property with ColumnMappingAttribute with a matching column name.
			throw new ColumnMappingException(type, Name);
		}

		public bool IsForeignKey()
		{
			return (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;
		}

		public bool IsPrimaryKey()
		{
			return (KeyType & (byte)KeyTypes.PrimaryKey) == (byte)KeyTypes.PrimaryKey;
		}

		public override object Clone()
		{
			Logger.Trace($"Cloning column mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Table '{Table.Name}')");
			return new EntityColumn(ColumnAttribute, Table);
		}

		public override string GetSelectString()
		{
			return Alias;
		}

		public override bool Equals(object obj)
		{
			if ((obj as EntityColumn) == null)
			{
				return false;
			}

			EntityColumn column = (EntityColumn)obj;

			return column.Name == Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}
