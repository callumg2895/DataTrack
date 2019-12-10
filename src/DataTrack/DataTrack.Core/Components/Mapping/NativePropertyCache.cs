using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	public static class NativePropertyCache
	{
		private static Cache<Type, Dictionary<string, PropertyInfo>> cache;

		public static void Init(int cacheSizeLimit, LogConfiguration config)
		{
			cache = new Cache<Type, Dictionary<string, PropertyInfo>>(cacheSizeLimit, "Native Property Cache", config);
		}

		public static void CacheItem(Type type, Dictionary<string, PropertyInfo> properties)
		{
			cache.CacheItem(type, properties);
		}

		public static Dictionary<string, PropertyInfo> RetrieveItem(Type type)
		{
			return cache.RetrieveItem(type);
		}

		public static void Stop()
		{
			cache.Stop();
		}
	}
}
