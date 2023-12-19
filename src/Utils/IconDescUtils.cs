using System;
using System.Collections.Generic;
using System.Reflection;
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
                                                                                      { 6207, new Color32(230, 239, 137, 255) }, //S
                                                                                      { 6222, new Color32(106, 175, 78, 255) }, //Rads
                                                                                      { 6225, new Color32(130, 235, 139, 255) }, //U
                                                                                      { 6226, new Color32(243, 98, 113, 255) }, //Pu
                                                                                      { 6201, new Color32(30, 29, 30, 255) }, //Tungsten
                                                                                  };
        internal static readonly Dictionary<int, Color32> CompentColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 7803, new Color32(228, 153, 255, 255) }, //光学芯片
                                                                                      { 7804, new Color32(228, 153, 255, 255) }, //光学纤维
                                                                                      { 7805, new Color32(93, 191, 255, 255) }, //量子主机
                                                                                      { 7806, new Color32(228, 153, 255, 255) }, //光学主机
                                                                                      { 6263, new Color32(150, 173, 240, 255) }, //氦闪约束器
                                                                                      { 6267, new Color32(147, 244, 241, 255) }, //大气采集站
                                                                                      { 6221, new Color32(122, 227, 130, 255) }, //同位素发电机
                                                                                      { 6261, new Color32(41, 221, 255, 255) }, //人造恒星MK2
                                                                                      { 6229, new Color32(164, 218, 255, 255) }, //量子缸
                                                                                      { 6231, new Color32(213, 82, 255, 255) }, //量子箱
                                                                                      { 6230, new Color32(196, 255, 106, 255) }, //粉碎机
                                                                                      { 7617, new Color32(77, 182, 241, 255) }, //机枪塔MK2
                                                                                      { 7618, new Color32(140, 64, 219, 255) }, //激光塔MK2
                                                                                      { 6501, new Color32(234, 163, 87, 255) }, //基础组件
                                                                                      { 6502, new Color32(87, 255, 191, 255) }, //先进组件
                                                                                      { 6503, new Color32(53, 206, 255, 255) }, //尖端组件
                                                                                      { 7501, new Color32(210, 157, 118, 255) }, //塑料基板
                                                                                      { 7504, new Color32(109, 196, 255, 255) }, //光学基板
                                                                                      { 6257, new Color32(241, 158, 60, 255) }, //大组装
                                                                                      { 6258, new Color32(71, 132, 253, 255) }, //大熔炉
                                                                                      { 6259, new Color32(249, 255, 89, 255) }, //大化反
                                                                                      { 6260, new Color32(201, 50, 65, 255) }, //大精密
                                                                                      { 6264, new Color32(71, 188, 84, 255) }, //大回收
                                                                                      { 6265, new Color32(106, 61, 172, 255) }, //大对撞
                                                                                  };
        internal static readonly Dictionary<int, Color32> FullWithIconColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 7612, new Color32(173, 207, 172, 255) }, //核子炮弹
                                                                                      { 7613, new Color32(187, 172, 252, 255) }, //反物质炮弹
                                                                                      { 7614, new Color32(155, 208, 233, 255) }, //洲际导弹
                                                                                      { 7615, new Color32(187, 172, 252, 255) }, //反物质导弹
                                                                                      { 6204, new Color32(161, 157, 152, 255) }, //铝合金
                                                                                      { 7707, new Color32(51, 51, 57, 255) }, //钨钢
                                                                                      { 6271, new Color32(221, 218, 255, 255) }, //三元合金
                                                                                      { 7608, new Color32(81, 83, 90, 255) }, //钨子弹
                                                                                      { 7609, new Color32(221, 218, 255, 255) }, //三元子弹
                                                                                      { 7616, new Color32(187, 172, 252, 255) }, //反物质子弹
                                                                                      { 6252, new Color32(160, 216, 255, 255) }, //铁粉
                                                                                      { 6253, new Color32(237, 176, 150, 255) }, //铜粉
                                                                                      { 6273, new Color32(218, 212, 195, 255) }, //铝粉
                                                                                      { 6254, new Color32(153, 236, 182, 255) }, //硅粉
                                                                                      { 6255, new Color32(232, 236, 255, 255) }, //钛粉
                                                                                      { 6281, new Color32(32, 33, 36, 255) }, //钨粉
                                                                                      { 6256, new Color32(56, 57, 62, 255) }, //煤粉
                                                                                      { 6251, new Color32(181, 184, 198, 255) }, //石粉
                                                                                      { 6277, new Color32(255, 255, 142, 255) }, //硫粉
                                                                                  };
        internal static readonly Dictionary<int, Color32> WhiteColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 7610, new Color32(255, 255, 255, 255) }, //核子单元
                                                                                      { 7611, new Color32(255, 255, 255, 255) }, //反物质单元
                                                                                  };
        internal static readonly Dictionary<int, Color32> MetalWithoutIconColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 6203, new Color32(186, 176, 144, 255) }, //Al Ignot
                                                                                      { 7705, new Color32(43, 44, 48, 255) }, //Tungsten Ignot
                                                                                      { 6227, new Color32(66, 222, 87, 255) }, //U Ignot
                                                                                      { 6228, new Color32(239, 83, 90, 255) }, //Pu Ignot
                                                                                      { 6208, new Color32(93, 91, 83, 255) }, //混凝土
                                                                                  };
        internal static readonly Dictionary<int, Color32> GlassColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 7706, new Color32(91, 91, 91, 255) }, //钨玻璃
                                                                                  };
        internal static readonly Dictionary<int, Color32> RodColor = new Dictionary<int, Color32>
                                                                                  {
                                                                                      { 6217, new Color32(163, 145, 85, 255) }, //煤油棒
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
            if (MetalWithoutIconColor.TryGetValue(itemid, out value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.iconAlpha = 0.0f;
                desc.iconEmission = Color.clear;
            }
            if (FullWithIconColor.TryGetValue(itemid, out value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.metallic = 0.0f;
                desc.iconEmission = Color.clear;
            }

            if (CompentColor.TryGetValue(itemid, out value))
            {
                //desc.faceColor = colors[1];
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.iconEmission = Color.clear;
            }
            if (GlassColor.TryGetValue(itemid, out value))
            {
                desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                desc.iconAlpha = 0.0f;
                desc.metallic = 1.5f;
                desc.solidAlpha = 0.8f;
                desc.iconEmission = Color.clear;
            }
            if (RodColor.TryGetValue(itemid, out value))
            {
                //desc.faceColor = value;
                desc.sideColor = value;
                desc.reserved0 = value;
                //desc.iconAlpha = 0.0f;
                desc.metallic = 1f;
                desc.solidAlpha = 0.6f;
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

        internal static IconToolNew.IconDesc ExportIconDesc(int itemId)
        {
            IconSet iconSet = GameMain.iconSet;

            var iconDesc = new IconToolNew.IconDesc();

            uint num1 = iconSet.itemIconIndex[itemId];
            if (num1 <= 0) return iconDesc;

            FieldInfo[] fields = typeof(IconToolNew.IconDesc).GetFields(BindingFlags.Instance | BindingFlags.Public);

            uint index = 0;
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.FieldType == typeof(float))
                {
                    fieldInfo.SetValue(iconDesc, iconSet.itemDescArr[(int)num1 * 40 + (int)index++]);
                }
                else if (fieldInfo.FieldType == typeof(Color))
                {
                    float r = iconSet.itemDescArr[(int)num1 * 40 + (int)index++];
                    float g = iconSet.itemDescArr[(int)num1 * 40 + (int)index++];
                    float b = iconSet.itemDescArr[(int)num1 * 40 + (int)index++];
                    float a = iconSet.itemDescArr[(int)num1 * 40 + (int)index++];

                    fieldInfo.SetValue(iconDesc, new Color(r, g, b, a));
                }
            }

            return iconDesc;
        }
    }
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
        
        if (hex.Length == 6) hex += "FF";
        
        if (hex.Length == 8)
            rgba = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        else
            throw new ArgumentException("Argument " + nameof(hex) + " : Invavid Length");
    }

    public static implicit operator Color(MyColor c)
        => new Color(c.r / (float)byte.MaxValue, c.g / (float)byte.MaxValue, c.b / (float)byte.MaxValue, c.a / (float)byte.MaxValue);

    public static implicit operator Color32(MyColor c) => new Color32(c.r, c.g, c.b, c.a);
}
