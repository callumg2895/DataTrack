using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Util.Extensions
{
    public static class CollectionExtension
    {
        public static void ForEach<T>(this ICollection<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
