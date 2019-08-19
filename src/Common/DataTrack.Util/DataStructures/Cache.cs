using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTrack.Util.DataStructures
{
	public class Cache<TKey, TValue>
	{
		private Dictionary<TKey, TValue> cacheDictionary;
		private Dictionary<TKey, long> cacheAccessMapping;
		private volatile bool shouldCull;
		private volatile bool typeMappingInUse;

		private readonly object typeMappingLock = new object();
		private readonly object shouldCullLock = new object();
		private readonly object typeMappingInUseLock = new object();

		private readonly int maxCachedItems;

		private Thread cullingThread;

		public Cache(int cacheSizeLimit)
		{
			cacheDictionary = new Dictionary<TKey, TValue>();
			cacheAccessMapping = new Dictionary<TKey, long>();

			maxCachedItems = cacheSizeLimit;

			shouldCull = true;
			typeMappingInUse = true;

			cullingThread = new Thread(new ThreadStart(Culling));
			cullingThread.Start();
		}

		private bool ShouldCull()
		{
			lock (shouldCullLock)
			{
				return shouldCull;
			}
		}

		private bool CullingInProgress()
		{
			lock (typeMappingInUseLock)
			{
				return typeMappingInUse;
			}
		}

		private void EndExecution()
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

		private void Culling()
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

		private int GetCurrentCacheSize()
		{
			lock (typeMappingLock)
			{
				return cacheDictionary.Count;
			}
		}

		private void CullItem()
		{
			lock (typeMappingLock)
			{
				long oldestAccess = long.MaxValue;
				TKey candidate = default;

				foreach (TKey key in cacheAccessMapping.Keys)
				{
					if (cacheAccessMapping[key] >= oldestAccess)
					{
						continue;
					}
					else
					{
						oldestAccess = cacheAccessMapping[key];
						candidate = key;
					}
				}

				if (candidate != null && cacheDictionary.ContainsKey(candidate))
				{
					cacheDictionary.Remove(candidate);
					cacheAccessMapping.Remove(candidate);
					Logger.Info(MethodBase.GetCurrentMethod(), $"Culled values for key {candidate.ToString()}");
				}
			}
		}

		public void CacheItem(TKey key, TValue value)
		{
			lock (typeMappingLock)
			{
				Logger.Info(MethodBase.GetCurrentMethod(), $"Caching value {value.ToString()} for key '{key.ToString()}'");
				cacheDictionary[key] = value;
				cacheAccessMapping[key] = DateTime.UtcNow.ToFileTime();
			}
		}

		public TValue RetrieveItem(TKey key)
		{
			TValue value = default(TValue);

			lock (typeMappingLock)
			{
				if (cacheDictionary.ContainsKey(key) && cacheAccessMapping.ContainsKey(key))
				{
					value = cacheDictionary[key];
					cacheAccessMapping[key] = DateTime.UtcNow.ToFileTime();

					Logger.Info(MethodBase.GetCurrentMethod(), $"Retrieved value {value.ToString()} for key '{key.ToString()}' from cache");
				}
			}

			return value;
		}

		public void Stop()
		{
			EndExecution();
		}
	}
}
