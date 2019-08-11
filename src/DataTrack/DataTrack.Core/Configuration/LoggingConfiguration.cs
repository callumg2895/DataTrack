using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Configuration
{
	internal class LoggingConfiguration
	{
		internal int MaxFileSize { get; set; }
		internal LogLevel LogLevel { get; set; }
	}
}
