using System.Text;
using HarmonyLib;
using ProjectGenesis.Utils;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.DisplayText
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
                case Utils_ERecipeType.Chemical:
                    __result = "化工厂".Translate();
                    break;

                case Utils_ERecipeType.Refine:
                    __result = "原油精炼厂".TranslateFromJson();
                    break;

                case Utils_ERecipeType.Assemble:
                    __result = "基础制造台".TranslateFromJson();
                    break;

                case Utils_ERecipeType.标准制造:
                    __result = "标准制造单元".TranslateFromJson();
                    break;

                case Utils_ERecipeType.高精度加工:
                    __result = "高精度装配线".TranslateFromJson();
                    break;

                case Utils_ERecipeType.矿物处理:
                    __result = "矿物处理厂".TranslateFromJson();
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "typeString", MethodType.Getter)]
        public static void ItemProto_typeString(ref ItemProto __instance, ref string __result)
        {
            if (__instance.Type != EItemType.Production) return;
            
            switch ((Utils_ERecipeType)__instance.prefabDesc.assemblerRecipeType)
            {
                case Utils_ERecipeType.Assemble:
                    __result = "基础制造".TranslateFromJson();
                    break;

                case Utils_ERecipeType.标准制造:
                    __result = "标准制造".TranslateFromJson();
                    break;

                case Utils_ERecipeType.高精度加工:
                    __result = "高精度加工".TranslateFromJson();
                    break;

                case Utils_ERecipeType.矿物处理:
                    __result = "T矿物处理".TranslateFromJson();
                    break;

                case Utils_ERecipeType.所有制造:
                    __result = "所有制造".TranslateFromJson();
                    break;

                case Utils_ERecipeType.垃圾回收:
                    __result = "物质回收".TranslateFromJson();
                    break;

                case Utils_ERecipeType.高分子化工:
                    __result = "T先进化工".TranslateFromJson();
                    break;
                    
                case Utils_ERecipeType.所有化工:
                    __result = "复合化工".TranslateFromJson();
                    break;

                case Utils_ERecipeType.复合制造:
                    __result = "复合制造".TranslateFromJson();
                    break;

                case Utils_ERecipeType.所有熔炉:
                    __result = "复合冶炼".TranslateFromJson();
                    break;
            }
        }

        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        [HarmonyPostfix]
        public static void GetPropValuePatch(ref ItemProto __instance, int index, ref string __result)
        {
            if (index >= __instance.DescFields.Length) return;

            switch (__instance.DescFields[index])
            {
                case 4:
                    if (!__instance.prefabDesc.isPowerGen) return;

                    switch (__instance.prefabDesc.fuelMask)
                    {
                        case 2:
                            __result = "裂变能".TranslateFromJson();
                            return;

                        case 4:
                            __result = "质能转换".TranslateFromJson();
                            return;

                        case 16:
                            __result = "聚变能".TranslateFromJson();
                            return;
                    }

                    if (__instance.ModelIndex == 464) __result = "裂变能".TranslateFromJson();

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
                case ProtoIDUsedByPatches.T黑雾协调中心:
                    __result = "黑雾协调中心文字描述".TranslateFromJson();
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "FindRecipes")]
        public static void ItemProto_FindRecipes(ItemProto __instance) => __instance.isRaw = __instance.recipes.Count == 0;
    }
}
