using System;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class UITechNodePatches
    {
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.UpdateLayoutDynamic))]
        [HarmonyPostfix]
        public static void UITechNode_UpdateLayoutDynamic(UITechNode __instance, bool forceUpdate = false, bool forceReset = false)
        {
            float num4 = Mathf.Clamp(
                Mathf.Max(__instance.unlockText.preferredWidth - 40f + __instance.unlockTextTrans.anchoredPosition.x,
                    Math.Min(__instance.techProto.unlockRecipeArray.Length, 3) * 46) + __instance.baseWidth, __instance.minWidth,
                __instance.maxWidth);

            float x = __instance.focusState < 1f
                ? Mathf.Lerp(__instance.minWidth, num4, __instance.focusState)
                : Mathf.Lerp(num4, __instance.maxWidth, __instance.focusState - 1f);

            __instance.panelRect.sizeDelta = new Vector2(x, __instance.panelRect.sizeDelta.y);

            __instance.titleText.rectTransform.sizeDelta =
                new Vector2(x - (GameMain.history.TechState(__instance.techProto.ID).curLevel > 0 ? 65 : 25), 24f);
        }
    }
}
