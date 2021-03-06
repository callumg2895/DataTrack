﻿using DataTrack.Core.Attributes;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Exceptions;
using DataTrack.Logging;
using System;
using System.Data;
using System.Reflection;

namespace DataTrack.Core.Components.Data
{
	public class EntityColumn : Column
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public EntityColumn(ColumnAttribute columnAttribute, EntityTable table)
			: base(table, columnAttribute.ColumnName)
		{
			ColumnAttribute = columnAttribute;
			ColumnType = ColumnTypes.EntityColumn;
			DataColumn = new DataColumn(Name);

			GetPropertyInfo();

			Logger.Trace($"Loaded database mapping for Property '{PropertyName}' of Entity '{Table.Type.Name}' (Column '{Name}')");
		}

		public byte KeyType { get; set; }
		public string? ForeignKeyTableMapping { get; set; }
		public override ColumnTypes ColumnType { get; set; }
		public DataColumn DataColumn { get; set; }

		private readonly ColumnAttribute ColumnAttribute;

		protected override void GetPropertyInfo()
		{
			Type type = Table.Type;

			// Try to find the property with a ColumnMappingAttribute that matches the one in the method call
			foreach (PropertyInfo property in type.GetProperties())
			{
				foreach (Attribute attribute in property.GetCustomAttributes())
				{
					if ((attribute as ColumnAttribute)?.ColumnName == Name)
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

		public override bool IsForeignKey()
		{
			return (KeyType & (byte)KeyTypes.ForeignKey) == (byte)KeyTypes.ForeignKey;
		}

		public override bool IsPrimaryKey()
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

		public void UpdateDataRow(DataRow dataRow, object data)
		{
			if (IsPrimaryKey())
			{
				Logger.Trace($"Data for column '{Name}' was skipped due to being a primary key");
				return;
			}

			dataRow[Name] = data;
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
