using System.Collections.Concurrent;
using System.Collections.Generic;

// ReSharper disable RemoveRedundantBraces

namespace ProjectGenesis.Utils
{
    internal static class DictionaryExtend
    {
        public static void TryAddOrInsert<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key)) { dict[key].Add(value); }
            else { dict[key] = new List<TValue> { value, }; }
        }

        public static void TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key)) { dict[key].Remove(value); }
        }

        public static bool Contains<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> dict, TKey key, TValue value) =>
            dict.TryGetValue(key, out List<TValue> list) && list.Contains(value);
    }
}
