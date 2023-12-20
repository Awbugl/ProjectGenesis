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
        }

        internal static void RegisterStrings()
        {
            foreach ((string key, StringProtoJson value) in StringProtoJsons)
            {
                LocalizationModule.RegisterTranslation(key, value.ENUS, value.ZHCN, "");
            }
        }

        public static string TranslateFromJson(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            if (!StringModProtoJsons.TryGetValue(s, out StringProtoJson stringProtoJson) && !StringProtoJsons.TryGetValue(s, out stringProtoJson))
                return s;

            return Localization.isZHCN ? stringProtoJson.ZHCN : stringProtoJson.ENUS;
        }
    }
}
