using System;
using System.Collections.Generic;

namespace DataTrack.Util.Extensions
{
	public static class CollectionExtension
	{
		public static void ForEach<T>(this ICollection<T> collection, Action<T> action)
		{
			foreach (T item in collection)
			{
				action(item);
			}
		}
	}
}
