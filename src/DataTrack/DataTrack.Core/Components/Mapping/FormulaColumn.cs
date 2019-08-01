using System;
using System.Collections.Generic;
using System.Text;

using DataTrack.Core.Attributes;
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
	public class FormulaColumn : Column, ICloneable
	{
		public FormulaColumn(FormulaAttribute formulaAttribute, EntityTable table)
			: base(table, formulaAttribute.Alias)
		{
			FormulaAttribute = formulaAttribute;
			Alias = Name;
			Formula = formulaAttribute.Query;
			PropertyName = GetPropertyName();

			Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
		}

		public string Formula { get; set; }

		private readonly FormulaAttribute FormulaAttribute;

		private string GetPropertyName()
		{
			Type type = Table.Type;

			// Try to find the property with a ColumnMappingAttribute that matches the one in the method call
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as FormulaAttribute)?.Alias == Name)
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
					if ((attribute as FormulaAttribute)?.Alias == Name)
					{
						return Parameter.SQLDataTypes[property.PropertyType];
					}
				}
			}
			// Technically the wrong exception to throw. The problem here is that the 'type' supplied
			// does not contain a property with ColumnMappingAttribute with a matching column name.
			throw new ColumnMappingException(type, Name);
		}

		public override object Clone()
		{
			Logger.Trace($"Cloning column mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Table '{Table.Name}')");
			return new FormulaColumn(FormulaAttribute, Table);
		}

		public override string GetSelectString()
		{
			return $"{Formula} as {Alias}";
		}

		public override bool Equals(object obj)
		{
			if ((obj as FormulaColumn) == null)
			{
				return false;
			}

			FormulaColumn column = (FormulaColumn)obj;

			return column.Name == Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}

