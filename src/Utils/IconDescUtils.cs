using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommonAPI.Systems;
using UnityEngine;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace ProjectGenesis.Utils
{
    internal static class IconDescUtils
    {
        internal static readonly Dictionary<int, Color32> FluidColor = new Dictionary<int, Color32>
                                                                       {
                                                                           { 7019, new Color32(129, 199, 241, 255) }, //O2
                                                                           { 6220, new Color32(137, 242, 178, 255) }, //N2
                                                                           { 6234, new Color32(244, 255, 183, 255) }, //He4
                                                                           { 6235, new Color32(210, 222, 142, 255) }, //He3
                                                                       };

        internal static readonly Dictionary<int, Color32> FluidWithoutIconColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 1114, new Color32(138, 83, 43, 255) },   //Coal Oil
                                                                                      { 7018, new Color32(97, 132, 186, 255) },  //Ocean
                                                                                      { 7006, new MyColor("DA3846FF") },   //苯
                                                                                      { 7009, new Color32(167, 255, 39, 255) },  //丙烯
                                                                                      { 6206, new Color32(191, 227, 255, 255) }, //CO2
                                                                                      { 6212, new Color32(188, 182, 5, 255) },   //JP10
                                                                                      { 7014, new Color32(104, 187, 154, 255) }, //HCl
                                                                                      { 7015, new Color32(42, 97, 32, 255) },    //FeCl3
                                                                                      { 7002, new Color32(195, 198, 234, 255) }, //NH3
                                                                                      { 7017, new Color32(157, 56, 157, 255) },  //HNO3
                                                                                  };
        internal static readonly Dictionary<int, Color32> OreColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 6202, new Color32(210, 184, 147, 255) }, //Al
                                                                                  };
        internal static readonly Dictionary<int, Color32> CompentColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 7803, new Color32(228, 153, 255, 255) }, //光学芯片
                                                                                  };


        private static readonly Color DefaultSideColor = new Color32(119, 136, 153, 255),
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
            if (MartixsDescs.TryGetValue(itemid, out IconToolNew.IconDesc iconDesc)) return iconDesc;

            IconToolNew.IconDesc desc = ProtoRegistry.GetDefaultIconDesc(Color.white, DefaultSideColor);

            if (FluidColor.TryGetValue(itemid, out Color32 value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.liquidity = 1f;
                desc.metallic = 1f;
                desc.solidAlpha = 0.6f;
            }

            if (FluidWithoutIconColor.TryGetValue(itemid, out value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.liquidity = 1f;
                desc.metallic = 1f;
                desc.solidAlpha = 0.6f;
                desc.iconEmission = Color.clear;
                desc.iconAlpha = 0.0f;
            }
            if (OreColor.TryGetValue(itemid, out value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.iconEmission = Color.clear;
            }
            if (CompentColor.TryGetValue(itemid, out value))
            {
                //desc.faceColor = colors[1];
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.iconEmission = Color.clear;
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

    [StructLayout(LayoutKind.Explicit)]
    internal struct MyColor
    {
        [FieldOffset(0)]
        private int rgba;

        [FieldOffset(0)]
        public byte r;

        [FieldOffset(1)]
        public byte g;

        [FieldOffset(2)]
        public byte b;

        [FieldOffset(3)]
        public byte a;

        public MyColor(
            byte r,
            byte g,
            byte b,
            byte a = 255)
        {
            rgba = 0;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public MyColor(
            float r,
            float g,
            float b,
            float a = 1) : this((byte)(Mathf.Clamp01(r) * (double)byte.MaxValue), (byte)(Mathf.Clamp01(g) * (double)byte.MaxValue),
                                (byte)(Mathf.Clamp01(b) * (double)byte.MaxValue), (byte)(Mathf.Clamp01(a) * (double)byte.MaxValue)) { }

        public MyColor(string hex)
        {
            r = 0;
            g = 0;
            b = 0;
            a = 0;

            switch (hex.Length)
            {
                case 8:
                    rgba = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                    break;

                case 6:
                    rgba = (int.Parse(hex, System.Globalization.NumberStyles.HexNumber) << 2) + 0xFF;
                    break;

                default:
                    throw new ArgumentException("Argument " + nameof(hex) + " : Invavid Length");
            }
        }

        public static implicit operator Color(MyColor c)
            => new Color(c.r / (float)byte.MaxValue, c.g / (float)byte.MaxValue, c.b / (float)byte.MaxValue, c.a / (float)byte.MaxValue);

        public static implicit operator Color32(MyColor c) => new Color32(c.r, c.g, c.b, c.a);
    }
}
