using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProjectGenesis.Utils.JsonHelper;

namespace ProjectGenesis.Utils
{
    internal static class TranslateUtils
    {
        private static Dictionary<string, StringProtoJson> _stringProtoJsons;

        static TranslateUtils()
        {
            Localization.language = (Language)PlayerPrefs.GetInt("language", 0);
        }

        private static Dictionary<string, StringProtoJson> StringProtoJsons
        {
            get
            {
                if (_stringProtoJsons != null) return _stringProtoJsons;
                _stringProtoJsons = StringProtos().ToDictionary(i => i.Name);
                return _stringProtoJsons;
            }
        }

        public static string TranslateFromJson(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            if (!StringProtoJsons.TryGetValue(s, out StringProtoJson stringProtoJson)) return s;

            switch (Localization.language)
            {
                case Language.zhCN:
                    return stringProtoJson.ZHCN;

                case Language.enUS:
                    return stringProtoJson.ENUS;

                default:
                    return s;
            }
        }
    }
}
