using System.Collections.Generic;
using System.Linq;

namespace ProjectGenesis
{
    internal static class TranslateUtils
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

        public static string TranslateFromJson(this string s)
        {
            if (s == null) return "";

            if (StringProtoJsons.ContainsKey(s))
            {
                var stringProtoJson = StringProtoJsons[s];
                switch (Localization.language)
                {
                    case Language.zhCN:
                        return stringProtoJson.ZHCN;

                    case Language.enUS:
                        return stringProtoJson.ENUS;
                }
            }

            var strings = LDB.strings;
            if (strings == null) return s;

            var stringProto = strings[s];
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
