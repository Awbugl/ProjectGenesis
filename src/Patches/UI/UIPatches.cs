using System;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class UIPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        public static void ItemProto_GetPropValue(
            ref ItemProto __instance,
            ref string __result,
            int index)
        {
            if (GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T化工技术革新) && __instance.Type == EItemType.Production)
            {
                var instanceRecipeType = __instance.prefabDesc.assemblerRecipeType;
                if ((instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.Chemical ||
                     instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.Refine ||
                     instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.高分子化工) &&
                    index == 22)
                    __result = "4x";
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawArea")]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawCover")]
        public static bool UIPowerGizmo_Draw(ref UIPowerGizmo __instance, Vector3 center, float radius) => radius < 2000;

        #region UITechNode

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechNode), "UpdateLayoutDynamic")]
        public static void UITechNode_UpdateLayoutDynamic(ref UITechNode __instance, bool forceUpdate = false, bool forceReset = false)
        {
            var num4 = Mathf.Max(__instance.unlockText.preferredWidth - 40f + __instance.unlockTextTrans.anchoredPosition.x,
                                 Math.Min(__instance.techProto.unlockRecipeArray.Length, 3) * 46) +
                       __instance.baseWidth;
            if (num4 < __instance.minWidth) num4 = __instance.minWidth;

            if (num4 > __instance.maxWidth) num4 = __instance.maxWidth;

            if (__instance.focusState < 1f)
                __instance.panelRect.sizeDelta
                    = new Vector2(Mathf.Lerp(__instance.minWidth, num4, __instance.focusState), __instance.panelRect.sizeDelta.y);
            else
                __instance.panelRect.sizeDelta = new Vector2(Mathf.Lerp(num4, __instance.maxWidth, __instance.focusState - 1f),
                                                             __instance.panelRect.sizeDelta.y);

            __instance.titleText.rectTransform.sizeDelta
                = new Vector2(__instance.panelRect.sizeDelta.x - (GameMain.history.TechState(__instance.techProto.ID).curLevel > 0 ? 65 : 25), 24f);
        }

        #endregion
    }
}
