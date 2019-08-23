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

		private string cacheName;

		public Cache(int cacheSizeLimit, string name)
		{
			cacheDictionary = new Dictionary<TKey, TValue>();
			cacheAccessMapping = new Dictionary<TKey, long>();

			maxCachedItems = cacheSizeLimit;

			shouldCull = true;
			typeMappingInUse = true;

			cullingThread = new Thread(new ThreadStart(Culling));
			cullingThread.Start();

			cacheName = $"({name})";
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

				Logger.Debug($"{cacheName} Beginning cache culling cycle - current cache size: {cacheSize} item(s)");

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
			long oldestAccess = long.MaxValue;
			TKey candidate = default;
			Dictionary<TKey, long> tempCacheAccessMapping;

			lock (typeMappingLock)
			{
				tempCacheAccessMapping = cacheAccessMapping;
			}

			foreach (TKey key in tempCacheAccessMapping.Keys)
			{
				if (tempCacheAccessMapping[key] >= oldestAccess)
				{
					continue;
				}
				else
				{
					oldestAccess = tempCacheAccessMapping[key];
					candidate = key;
				}
			}

			lock (typeMappingLock)
			{
				if (candidate != null && cacheDictionary.ContainsKey(candidate))
				{
					cacheDictionary.Remove(candidate);
					cacheAccessMapping.Remove(candidate);
					Logger.Debug($"{cacheName} Culled values for key {candidate.ToString()}");
				}
			}
		}

		public void CacheItem(TKey key, TValue value)
		{
			lock (typeMappingLock)
			{
				Logger.Trace($"{cacheName} Caching value {value.ToString()} for key '{key.ToString()}'");
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

					Logger.Trace($"{cacheName} Retrieved value {value.ToString()} for key '{key.ToString()}' from cache");
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
