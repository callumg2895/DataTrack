using DataTrack.Core.Components.Data;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.Components.Cache
{
	public sealed class MappingCache : Cache<Type, EntityTable>
	{
		private static MappingCache instance = null;
		private static readonly object instanceLock = new object();

		public static MappingCache Instance
		{
			get
			{
				lock (instanceLock)
				{
					return instance;
				}
			}
		}

		private MappingCache(int cacheSizeLimit, string cacheName, LogConfiguration config)
			: base(cacheSizeLimit, cacheName, config)
		{

		}

		public static void Init(int cacheSizeLimit, LogConfiguration config)
		{
			lock (instanceLock)
			{
				if (instance == null)
				{
					instance = new MappingCache(cacheSizeLimit, "MappingCache", config);
				}
			}
		}
	}
}
