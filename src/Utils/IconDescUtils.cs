using System.Collections.Generic;
using CommonAPI.Systems;
using UnityEngine;

namespace ProjectGenesis.Utils
{
    internal static class IconDescUtils
    {
        // Specify color of each fluid here, one per line.
        internal static readonly Dictionary<int, Color32> FluidColor = new Dictionary<int, Color32>
                                                                       {
                                                                           { 7017, new Color32(61, 137, 224, 255) },
                                                                           { 7016, new Color32(176, 106, 85, 255) },
                                                                           { 7015, new Color32(116, 152, 99, 255) },
                                                                           { 6531, new Color32(187, 217, 219, 255) },
                                                                           { 7014, new Color32(99, 179, 148, 255) },
                                                                           { 7013, new Color32(226, 72, 86, 255) },
                                                                           { 7012, new Color32(214, 39, 98, 255) },
                                                                           { 7011, new Color32(51, 255, 173, 255) },
                                                                           { 7010, new Color32(188, 149, 92, 255) },
                                                                           { 7009, new Color32(218, 207, 147, 255) },
                                                                           { 6533, new Color32(33, 44, 65, 255) },
                                                                           { 7008, new Color32(147, 230, 43, 255) },
                                                                           { 7007, new Color32(109, 183, 101, 255) },
                                                                           { 7006, new Color32(218, 127, 78, 255) },
                                                                           { 7005, new Color32(158, 212, 68, 255) },
                                                                           { 6532, new Color32(117, 184, 41, 255) },
                                                                           { 7004, new Color32(115, 177, 74, 255) },
                                                                           { 7003, new Color32(119, 176, 123, 255) },
                                                                           { 7002, new Color32(216, 216, 216, 255) },
                                                                           { 6999, new Color32(255, 243, 128, 255) },
                                                                           { 1116, new Color32(125, 27, 126, 255) },
                                                                           { 7001, new Color32(185, 185, 185, 255) },
                                                                           { 7018, new Color32(90, 126, 179, 255) },
                                                                           { 7019, new Color32(170, 198, 255, 255) },
                                                                           { 6211, new Color32(10, 60, 16, 255) },
                                                                           { 6202, new Color32(223, 222, 31, 255) },
                                                                           { 6213, new Color32(29, 29, 135, 255) },
                                                                           { 6215, new Color32(255, 128, 52, 255) },
                                                                           { 6207, new Color32(116, 99, 22, 255) },
                                                                           { 6214, new Color32(142, 138, 60, 255) },
                                                                           { 6203, new Color32(202, 167, 27, 255) },
                                                                           { 6204, new Color32(224, 209, 23, 255) },
                                                                           { 6212, new Color32(222, 214, 0, 255) },
                                                                           { 6210, new Color32(138, 172, 164, 255) },
                                                                           { 6219, new Color32(193, 130, 58, 255) },
                                                                           { 1114, new Color32(204, 102, 0, 255) },
                                                                           { 6201, new Color32(241, 181, 37, 255) },
                                                                           { 6209, new Color32(230, 81, 21, 255) },
                                                                           { 6208, new Color32(220, 122, 29, 255) },
                                                                           { 6205, new Color32(131, 209, 255, 255) },
                                                                           { 6234, new Color32(255, 232, 124, 255) },
                                                                           { 6235, new Color32(244, 164, 96, 255) }
                                                                       };

        private static readonly Color DefaultSideColor = new Color32(119, 136, 153, 255),
                                      FluidSideColor = new Color32(229, 228, 226, 255),
                                      Color6278 = new Color(1f, 0.4117f, 0.3137f, 0.1961f),
                                      Emission6278 = new Color(1f, 0.2706f, 0f, 0f),
                                      Color6279 = new Color(1f, 0.7530f, 0.7961f, 0.1961f),
                                      Emission6279 = new Color(0.7804f, 0.0824f, 0.5216f, 0f),
                                      Color6280 = new Color(0.5020f, 0.5020f, 0.5020f, 0.1961f);

        private static readonly Dictionary<int, IconToolNew.IconDesc> MartixsDescs = new Dictionary<int, IconToolNew.IconDesc>
                                                                                     {
                                                                                         { 6278, GetMartixIconDesc(Color6278, Emission6278) },
                                                                                         { 6279, GetMartixIconDesc(Color6279, Emission6279) },
                                                                                         { 6280, GetDefaultIconDesc(Color6280, Color.clear) }
                                                                                     };

        internal static IconToolNew.IconDesc GetIconDesc(int itemid)
        {
            if (MartixsDescs.TryGetValue(itemid, out var iconDesc)) return iconDesc;

            var desc = ProtoRegistry.GetDefaultIconDesc(Color.white, DefaultSideColor);
            if (FluidColor.ContainsKey(itemid))
            {
                desc.faceColor = FluidSideColor;
                desc.sideColor = FluidSideColor;
                desc.reserved0 = FluidColor[itemid];
                desc.liquidity = 1f;
                desc.metallic = 1f;
                desc.solidAlpha = 0.6f;
            }

            return desc;
        }

        private static IconToolNew.IconDesc GetMartixIconDesc(Color color, Color emission)
            => new IconToolNew.IconDesc
               {
                   faceColor = color,
                   sideColor = color,
                   faceEmission = emission,
                   sideEmission = emission,
                   iconEmission = Color.clear,
                   metallic = 0f,
                   smoothness = 0f,
                   solidAlpha = 0.5f,
                   iconAlpha = 0.0f
               };

        private static IconToolNew.IconDesc GetDefaultIconDesc(Color color, Color emission)
            => ProtoRegistry.GetDefaultIconDesc(color, color, emission, emission);
    }
}
