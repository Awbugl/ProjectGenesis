using System.Collections.Generic;
using System.Linq;
using CommonAPI.Systems.ModLocalization;
using static ProjectGenesis.Utils.JsonHelper;

namespace ProjectGenesis.Utils
{
    internal static class TranslateUtils
    {
        private static readonly Dictionary<string, StringProtoJson> StringProtoJsons, StringModProtoJsons;
        
        static TranslateUtils()
        {
            StringProtoJsons = StringProtos().ToDictionary(i => i.Name);

            StringModProtoJsons = StringModProtos().ToDictionary(i => i.Name);

            RegisterStrings();
        }

        private static void RegisterStrings()
        {
            foreach ((string key, StringProtoJson value) in StringProtoJsons)
            {
                if (Localization.namesIndexer.ContainsKey(key))
                    LocalizationModule.EditTranslation(key, value.ENUS, value.ZHCN, "");
                else
                    LocalizationModule.RegisterTranslation(key, value.ENUS, value.ZHCN, "");
            }
        }

        public static string TranslateFromJson(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            if (!StringModProtoJsons.TryGetValue(s, out StringProtoJson stringProtoJson))
            {
                ProjectGenesis.logger.LogInfo("key:" + s + "translate not found");

                if (!StringProtoJsons.TryGetValue(s, out stringProtoJson)) return s;
            }

            if (Localization.isZHCN) return stringProtoJson.ZHCN;

            if (Localization.isENUS) return stringProtoJson.ENUS;

            return !Localization.namesIndexer.ContainsKey(s) ? s : Localization.currentStrings[Localization.namesIndexer[s]];
        }
    }
}
