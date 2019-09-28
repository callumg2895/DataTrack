using DataTrack.Core.Enums;
using DataTrack.Core.Components.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DataTrack.Logging;
using System.Reflection;
using System.Linq;
using System.Data.SqlClient;
using DataTrack.Core.Interface;

namespace DataTrack.Core.Components.Query
{
	public abstract class Query : IQuery
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		protected readonly Type baseType;
		protected readonly Stopwatch stopwatch;

		public CRUDOperationTypes OperationType { get; set; }
		public string QueryString { get; set; }
		internal Mapping.Mapping Mapping { get; set; }

		internal Query(Type type, Mapping.Mapping mapping)
		{
			OperationType = CRUDOperationTypes.Read;
			QueryString = string.Empty;
			baseType = type;
			stopwatch = new Stopwatch();
			Mapping = mapping;
		}

		public abstract IQuery AddRestriction(string property, RestrictionTypes type, object value);

		public abstract dynamic Execute();

		public abstract dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null);

		public List<Parameter> GetParameters()
		{
			List<Parameter> parameters = new List<Parameter>();

			foreach (EntityTable table in Mapping.Tables)
			{
				foreach (Column column in table.Columns)
				{
					parameters.AddRange(column.Parameters);
				}
			}

			return parameters;
		}

		public abstract override string ToString();

		private protected void ValidateMapping(Mapping.Mapping mapping)
		{
			// Check for valid Table/Columns
			if (mapping.Tables.Count == 0 || mapping.Tables.Any(t => t.Columns.Count == 0))
			{
				string message = $"Mapping data for class '{baseType.Name}' was incomplete/empty";
				Logger.Error(MethodBase.GetCurrentMethod(), message);
				throw new Exception(message);
			}
		}
	}
}
