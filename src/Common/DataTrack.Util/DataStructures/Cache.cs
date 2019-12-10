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
		private Logger logger;
		private Dictionary<TKey, TValue> cacheDictionary;
		private Dictionary<TKey, long> cacheAccessMapping;
		private volatile bool shouldCull;
		private volatile bool typeMappingInUse;

		private readonly object cacheLock = new object();
		private readonly object shouldCullLock = new object();
		private readonly object cacheInUseLock = new object();

		private readonly int maxCachedItems;

		private Thread cullingThread;

		private string cacheName;

		protected Cache(int cacheSizeLimit, string name, LogConfiguration config)
		{
			logger = new Logger(config);
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
			lock (cacheInUseLock)
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
			while (ShouldCull())
			{
				Thread.Sleep(100);

				int cacheSize = GetCurrentCacheSize();

				logger.Trace($"{cacheName} Beginning cache culling cycle - current cache size: {cacheSize} item(s)");

				lock (cacheInUseLock)
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
			lock (cacheLock)
			{
				return cacheDictionary.Count;
			}
		}

		private void CullItem()
		{
			long oldestAccess = long.MaxValue;
			TKey candidate = default;
			Dictionary<TKey, long> tempCacheAccessMapping;

			lock (cacheLock)
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

			if (candidate == null)
			{
				logger.Warn($"{cacheName} Attempting to cull key will null value.");
				return;
			}
			
			EvictItem(candidate);
		}

		public virtual void CacheItem(TKey key, TValue value)
		{
			lock (cacheLock)
			{
				logger.Trace($"{cacheName} Caching value '{value.ToString()}' for key '{key.ToString()}'");
				cacheDictionary[key] = value;
				cacheAccessMapping[key] = DateTime.UtcNow.ToFileTime();
			}
		}

		public virtual TValue RetrieveItem(TKey key)
		{
			TValue value = default(TValue);

			lock (cacheLock)
			{
				if (cacheDictionary.ContainsKey(key) && cacheAccessMapping.ContainsKey(key))
				{
					value = cacheDictionary[key];
					cacheAccessMapping[key] = DateTime.UtcNow.ToFileTime();

					logger.Trace($"{cacheName} Retrieved value '{value.ToString()}' for key '{key.ToString()}' from cache");
				}
			}

			return value;
		}

		public virtual void EvictItem(TKey key)
		{
			lock (cacheLock)
			{
				cacheDictionary.Remove(key);
				cacheAccessMapping.Remove(key);
				logger.Debug($"{cacheName} Evicted values for key '{key.ToString()}'");
			}
		}

		public void Stop()
		{
			EndExecution();
		}
	}
}
