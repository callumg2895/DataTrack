using System;
using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Mapping
{
	public class FormulaColumn : Column, ICloneable
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public FormulaColumn(FormulaAttribute formulaAttribute, EntityTable table)
			: base(table, formulaAttribute.Alias)
		{
			FormulaAttribute = formulaAttribute;
			Alias = Name;
			Formula = formulaAttribute.Query;
			ColumnType = ColumnTypes.FormulaColumn;

			GetPropertyInfo();

			Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
		}

		public string Formula { get; set; }
		public override ColumnTypes ColumnType { get; set; }

		private readonly FormulaAttribute FormulaAttribute;

		protected override void GetPropertyInfo()
		{
			Type type = Table.Type;

			// Try to find the property with a ColumnMappingAttribute that matches the one in the method call
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as FormulaAttribute)?.Alias == Name)
					{
						PropertyName = property.Name;
						PropertyType = property.PropertyType;
						return;
					}
				}
			}

			throw new ColumnMappingException(type, Name);
		}

		public override SqlDbType GetSqlDbType()
		{
			return Parameter.SQLDataTypes[PropertyType];
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

