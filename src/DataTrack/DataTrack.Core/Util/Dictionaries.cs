using DataTrack.Core.Components.Mapping;
using System;
using System.Collections.Generic;

namespace DataTrack.Core.Util
{
	public static class Dictionaries
	{
		public static Dictionary<Type, EntityTable> TypeMappingCache = new Dictionary<Type, EntityTable>();
	}
}
