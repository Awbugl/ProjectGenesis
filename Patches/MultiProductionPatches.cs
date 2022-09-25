using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    // A patch to support multi (>=3) productions.
    internal static class MultiProductionPatches
    {
        #region UIReplicatorWindow

        // Patch replicator window
        private static readonly List<Image> ReplicatorImages = new List<Image>();
        private static readonly List<Text> ReplicatorTexts = new List<Text>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "OnSelectedRecipeChange")]
        public static void UIReplicatorWindow_OnSelectedRecipeChange(ref UIReplicatorWindow __instance, bool changed)
        {
            ref var selectedRecipe
                = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RecipeProto>(UIRoot.instance.uiGame.replicator, "selectedRecipe");
            if (selectedRecipe == null) return;

            var results = selectedRecipe.Results.Length;
            if (results <= 2)
            {
                ReplicatorImages.ForEach(image => image.gameObject.SetActive(false));
                ReplicatorTexts.ForEach(text => text.gameObject.SetActive(false));

                __instance.treeMainIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, 0f);

                __instance.treeMainCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(1, -11f);

                return;
            }

            if (ReplicatorImages.Count < results - 2)
                for (var x = ReplicatorImages.Count; x < results - 2; ++x)
                {
                    ReplicatorImages.Add(Object.Instantiate(__instance.treeMainIcon1, __instance.treeMainIcon1.transform.parent));
                    ReplicatorTexts.Add(Object.Instantiate(__instance.treeMainCountText1, __instance.treeMainCountText1.transform.parent));
                }

            for (var i = 0; i < ReplicatorImages.Count; ++i)
            {
                var image = ReplicatorImages[i];
                var text = ReplicatorTexts[i];

                if (i >= results - 2)
                {
                    image.gameObject.SetActive(false);
                    text.gameObject.SetActive(false);
                    continue;
                }

                image.gameObject.SetActive(true);
                text.gameObject.SetActive(true);

                var itemProto = LDB.items.Select(selectedRecipe.Results[i + 2]);
                var num = selectedRecipe.ResultCounts[i + 2];

                var button = image.gameObject.GetComponent<UIButton>();
                button.tips.itemId = itemProto?.ID ?? 0;
                button.tips.type = UIButton.ItemTipType.IgnoreIncPoint;
                image.sprite = itemProto?.iconSprite;
                text.gameObject.SetActive(num > 1);
                text.text = num > 1 ? ("x" + num) : "";

                image.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) + 64 * (i + 2), 0f);
                text.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) + 64 * (i + 2) - 24, -11f);
            }

            __instance.treeMainBox.sizeDelta = new Vector2(64 * results, 64f);

            __instance.treeMainIcon0.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1), 0f);

            __instance.treeMainIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) + 64 * 1, 0f);

            __instance.treeMainCountText0.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) - 24, -11f);

            __instance.treeMainCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition
                = new Vector2(-32 * (results - 1) + 64 * 1 - 24, -11f);


            ref List<UIButton> treeUpList
                = ref AccessTools.FieldRefAccess<UIReplicatorWindow, List<UIButton>>(UIRoot.instance.uiGame.replicator, "treeUpList");

            foreach (var treeUp in treeUpList) treeUp.gameObject.SetActive(false);
        }

        #endregion

        #region UIAssemblerWindow

        // Patch assembler window
        private static readonly List<Image> AssemblerProductProgress = new List<Image>();
        private static readonly List<Image> AssemblerExtraProductProgress = new List<Image>();
        private static readonly List<Image> AssemblerProductIcon = new List<Image>();
        private static readonly List<Text> AssemblerProductCountText = new List<Text>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAssemblerWindow), "_OnUpdate")]
        public static void UIAssemblerWindow_OnUpdate(ref UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null) return;
            var assembler = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assembler.id != __instance.assemblerId) return;
            if (assembler.recipeId == 0) return;

            var results = assembler.products.Length;
            if (results <= 2)
            {
                AssemblerProductProgress.ForEach(image => image.gameObject.SetActive(false));
                AssemblerExtraProductProgress.ForEach(image => image.gameObject.SetActive(false));
                AssemblerProductIcon.ForEach(image => image.gameObject.SetActive(false));
                AssemblerProductCountText.ForEach(text => text.gameObject.SetActive(false));
                return;
            }

            if (AssemblerProductProgress.Count < results - 2)
                for (var x = AssemblerProductProgress.Count; x < results - 2; ++x)
                {
                    AssemblerProductProgress.Add(Object.Instantiate(__instance.productProgress1, __instance.productProgress1.transform.parent));
                    AssemblerExtraProductProgress.Add(Object.Instantiate(__instance.extraProductProgress1,
                                                                         __instance.extraProductProgress1.transform.parent));
                    AssemblerProductCountText.Add(Object.Instantiate(__instance.productCountText1, __instance.productCountText1.transform.parent));

                    var icon = Object.Instantiate(__instance.productIcon1, __instance.productIcon1.transform.parent);
                    var instance = __instance;
                    var index = x + 2;

                    icon.gameObject.GetComponent<UIButton>().button.onClick.AddListener(() =>
                                                                                        {
                                                                                            if (instance.assemblerId == 0 || instance.factory == null)
                                                                                                return;

                                                                                            var assemblerComponent
                                                                                                = instance.factorySystem.assemblerPool
                                                                                                    [instance.assemblerId];
                                                                                            if (assemblerComponent.id != instance.assemblerId ||
                                                                                                assemblerComponent.recipeId == 0 ||
                                                                                                assemblerComponent.products.Length < index + 1 ||
                                                                                                assemblerComponent.produced[index] <= 0)
                                                                                                return;

                                                                                            var num
                                                                                                = instance.player
                                                                                                          .TryAddItemToPackage(assemblerComponent.products[index],
                                                                                                                               assemblerComponent
                                                                                                                                  .produced[index], 0,
                                                                                                                               throwTrash: false);
                                                                                            assemblerComponent.produced[index] = 0;

                                                                                            if (num > 0)
                                                                                                UIItemup.Up(assemblerComponent.products[index], num);
                                                                                        });

                    AssemblerProductIcon.Add(icon);
                }

            for (var i = 0; i < AssemblerProductProgress.Count; ++i)
            {
                var productProgress = AssemblerProductProgress[i];
                var extraProductProgress = AssemblerExtraProductProgress[i];
                var productIcon = AssemblerProductIcon[i];
                var productCountText = AssemblerProductCountText[i];

                if (i >= results - 2)
                {
                    productProgress.gameObject.SetActive(false);
                    extraProductProgress.gameObject.SetActive(false);
                    productIcon.gameObject.SetActive(false);
                    productCountText.gameObject.SetActive(false);
                    continue;
                }

                var index = i + 2;
                var itemProto = LDB.items.Select(assembler.products[index]);

                productIcon.sprite = itemProto?.iconSprite;

                var button = productIcon.gameObject.GetComponent<UIButton>();
                button.tips.itemId = assembler.products[index];
                button.tips.itemInc = 0;
                button.tips.itemCount = 0;
                button.tips.type = UIButton.ItemTipType.Item;
                button.tips.corner = 2;
                button.tips.delay = 0.2f;
                button.button.interactable = assembler.produced[index] > 0;

                productCountText.text = assembler.produced[index].ToString();
                productProgress.fillAmount = __instance.productProgress1.fillAmount;
                extraProductProgress.fillAmount = __instance.extraProductProgress1.fillAmount;

                var vector = __instance.productProgress1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productProgress.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.extraProductProgress1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                extraProductProgress.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.productIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productIcon.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.productCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productCountText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(vector.x + 64 * (index - 1), vector.y);

                productProgress.gameObject.SetActive(true);
                extraProductProgress.gameObject.SetActive(true);
                productIcon.gameObject.SetActive(true);
                productCountText.gameObject.SetActive(true);
            }

            __instance.productGroup.sizeDelta = new Vector2(64 * results, 64f);
            __instance.speedGroup.anchoredPosition = new Vector2(64 * results + 16, __instance.speedGroup.anchoredPosition.y);
            __instance.servingGroup.anchoredPosition = new Vector2(64 * results + 96, __instance.servingGroup.anchoredPosition.y);
        }

        #endregion
    }
}
