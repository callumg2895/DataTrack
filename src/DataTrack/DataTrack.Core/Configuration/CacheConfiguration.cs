using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DataTrack.Core.Configuration
{
	internal class CacheConfiguration
	{
		internal CacheConfiguration(XmlNode cacheNode)
		{
			XmlNode xmlNodeMaxCacheSize = cacheNode.SelectSingleNode("MaxCacheSize");

			CacheSizeLimit = int.Parse(xmlNodeMaxCacheSize.InnerText);
		}

		internal int CacheSizeLimit { get; set; }
	}
}
