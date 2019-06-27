using DataTrack.Logging;
using System;

namespace DataTrack.Core.Exceptions
{
	public class ColumnMappingException : Exception
	{
		public ColumnMappingException(Type type, string columnName)
			: base($"No property of type {type.Name} is mapped to column '{columnName}'")
		{
			Logger.ErrorFatal(TargetSite, Message);
		}
	}
}
