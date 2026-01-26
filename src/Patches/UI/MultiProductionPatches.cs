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
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.OnSelectedRecipeChange))]
        public static void UIReplicatorWindow_OnSelectedRecipeChange(UIReplicatorWindow __instance)
        {
            RecipeProto selectedRecipe = UIRoot.instance.uiGame.replicator.selectedRecipe;

            if (selectedRecipe == null) return;

            int results = selectedRecipe.Results.Length;

            if (results <= 2)
            {
                ReplicatorImages.ForEach(image => image.gameObject.SetActive(false));
                ReplicatorTexts.ForEach(text => text.gameObject.SetActive(false));

                __instance.treeMainIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, 0f);

                __instance.treeMainCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(1, -11f);

                return;
            }

            if (ReplicatorImages.Count < results - 2)
                for (int x = ReplicatorImages.Count; x < results - 2; ++x)
                {
                    ReplicatorImages.Add(Object.Instantiate(__instance.treeMainIcon1, __instance.treeMainIcon1.transform.parent));
                    ReplicatorTexts.Add(Object.Instantiate(__instance.treeMainCountText1, __instance.treeMainCountText1.transform.parent));
                }

            for (var i = 0; i < ReplicatorImages.Count; ++i)
            {
                Image image = ReplicatorImages[i];
                Text text = ReplicatorTexts[i];

                if (i >= results - 2)
                {
                    image.gameObject.SetActive(false);
                    text.gameObject.SetActive(false);

                    continue;
                }

                image.gameObject.SetActive(true);
                text.gameObject.SetActive(true);

                ItemProto itemProto = LDB.items.Select(selectedRecipe.Results[i + 2]);
                int num = selectedRecipe.ResultCounts[i + 2];

                UIButton button = image.gameObject.GetComponent<UIButton>();
                button.tips.itemId = itemProto?.ID ?? 0;
                button.tips.type = UIButton.ItemTipType.IgnoreIncPoint;
                image.sprite = itemProto?.iconSprite;
                text.gameObject.SetActive(num > 1);
                text.text = num > 1 ? "x" + num : "";

                image.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) + 64 * (i + 2), 0f);
                text.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1) + 64 * (i + 2) - 24, -11f);
            }

            __instance.treeMainBox.sizeDelta = new Vector2(64 * results, 64f);

            __instance.treeMainIcon0.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-32 * (results - 1), 0f);

            __instance.treeMainIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(-32 * (results - 1) + 64 * 1, 0f);

            __instance.treeMainCountText0.gameObject.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(-32 * (results - 1) - 24, -11f);

            __instance.treeMainCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(-32 * (results - 1) + 64 * 1 - 24, -11f);


            List<UIButton> treeUpList = UIRoot.instance.uiGame.replicator.treeUpList;

            foreach (UIButton treeUp in treeUpList) treeUp.gameObject.SetActive(false);
        }

    #endregion

    #region UIAssemblerWindow

        // Patch assembler window
        private static readonly List<Image> AssemblerProductProgress = new List<Image>();
        private static readonly List<Image> AssemblerExtraProductProgress = new List<Image>();
        private static readonly List<Image> AssemblerProductIcon = new List<Image>();
        private static readonly List<Text> AssemblerProductCountText = new List<Text>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow._OnUpdate))]
        public static void UIAssemblerWindow_OnUpdate(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[__instance.assemblerId];

            if (assembler.id != __instance.assemblerId) return;

            if (assembler.recipeId == 0) return;

            int results = assembler.recipeExecuteData.products.Length;

            if (results <= 2)
            {
                AssemblerProductProgress.ForEach(image => image.gameObject.SetActive(false));
                AssemblerExtraProductProgress.ForEach(image => image.gameObject.SetActive(false));
                AssemblerProductIcon.ForEach(image => image.gameObject.SetActive(false));
                AssemblerProductCountText.ForEach(text => text.gameObject.SetActive(false));

                return;
            }

            if (AssemblerProductProgress.Count < results - 2)
                for (int x = AssemblerProductProgress.Count; x < results - 2; ++x)
                {
                    AssemblerProductProgress.Add(Object.Instantiate(__instance.productProgress1,
                        __instance.productProgress1.transform.parent));
                    AssemblerExtraProductProgress.Add(Object.Instantiate(__instance.extraProductProgress1,
                        __instance.extraProductProgress1.transform.parent));
                    AssemblerProductCountText.Add(Object.Instantiate(__instance.productCountText1,
                        __instance.productCountText1.transform.parent));

                    Image icon = Object.Instantiate(__instance.productIcon1, __instance.productIcon1.transform.parent);
                    UIAssemblerWindow instance = __instance;
                    int index = x + 2;

                    icon.gameObject.GetComponent<UIButton>().button.onClick.AddListener(() =>
                    {
                        if (instance.assemblerId == 0 || instance.factory == null) return;

                        AssemblerComponent assemblerComponent = instance.factorySystem.assemblerPool[instance.assemblerId];

                        if (assemblerComponent.id != instance.assemblerId || assemblerComponent.recipeId == 0
                                                                          || assemblerComponent.recipeExecuteData.products.Length < index + 1
                                                                          || assemblerComponent.produced[index] <= 0)
                            return;

                        int num = instance.player.TryAddItemToPackage(assemblerComponent.recipeExecuteData.products[index],
                            assemblerComponent.produced[index], 0, false);
                        assemblerComponent.produced[index] = 0;

                        if (num > 0) UIItemup.Up(assemblerComponent.recipeExecuteData.products[index], num);
                    });

                    AssemblerProductIcon.Add(icon);
                }

            for (var i = 0; i < AssemblerProductProgress.Count; ++i)
            {
                Image productProgress = AssemblerProductProgress[i];
                Image extraProductProgress = AssemblerExtraProductProgress[i];
                Image productIcon = AssemblerProductIcon[i];
                Text productCountText = AssemblerProductCountText[i];

                if (i >= results - 2)
                {
                    productProgress.gameObject.SetActive(false);
                    extraProductProgress.gameObject.SetActive(false);
                    productIcon.gameObject.SetActive(false);
                    productCountText.gameObject.SetActive(false);

                    continue;
                }

                int index = i + 2;
                ItemProto itemProto = LDB.items.Select(assembler.recipeExecuteData.products[index]);

                productIcon.sprite = itemProto?.iconSprite;

                UIButton button = productIcon.gameObject.GetComponent<UIButton>();
                button.tips.itemId = assembler.recipeExecuteData.products[index];
                button.tips.itemInc = 0;
                button.tips.itemCount = 0;
                button.tips.type = UIButton.ItemTipType.Item;
                button.tips.corner = 2;
                button.tips.delay = 0.2f;
                button.button.interactable = assembler.produced[index] > 0;

                productCountText.text = assembler.produced[index].ToString();
                productProgress.fillAmount = __instance.productProgress1.fillAmount;
                extraProductProgress.fillAmount = __instance.extraProductProgress1.fillAmount;

                Vector2 vector = __instance.productProgress1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productProgress.gameObject.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.extraProductProgress1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                extraProductProgress.gameObject.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.productIcon1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productIcon.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(vector.x + 64 * (index - 1), vector.y);
                vector = __instance.productCountText1.gameObject.GetComponent<RectTransform>().anchoredPosition;
                productCountText.gameObject.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(vector.x + 64 * (index - 1), vector.y);

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
