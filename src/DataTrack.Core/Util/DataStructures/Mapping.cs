using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Util.DataStructures
{
    public class Mapping<T1, T2>
    {
        private Dictionary<T1, T2> forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> reverse = new Dictionary<T2, T1>();

        public T2 this[T1 item]
        {
            get => forward.ContainsKey(item) 
                ? forward[item] 
                : throw new ArgumentOutOfRangeException($"Mapping does not contain key '{item.ToString()}'");
            set
            {
                if (reverse.ContainsKey(value))
                    throw new ArgumentOutOfRangeException($"Mapping already contains key '{value.ToString()}'");
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
                : throw new ArgumentOutOfRangeException($"Mapping does not contain key '{item.ToString()}'");

            set
            {
                if (forward.ContainsKey(value))
                    throw new ArgumentOutOfRangeException($"Mapping already contains key '{value.ToString()}'");
                else
                {
                    reverse[item] = value;
                    forward[value] = item;
                }
            }
        }

        public bool ContainsKey(T1 item) => forward.ContainsKey(item);

        public bool ContainsKey(T2 item) => reverse.ContainsKey(item);

    }
}
