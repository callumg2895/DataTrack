using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Core.Components.Mapping
{
	public static class MappingCache
	{

		private static Dictionary<Type, EntityTable> TypeTableMapping;
		private static Dictionary<Type, long> TypeAccessMapping;
		private static volatile bool shouldCull;
		private static volatile bool typeMappingInUse;

		private static readonly object typeMappingLock = new object();
		private static readonly object shouldCullLock = new object();
		private static readonly object typeMappingInUseLock = new object();

		private static int maxCachedItems = 10;

		private static Thread cullingThread;

		public static void Init()
		{
			TypeTableMapping = new Dictionary<Type, EntityTable>();
			TypeAccessMapping = new Dictionary<Type, long>();

			shouldCull = true;
			typeMappingInUse = true;

			cullingThread = new Thread(new ThreadStart(Culling));
			cullingThread.Start();
		}

		private static bool ShouldCull()
		{
			lock (shouldCullLock)
			{
				return shouldCull;
			}
		}

		private static bool CullingInProgress()
		{
			lock (typeMappingInUseLock)
			{
				return typeMappingInUse;
			}
		}

		private static void EndExecution()
		{
			while (CullingInProgress())
			{
				continue;
			}

			lock (shouldCullLock)
			{
				shouldCull = false;
			}
		}

		public static void Culling()
		{
			while (!Logger.IsStarted())
			{

			}

			while (ShouldCull())
			{
				Thread.Sleep(100);

				int cacheSize = GetCurrentCacheSize();

				Logger.Debug(MethodBase.GetCurrentMethod(), $"Beginning cache culling cycle - current cache size: {cacheSize} item(s)");

				lock (typeMappingInUseLock)
				{
					typeMappingInUse = cacheSize > maxCachedItems;
				}

				if (cacheSize > maxCachedItems)
				{
					CullItem();
				}
			}
		}

		private static int GetCurrentCacheSize()
		{
			lock (typeMappingLock)
			{
				return TypeTableMapping.Count;
			}
		}

		private static void CullItem()
		{
			lock (typeMappingLock)
			{
				int? minAccessCount = null;
				Type? candidate = null;

				foreach (Type key in TypeAccessMapping.Keys)
				{
					if (minAccessCount.HasValue && TypeAccessMapping[key] >= minAccessCount.Value)
					{
						continue;
					}
					else
					{
						minAccessCount = (int?)TypeAccessMapping[key];
						candidate = key;
					}
				}

				if (candidate != null)
				{
					TypeTableMapping.Remove(candidate);
					TypeAccessMapping.Remove(candidate);
					Logger.Info(MethodBase.GetCurrentMethod(), $"Culled mapping for {candidate.Name}");
				}
			}
		}

		public static void CacheItem(Type type, EntityTable table)
		{
			lock (typeMappingLock)
			{
				Logger.Info(MethodBase.GetCurrentMethod(), $"Caching database mapping for Entity '{type.Name}'");
				TypeTableMapping[type] = table;
				TypeAccessMapping[type] = 1;		
			}
		}

		public static EntityTable? RetrieveItem(Type type)
		{
			EntityTable? table = null;

			lock (typeMappingLock)
			{
				if (TypeTableMapping.ContainsKey(type) && TypeAccessMapping.ContainsKey(type))
				{
					Logger.Info(MethodBase.GetCurrentMethod(), $"Loading Table object for '{type.Name}' entity from cache");
					table = TypeTableMapping[type];
					TypeAccessMapping[type] += 1;
				}				
			}

			return table;
		}

		public static void Stop()
		{
			EndExecution();
		}
	}
}
