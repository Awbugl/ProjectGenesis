using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                _stringProtoJsons = JsonHelper.StringProtos().ToDictionary(i => i.Name);
                return _stringProtoJsons;
            }
        }

        public static string TranslateFromJson(this string s)
        {
            if (s == null) return "";

            if (StringProtoJsons.ContainsKey(s))
            {
                StringProtoJson stringProtoJson = StringProtoJsons[s];
                switch (Localization.language)
                {
                    case Language.zhCN:
                        return stringProtoJson.ZHCN;

                    case Language.enUS:
                        return stringProtoJson.ENUS;
                }
            }

            StringProtoSet strings = LDB.strings;
            if (strings == null) return s;

            StringProto stringProto = strings[s];
            if (stringProto == null) return s;
            switch (Localization.language)
            {
                case Language.zhCN:
                    return stringProto.ZHCN;

                case Language.enUS:
                    return stringProto.ENUS;

                case Language.frFR:
                    return stringProto.FRFR;

                default:
                    return s;
            }
        }
    }
}
