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
                case Utils_ERecipeType.电路蚀刻:
                    __result = "电路蚀刻机".TranslateFromJson();
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
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "fuelTypeString", MethodType.Getter)]
        public static void ItemProto_fuelTypeString(ref ItemProto __instance, ref string __result)
        {
            var type = __instance.FuelType;

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
                            __result = "仿星器".TranslateFromJson();
                            break;
                    }

                    return;

                case 18:
                    if (__instance.prefabDesc.isCollectStation && __instance.ID == ProtoIDUsedByPatches.I大气采集器) __result = "行星大气".TranslateFromJson();

                    return;

                case 22:
                    if (GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T化工技术革新) && __instance.prefabDesc.isAssembler)
                        switch (__instance.prefabDesc.modelIndex)
                        {
                            case ProtoIDUsedByPatches.M化工厂:
                            case ProtoIDUsedByPatches.M精炼厂:
                            case ProtoIDUsedByPatches.M先进化工厂:
                                __result = "4x";
                                return;
                        }

                    return;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TechProto), "UnlockFunctionText")]
        public static void TechProto_UnlockFunctionText(ref TechProto __instance, ref string __result, StringBuilder sb)
        {
            switch (__instance.ID)
            {
                case ProtoIDUsedByPatches.T化工技术革新:
                    __result = "化工技术革新文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T海洋排污1:
                    __result = "海洋排污文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T海洋排污2:
                    __result = "海洋排污2文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T大气排污:
                    __result = "大气排污文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T巨型建筑工程学:
                    __result = "巨型建筑工程学文字描述".TranslateFromJson();
                    break;

                case ProtoIDUsedByPatches.T行星协调中心:
                    __result = "行星协调中心文字描述".TranslateFromJson();
                    break;
            }
        }
    }
}
