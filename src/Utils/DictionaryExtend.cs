using System.Collections.Generic;

// ReSharper disable RemoveRedundantBraces

namespace ProjectGenesis.Utils
{
    internal static class DictionaryExtend
    {
        public static void TryAddOrInsert<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(value);
            }
            else
            {
                dict[key] = new List<TValue> { value };
            }
        }
    }
}
