using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Exceptions
{
	public class MappingException : Exception
	{
		private static Logger Logger = DataTrackConfiguration.Logger;

		public MappingException(Type type)
			: this($"Could not find mapping information for {type.Name}")
		{
			Logger.ErrorFatal(TargetSite, Message);
		}

		public MappingException(string message)
			: base(message)
		{
			Logger.ErrorFatal(TargetSite, Message);
		}
	}
}
