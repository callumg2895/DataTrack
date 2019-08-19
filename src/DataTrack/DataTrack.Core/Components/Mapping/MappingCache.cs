using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.Components.Mapping
{
	public static class MappingCache
	{
		private static Cache<Type, EntityTable> cache;

		public static void Init(int cacheSizeLimit)
		{
			cache = new Cache<Type, EntityTable>(cacheSizeLimit, "MappingCache");
		}

		public static void CacheItem(Type type, EntityTable table)
		{
			cache.CacheItem(type, table);
		}

		public static EntityTable RetrieveItem(Type type)
		{
			return cache.RetrieveItem(type);
		}

		public static void Stop()
		{
			cache.Stop();
		}
	}
}
