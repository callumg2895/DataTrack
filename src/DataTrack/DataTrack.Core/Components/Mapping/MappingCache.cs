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
		private static Cache<Type, EntityTable> mappingCache;

		public static void Init(int cacheSizeLimit)
		{
			mappingCache = new Cache<Type, EntityTable>(cacheSizeLimit);
		}

		public static void CacheItem(Type type, EntityTable table)
		{
			mappingCache.CacheItem(type, table);
		}

		public static EntityTable RetrieveItem(Type type)
		{
			return mappingCache.RetrieveItem(type);
		}

		public static void Stop()
		{
			mappingCache.Stop();
		}
	}
}
