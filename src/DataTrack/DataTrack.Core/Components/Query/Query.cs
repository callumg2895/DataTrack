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

namespace DataTrack.Core.Components.Query
{
	public abstract class Query
	{
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

		public abstract dynamic Execute();

		internal abstract dynamic Execute(SqlCommand command, SqlConnection connection, SqlTransaction? transaction = null);

		public abstract override string ToString();
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
