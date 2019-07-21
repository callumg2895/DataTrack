using DataTrack.Core.Enums;
using DataTrack.Core.Components.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DataTrack.Logging;
using System.Reflection;
using System.Linq;

namespace DataTrack.Core.Components.Query
{
	public abstract class Query
	{
		protected readonly Type baseType;
		protected readonly Stopwatch stopwatch;

		public CRUDOperationTypes OperationType { get; set; }
		public string QueryString { get; set; }

		public Query(Type type)
		{
			OperationType = CRUDOperationTypes.Read;
			QueryString = string.Empty;
			baseType = type;
			stopwatch = new Stopwatch();
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
