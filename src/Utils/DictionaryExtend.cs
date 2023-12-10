using System.Collections.Generic;

namespace ProjectGenesis.Utils
{
    internal static class DictionaryExtend
    {
        public static bool TryAddOrInsert<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key)) return dict.TryAdd(key, new List<TValue>() { value });

            dict[key].Add(value);
            return true;
        }
    }
}
