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
	public abstract class Column : ICloneable
	{
		public EntityTable Table { get; set; }
		public List<Restriction> Restrictions { get; set; }
		public List<Parameter> Parameters { get; set; }
		public string Alias { get; set; }
		public string Name { get; set; }
		public string PropertyName { get; set; }
		public abstract ColumnTypes ColumnType { get; set; }

		public Column(EntityTable table, string name)
		{
			Table = table;
			Restrictions = new List<Restriction>();
			Parameters = new List<Parameter>();
			Name = name;
			Alias = $"{table.Type.Name}.{Name}";
			PropertyName = string.Empty;
		}

		public void AddParameter(object value)
		{
			Parameter parameter = new Parameter(this, value);
			Parameters.Add(parameter);
		}

		public void AddRestriction(RestrictionTypes type, object value)
		{
			Parameter parameter = new Parameter(this, value);
			Restriction restriction = new Restriction(this, parameter, type);

			Parameters.Add(parameter);
			Restrictions.Add(restriction);
		}

		public virtual bool IsForeignKey()
		{
			return false;
		}

		public virtual bool IsPrimaryKey()
		{
			return false;
		}

		public abstract SqlDbType GetSqlDbType();

		public abstract string GetSelectString();

		public abstract object Clone();

		public abstract override int GetHashCode();

		protected abstract string GetPropertyName();
	}
}
