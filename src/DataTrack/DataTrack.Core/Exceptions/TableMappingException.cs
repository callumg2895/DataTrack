using DataTrack.Logging;
using System;

namespace DataTrack.Core.Exceptions
{
	public class TableMappingException : MappingException
	{
		public TableMappingException(Type type, string tableName)
			: base($"No child property of type {type.Name} is mapped to table '{tableName}'")
		{

		}
	}
}
