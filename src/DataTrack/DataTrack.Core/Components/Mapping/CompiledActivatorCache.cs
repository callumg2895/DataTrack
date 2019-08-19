using DataTrack.Core.Interface;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	public static class CompiledActivatorCache
	{
		private static Cache<Type, Func<object>> cache;

		public static void Init(int cacheSizeLimit)
		{
			cache = new Cache<Type, Func<object>>(cacheSizeLimit, "CompiledActivatorCache");
		}

		public static void CacheItem(Type type, Func<object> activator)
		{
			cache.CacheItem(type, activator);
		}

		public static Func<object> RetrieveItem(Type type)
		{
			return cache.RetrieveItem(type);
		}

		public static void Stop()
		{
			cache.Stop();
		}

	}
}
