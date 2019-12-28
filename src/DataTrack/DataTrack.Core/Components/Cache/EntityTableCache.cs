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
	public sealed class EntityTableCache : Cache<Type, EntityTable>
	{
		private static EntityTableCache instance = null;
		private static readonly object instanceLock = new object();

		public static EntityTableCache Instance
		{
			get
			{
				lock (instanceLock)
				{
					return instance;
				}
			}
		}

		private EntityTableCache(int cacheSizeLimit, string cacheName, LogConfiguration config)
			: base(cacheSizeLimit, cacheName, config)
		{

		}

		public static void Init(int cacheSizeLimit, LogConfiguration config)
		{
			lock (instanceLock)
			{
				if (instance == null)
				{
					instance = new EntityTableCache(cacheSizeLimit, "MappingCache", config);
				}
			}
		}
	}
}
