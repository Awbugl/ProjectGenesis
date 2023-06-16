using HarmonyLib;
using ProjectGenesis.Patches.Logic.LithographyAssembler;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class LithographyPatches
    {
        private static GameObject _obj;
        private static GameObject _iconObj;
        private static Text _text;
        private static Image _icon;
        private static UIButton _uiButton;
        private static Button _button;

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void VFPreload_InvokeOnLoadWorkEnded_Postfix()
        {
            if (_obj == null)
            {
                var grandParent = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Assembler Window/produce").transform;
                _obj = new GameObject { name = "lithography" };
                _obj.transform.SetParent(grandParent, false);
                _obj.AddComponent<Image>();

                var image = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel").GetComponent<Image>();

                var image1 = _obj.GetComponent<Image>();
                image1.sprite = image.sprite;
                image1.color = image.color;

                _obj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
                _obj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                _obj.transform.localPosition = new Vector3(100, -45, 0);

                var parent = _obj.transform;
                var oriIconObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/ray-receiver/catalytic/eff-text");

                var _textobj = Object.Instantiate(oriIconObj, parent);
                _textobj.transform.localScale = Vector3.one;
                _textobj.transform.localPosition = new Vector3(0, -42, 0);
                _text = _textobj.transform.GetComponent<Text>();
                _text.fontSize = 16;

                oriIconObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel/fuel-icon");
                _iconObj = Object.Instantiate(oriIconObj, parent);
                _iconObj.transform.localScale = Vector3.one;
                _icon = _iconObj.GetComponent<Image>();
                _iconObj.transform.Find("cnt-text").GetComponent<Text>().text = "";

                var oriButtonObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel/button");
                var emptyRodButtonObj = Object.Instantiate(oriButtonObj, parent);
                emptyRodButtonObj.transform.localScale = Vector3.one;
                _uiButton = emptyRodButtonObj.GetComponent<UIButton>();
                _button = emptyRodButtonObj.GetComponent<Button>();
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnEmptyRodIconClick);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAssemblerWindow), "OnServingBoxChange")]
        public static void UIAssemblerWindow_OnServingBoxChange_Postfix(ref UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factorySystem == null)
            {
                _obj.SetActive(false);
                return;
            }

            var assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assemblerComponent.id != __instance.assemblerId)
            {
                _obj.SetActive(false);
                return;
            }

            _obj.SetActive(assemblerComponent.recipeType == (ERecipeType_1)Utils_ERecipeType.电路蚀刻);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIAssemblerWindow), "_OnUpdate")]
        public static void UIAssemblerWindow_OnUpdate_Postfix(ref UIAssemblerWindow __instance)
        {
            if (_obj.activeSelf)
            {
                if (__instance.assemblerId == 0 || __instance.factorySystem == null) return;
                ref var assemblerComponent = ref __instance.factorySystem.assemblerPool[__instance.assemblerId];
                if (assemblerComponent.id != __instance.assemblerId) return;

                var data = LithographyAssemblerPatches.GetLithographyData(__instance.factorySystem.planet.id, __instance.assemblerId);

                if (data.ItemId != 0)
                {
                    _iconObj.SetActive(true);
                    var itemProto = LDB.items.Select(data.ItemId);
                    _icon.sprite = itemProto.iconSprite;
                    _uiButton.tips.itemId = itemProto.ID;
                    _uiButton.tips.itemCount = data.ItemCount;
                }
                else
                {
                    _iconObj.SetActive(false);
                    _uiButton.tips.itemId = 0;
                    _uiButton.tips.itemCount = 0;
                }

                _uiButton.tips.itemInc = 0;
                _uiButton.tips.type = UIButton.ItemTipType.IgnoreIncPoint;
            }
        }

        private static void OnEmptyRodIconClick()
        {
            var assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;
            var player = assemblerWindow.player;

            if (assemblerWindow.assemblerId == 0 || assemblerWindow.factorySystem == null || player == null) return;

            ref var assemblerComponent = ref assemblerWindow.factorySystem.assemblerPool[assemblerWindow.assemblerId];
            if (assemblerComponent.id != assemblerWindow.assemblerId) return;

            if (player.inhandItemId > 0 && player.inhandItemCount == 0)
            {
                player.SetHandItems(0, 0);
                return;
            }

            var data = LithographyAssemblerPatches.GetLithographyData(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId);

            if (player.inhandItemId > 0 && player.inhandItemCount > 0)
            {
                if (player.inhandItemId == 1000) return;

                var lithographyLensId = LithographyAssemblerPatches.GetLithographyLenId(assemblerComponent.recipeId);
                var flag = lithographyLensId != 0 && lithographyLensId == player.inhandItemId;

                if (!flag)
                {
                    UIRealtimeTip.Popup("不相符的物品".TranslateFromJson());
                    return;
                }

                if (player.inhandItemId == data.ItemId || data.ItemId == 0)
                {
                    if (data.ItemCount >= 1)
                    {
                        UIRealtimeTip.Popup("栏位已满".Translate());
                        return;
                    }

                    var inhandItemCount = player.inhandItemCount;

                    var inhandItemInc = player.inhandItemInc;
                    var itemInc = inhandItemCount == 0 ? 0 : inhandItemInc / inhandItemCount;

                    data.ItemId = player.inhandItemId;
                    data.ItemCount = 1;
                    data.ItemInc = itemInc;

                    LithographyAssemblerPatches.SetLithographyData(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId, data);

                    player.AddHandItemCount_Unsafe(-1);
                    player.SetHandItemInc_Unsafe(player.inhandItemInc - itemInc);

                    if (player.inhandItemCount <= 0)
                    {
                        player.SetHandItemId_Unsafe(0);
                        player.SetHandItemCount_Unsafe(0);
                        player.SetHandItemInc_Unsafe(0);
                        return;
                    }
                }
                else
                {
                    if (player.inhandItemCount > 100)
                    {
                        UIRealtimeTip.Popup("不相符的物品".TranslateFromJson());
                        return;
                    }

                    player.SetHandItemId_Unsafe(0);
                    player.SetHandItemCount_Unsafe(0);
                    player.SetHandItemInc_Unsafe(0);

                    if (VFInput.shift || VFInput.control)
                    {
                        LithographyAssemblerPatches.SetEmpty(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId);
                        return;
                    }

                    LithographyAssemblerPatches.SetEmpty(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId, false);
                    return;
                }
            }
            else if (player.inhandItemId == 0 && player.inhandItemCount == 0)
            {
                if (data.ItemId == 0) return;

                if (VFInput.shift || VFInput.control)
                {
                    LithographyAssemblerPatches.SetEmpty(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId);
                    return;
                }

                LithographyAssemblerPatches.SetEmpty(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId, false);
                return;
            }
        }
    }
}
