using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Cache
{
	public sealed class CompiledActivatorCache : Cache<Type, Func<object>>
	{
		private static CompiledActivatorCache instance = null;
		private static readonly object instanceLock = new object();

		public static CompiledActivatorCache Instance
		{
			get
			{
				lock (instanceLock)
				{
					return instance;
				}
			}
		}

		private CompiledActivatorCache(int cacheSizeLimit, string cacheName, LogConfiguration config)
			: base(cacheSizeLimit, cacheName, config)
		{

		}

		public static void Init(int cacheSizeLimit, LogConfiguration config)
		{
			instance = new CompiledActivatorCache(cacheSizeLimit, "CompiledActivatorCache", config);
		}
	}
}
