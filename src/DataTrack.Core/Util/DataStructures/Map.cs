using System;
using System.Collections.Generic;

namespace DataTrack.Core.Util.DataStructures
{
	public class Map<T1, T2>
	{
		private readonly Dictionary<T1, T2> forward = new Dictionary<T1, T2>();
		private readonly Dictionary<T2, T1> reverse = new Dictionary<T2, T1>();

		public Dictionary<T1, T2>.KeyCollection ForwardKeys => forward.Keys;
		public Dictionary<T2, T1>.KeyCollection ReverseKeys => reverse.Keys;

		public T2 this[T1 item]
		{
			get => forward.ContainsKey(item)
				? forward[item]
				: throw new ArgumentOutOfRangeException($"Mapping does not contain key '{item?.ToString()}'");
			set
			{
				if (reverse.ContainsKey(value))
				{
					throw new ArgumentOutOfRangeException($"Mapping already contains key '{value?.ToString()}'");
				}
				else
				{
					forward[item] = value;
					reverse[value] = item;
				}
			}
		}

		public T1 this[T2 item]
		{
			get => reverse.ContainsKey(item)
				? reverse[item]
				: throw new ArgumentOutOfRangeException($"Mapping does not contain key '{item?.ToString()}'");

			set
			{
				if (forward.ContainsKey(value))
				{
					throw new ArgumentOutOfRangeException($"Mapping already contains key '{value?.ToString()}'");
				}
				else
				{
					reverse[item] = value;
					forward[value] = item;
				}
			}
		}

		public bool ContainsKey(T1 item)
		{
			return forward.ContainsKey(item);
		}

		public bool ContainsKey(T2 item)
		{
			return reverse.ContainsKey(item);
		}
	}
}
