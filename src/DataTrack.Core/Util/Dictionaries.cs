using DataTrack.Core.SQL.DataStructures;
using System;
using System.Collections.Generic;

namespace DataTrack.Core.Util
{
	public static class Dictionaries
	{
		public static Dictionary<Type, Table> TypeMappingCache = new Dictionary<Type, Table>();
	}
}
