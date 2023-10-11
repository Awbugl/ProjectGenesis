using System.Text;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.DisplayTextPatches
{
    internal static class DisplayTextPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecipeProto), "madeFromString", MethodType.Getter)]
        public static void RecipeProto_madeFromString(ref RecipeProto __instance, ref string __result)
        {
            var type = (Utils_ERecipeType)__instance.Type;

            switch (type)
            {
                case Utils_ERecipeType.标准制造:
                    __result = "标准制造单元".TranslateFromJson();
                    break;

                case Utils_ERecipeType.高精度加工:
                    __result = "高精度装配线".TranslateFromJson();
                    break;

                case Utils_ERecipeType.矿物处理:
                    __result = "矿物处理厂".TranslateFromJson();
                    break;

                case Utils_ERecipeType.精密组装:
                    __result = "精密制造厂".TranslateFromJson();
                    break;

                case Utils_ERecipeType.聚变生产:
                    __result = "紧凑式回旋加速器".TranslateFromJson();
                    break;

                case Utils_ERecipeType.垃圾回收:
                    __result = "物质分解设施".TranslateFromJson();
                    break;

                case Utils_ERecipeType.高分子化工:
                    __result = "先进化学反应釜".TranslateFromJson();
                    break;

                case (Utils_ERecipeType)21:
                    __result = "星际组装厂（巨构）".TranslateFromJson();
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "fuelTypeString", MethodType.Getter)]
        public static void ItemProto_fuelTypeString(ref ItemProto __instance, ref string __result)
        {
            int type = __instance.FuelType;

            switch (type)
            {
                case 2:
                    __result = "裂变能".TranslateFromJson();
                    break;

                case 16:
                    __result = "聚变能".TranslateFromJson();
                    break;
            }
        }

        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        [HarmonyPostfix]
        public static void GetPropValuePatch(ref ItemProto __instance, int index, ref string __result)
        {
            if ((ulong)index >= (ulong)__instance.DescFields.Length)
            {
                Debug.Log("Genesis Book:Now Loading");
                __result = "";
                return;
            }

            switch (__instance.DescFields[index])
            {
                case 4:
                    if (!__instance.prefabDesc.isPowerGen) return;

                    switch (__instance.prefabDesc.fuelMask)
                    {
                        case 2:
                            __result = "裂变能".TranslateFromJson();
                            break;

                        case 4:
                            __result = "质能转换".TranslateFromJson();
                            break;

                        case 16:
                            __result = "聚变能".TranslateFromJson();
                            break;
                    }

                    return;

                case 18:
                    if (__instance.prefabDesc.isCollectStation && __instance.ID == ProtoIDUsedByPatches.I大气采集器) __result = "行星大气".TranslateFromJson();

                    return;

                case 19:
                    if (__instance.prefabDesc.minerType == EMinerType.Oil)
                        __result = (600000.0 / __instance.prefabDesc.minerPeriod * GameMain.history.miningSpeedScale).ToString("0.##") + "x";
                    return;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TechProto), "UnlockFunctionText")]
        public static void TechProto_UnlockFunctionText(ref TechProto __instance, ref string __result, StringBuilder sb)
        {
            switch (__instance.ID)
            {
                case ProtoIDUsedByPatches.T巨型建筑工程学:
                    __result = "巨型建筑工程学文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T行星协调中心:
                    __result = "行星协调中心文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T虫洞航行:
                    __result = "虫洞航行文字描述".TranslateFromJson();
                    break;
            }
        }
    }
}
