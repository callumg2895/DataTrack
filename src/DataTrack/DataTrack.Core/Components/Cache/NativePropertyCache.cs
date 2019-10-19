using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Cache
{
	public sealed class NativePropertyCache : Cache<Type, Dictionary<string, PropertyInfo>>
	{
		private static NativePropertyCache instance = null;
		private static readonly object instanceLock = new object();

		public static NativePropertyCache Instance
		{
			get
			{
				lock (instanceLock)
				{
					return instance;
				}
			}
		}

		private NativePropertyCache(int cacheSizeLimit, string cacheName, LogConfiguration config)
			: base(cacheSizeLimit, cacheName, config)
		{

		}

		public static void Init(int cacheSizeLimit, LogConfiguration config)
		{
			lock (instanceLock)
			{
				if (instance == null)
				{
					instance = new NativePropertyCache(cacheSizeLimit, "NativePropertyCache", config);
				}
			}
		}
	}
}
