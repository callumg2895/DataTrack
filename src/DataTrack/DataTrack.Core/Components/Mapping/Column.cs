﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Mapping
{
	public class Column : ICloneable
	{
		public Column(ColumnAttribute columnAttribute, EntityTable table)
		{
			ColumnAttribute = columnAttribute;

			Table = table;
			Restrictions = new List<Restriction>();
			Parameters = new List<Parameter>();
			Name = columnAttribute.ColumnName;
			Alias = $"{table.Type.Name}.{Name}";
			PropertyName = GetPropertyName();

			Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
		}

		public EntityTable Table { get; set; }
		public List<Restriction> Restrictions { get; set; }
		public List<Parameter> Parameters { get; set; }
		public string Name { get; set; }
		public string Alias { get; set; }
		public string PropertyName { get; set; }
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

		public SqlDbType GetSqlDbType()
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

		public void AddRestriction(RestrictionTypes type, object value)
		{
			Parameter parameter = new Parameter(this, value);
			Restriction restriction = new Restriction(this, parameter, type);

			Parameters.Add(parameter);
			Restrictions.Add(restriction);
		}

		public void AddParameter(object value)
		{
			Parameter parameter = new Parameter(this, value);
			Parameters.Add(parameter);
		}

		public object Clone()
		{
			Logger.Trace($"Cloning column mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Table '{Table.Name}')");
			return new Column(ColumnAttribute, Table);
		}

		public override bool Equals(object obj)
		{
			if ((obj as Column) == null)
			{
				return false;
			}

			Column column = (Column)obj;

			return column.Name == Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}