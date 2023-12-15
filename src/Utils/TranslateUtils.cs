using System;
using System.Collections.Generic;
using System.Linq;
using static ProjectGenesis.Utils.JsonHelper;

namespace ProjectGenesis.Utils
{
    internal static class TranslateUtils
    {
        private static readonly Dictionary<string, StringProtoJson> StringProtoJsons;

        private static readonly int ZHCNIndex, ENUSIndex;

        static TranslateUtils()
        {
            if (!Localization.Loaded) Localization.Load();

            int languageCount = Localization.LanguageCount;
            for (int index = 0; index < languageCount; ++index)
            {
                if (Localization.Languages[index].lcId == 2052) ZHCNIndex = index;

                if (Localization.Languages[index].lcId == 1033) ENUSIndex = index;
            }

            StringProtoJsons = StringProtos().ToDictionary(i => i.Name);

            RegisterStrings();
        }

        private static void RegisterStrings()
        {
            int newlength = Localization.namesIndexer.Count + StringProtoJsons.Count;

            for (int index = 0; index < Localization.strings.Length; index++)
            {
                Array.Resize(ref Localization.strings[index], newlength);
            }

            foreach ((string key, StringProtoJson value) in StringProtoJsons)
            {
                Localization.namesIndexer.TryAdd(key, Localization.namesIndexer.Count);
                int idx = Localization.namesIndexer[key];
                Localization.strings[ZHCNIndex][idx] = value.ZHCN;
                Localization.strings[ENUSIndex][idx] = value.ENUS;
            }
        }

        public static string TranslateFromJson(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            if (!StringProtoJsons.TryGetValue(s, out StringProtoJson stringProtoJson)) return s;


            if (Localization.isZHCN)
            {
                return stringProtoJson.ZHCN;
            }

            if (Localization.isENUS)
            {
                return stringProtoJson.ENUS;
            }

            return !Localization.namesIndexer.ContainsKey(s) ? s : Localization.currentStrings[Localization.namesIndexer[s]];
        }
    }
}
