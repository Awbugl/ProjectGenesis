using System;
using HarmonyLib;
using UnityEngine;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        // 绕开 CommonAPI 的 UIAssemblerWindowPatch.ChangePicker
        [HarmonyPatch(typeof(UIAssemblerWindow), "OnSelectRecipeClick")]
        [HarmonyPriority(1)]
        [HarmonyPrefix]
        public static bool UIAssemblerWindow_OnSelectRecipeClick(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 ||
                __instance.factory == null ||
                __instance.factorySystem.assemblerPool[__instance.assemblerId].id != __instance.assemblerId)
                return false;

            int entityId = __instance.factorySystem.assemblerPool[__instance.assemblerId].entityId;
            if (entityId == 0) return false;

            ItemProto itemProto = LDB.items.Select(__instance.factory.entityPool[entityId].protoId);
            if (itemProto == null) return false;

            ERecipeType assemblerRecipeType = itemProto.prefabDesc.assemblerRecipeType;
            if (UIRecipePicker.isOpened)
                UIRecipePicker.Close();
            else
                UIRecipePicker.Popup(__instance.windowTrans.anchoredPosition + new Vector2(-300f, -135f),
                                     i => AccessTools.Method(typeof(UIAssemblerWindow), "OnRecipePickerReturn")
                                                     .Invoke(__instance, new object[] { i }), assemblerRecipeType);

            return false;
        }

        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPostfix]
        public static void UIRecipePicker_RefreshIcons(
            UIRecipePicker __instance,
            ref ERecipeType_1 ___filter,
            ref int ___currentType,
            ref uint[] ___indexArray,
            ref RecipeProto[] ___protoArray)
        {
            var filter = (Utils.ERecipeType)___filter;
            if (filter != Utils.ERecipeType.所有化工 && filter != Utils.ERecipeType.所有熔炉 && filter != Utils.ERecipeType.所有制造) return;

            Array.Clear(___indexArray, 0, ___indexArray.Length);
            Array.Clear(___protoArray, 0, ___protoArray.Length);
            GameHistoryData history = GameMain.history;
            RecipeProto[] dataArray = LDB.recipes.dataArray;
            IconSet iconSet = GameMain.iconSet;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int index1 = 0; index1 < dataArray.Length; ++index1)
            {
                int gridIndex = dataArray[index1].GridIndex;
                if (gridIndex < 1101 || !history.RecipeUnlocked(dataArray[index1].ID)) continue;

                if (___filter != ERecipeType_1.None && !ContainsRecipeType(___filter, dataArray[index1].Type)) continue;

                int num1 = gridIndex / 1000;
                int num2 = (gridIndex - num1 * 1000) / 100 - 1;
                int num3 = gridIndex % 100 - 1;
                if (num2 < 0 || num3 < 0 || num2 >= 7 || num3 >= 17) continue;

                int index2 = num2 * 17 + num3;
                if (index2 < 0 || index2 >= ___indexArray.Length || num1 != ___currentType) continue;

                ___indexArray[index2] = iconSet.recipeIconIndex[dataArray[index1].ID];
                ___protoArray[index2] = dataArray[index1];
            }
        }
    }
}
