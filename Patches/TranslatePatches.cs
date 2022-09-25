using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static class TranslatePatches
    {
        private static Dictionary<string, StringProtoJson> _stringProtoJsons;

        private static Dictionary<string, StringProtoJson> StringProtoJsons
        {
            get
            {
                if (_stringProtoJsons != null) return _stringProtoJsons;
                _stringProtoJsons = JsonHelper.StringProtos().ToDictionary(i => i.Name);
                return _stringProtoJsons;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StringTranslate), "Translate")]
        public static void StringTranslate_Translate(string s, ref string __result)
        {
            if (s == null) return;

            if (StringProtoJsons.TryGetValue(s, out var stringProtoJson))
                switch (Localization.language)
                {
                    case Language.zhCN:
                        __result = stringProtoJson.ZHCN;
                        break;

                    case Language.enUS:
                        __result = stringProtoJson.ENUS;
                        break;
                }
        }

        public static string TranslateFromJson(this string s)
        {
            if (s == null) return "";

            if (StringProtoJsons.TryGetValue(s, out var stringProtoJson))
                switch (Localization.language)
                {
                    case Language.zhCN:
                        return stringProtoJson.ZHCN;

                    case Language.enUS:
                        return stringProtoJson.ENUS;
                }

            return s;
        }
    }
}
