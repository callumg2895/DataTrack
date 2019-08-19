using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	public static class NativePropertyCache
	{
		private static Cache<Type, List<PropertyInfo>> cache;

		public static void Init(int cacheSizeLimit)
		{
			cache = new Cache<Type, List<PropertyInfo>>(cacheSizeLimit, "Native Property Cache");
		}

		public static void CacheItem(Type type, List<PropertyInfo> properties)
		{
			cache.CacheItem(type, properties);
		}

		public static List<PropertyInfo> RetrieveItem(Type type)
		{
			return cache.RetrieveItem(type);
		}

		public static void Stop()
		{
			cache.Stop();
		}
	}
}
