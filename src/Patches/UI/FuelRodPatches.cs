using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class FuelRodPatches
    {
        private static GameObject _usedFuelObj;
        private static GameObject _emptyRodIconObj;
        private static Image _emptyRodIcon;
        private static Text _emptyRodCount;
        private static UIButton _emptyRodUIButton;
        private static Button _emptyRodButton;

        [HarmonyPatch(typeof(UIGame), nameof(UIGame._OnInit))]
        [HarmonyPostfix]
        public static void Init(UIRoot __instance)
        {
            if (_usedFuelObj) return;

            var gameObject = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel");

            _usedFuelObj = new GameObject { name = "usedFuel", };
            _usedFuelObj.transform.SetParent(gameObject.transform.parent, false);

            Image image = _usedFuelObj.AddComponent<Image>();
            Image component = gameObject.GetComponent<Image>();
            image.sprite = component.sprite;
            image.color = component.color;

            _usedFuelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
            _usedFuelObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            _usedFuelObj.transform.localPosition = new Vector3(355, -85, 0);
            Transform parent = _usedFuelObj.transform;

            _emptyRodIconObj = Object.Instantiate(gameObject.transform.Find("fuel-icon").gameObject, parent);
            _emptyRodIconObj.transform.localScale = Vector3.one;
            _emptyRodIcon = _emptyRodIconObj.GetComponent<Image>();
            _emptyRodIcon.enabled = true;
            _emptyRodCount = _emptyRodIconObj.transform.Find("cnt-text").GetComponent<Text>();
            _emptyRodCount.fontSize = 16;

            GameObject emptyRodButtonObj = Object.Instantiate(gameObject.transform.Find("button").gameObject, parent);
            emptyRodButtonObj.transform.localScale = Vector3.one;
            _emptyRodUIButton = emptyRodButtonObj.GetComponent<UIButton>();
            _emptyRodButton = emptyRodButtonObj.GetComponent<Button>();
            _emptyRodButton.onClick.RemoveAllListeners();
            _emptyRodButton.onClick.AddListener(OnEmptyRodIconClick);
        }

        [HarmonyPatch(typeof(EntityBriefInfo), nameof(EntityBriefInfo.SetBriefInfo))]
        [HarmonyPostfix]
        public static void SetBriefInfoPostPatch(EntityBriefInfo __instance, PlanetFactory _factory, int _entityId)
        {
            if (_factory == null) return;

            if (_entityId == 0) return;

            EntityData entityData = _factory.entityPool[_entityId];

            if (entityData.id == 0 || entityData.powerGenId == 0) return;

            PowerGeneratorComponent[] genPool = _factory.powerSystem.genPool;
            int powerGenId2 = entityData.powerGenId;

            PowerGeneratorComponent powerGeneratorComponent = genPool[powerGenId2];

            if (powerGeneratorComponent.fuelMask != 0)
            {
                var productCount = (int)powerGeneratorComponent.productCount;

                if (productCount > 0) __instance.storage.Add(ProtoID.I空燃料棒, productCount, powerGeneratorComponent.catalystIncPoint / productCount);
            }
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow.OnGeneratorIdChange))]
        [HarmonyPostfix]
        public static void UIPowerGeneratorWindow_OnGeneratorIdChange_Postfix(UIPowerGeneratorWindow __instance)
        {
            if (__instance.generatorId == 0 || __instance.factory == null) return;

            PowerGeneratorComponent powerGeneratorComponent = __instance.powerSystem.genPool[__instance.generatorId];

            if (powerGeneratorComponent.id != __instance.generatorId) return;

            _usedFuelObj.SetActive(powerGeneratorComponent.fuelMask != 0);
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UIPowerGenWinOnUpdatePatch(UIPowerGeneratorWindow __instance)
        {
            if (!_usedFuelObj.activeSelf) return;

            if (__instance == null) return;

            if (__instance.generatorId == 0 || __instance.factory == null) return;

            PowerGeneratorComponent powerGeneratorComponent = __instance.powerSystem.genPool[__instance.generatorId];

            if (powerGeneratorComponent.id != __instance.generatorId) return;

            ItemProto generatorProto = LDB.items.Select(__instance.factory.entityPool[powerGeneratorComponent.entityId].protoId);

            if (generatorProto == null) return;

            PowerNetwork powerNetwork = __instance.powerSystem.netPool[powerGeneratorComponent.networkId];
            __instance.powerNetworkDesc.powerNetwork = powerNetwork;
            Assert.NotNull(powerNetwork);

            if (powerNetwork == null) return;

            if (powerGeneratorComponent.fuelMask != 0)
            {
                _emptyRodIconObj.SetActive(true);
                ItemProto itemProto = LDB.items.Select(ProtoID.I空燃料棒);
                _emptyRodIcon.sprite = itemProto.iconSprite;
                _emptyRodCount.text = ((int)powerGeneratorComponent.productCount).ToString();
                _emptyRodUIButton.tips.itemId = itemProto.ID;
                _emptyRodUIButton.tips.itemCount = (int)powerGeneratorComponent.productCount;
            }
            else
            {
                _emptyRodIconObj.SetActive(false);
                _emptyRodCount.text = "";
                _emptyRodUIButton.tips.itemId = 0;
                _emptyRodUIButton.tips.itemCount = 0;
            }

            _emptyRodUIButton.tips.itemInc = 0;
            _emptyRodUIButton.tips.type = UIButton.ItemTipType.Item;
        }

        private static void OnEmptyRodIconClick()
        {
            UIPowerGeneratorWindow window = UIRoot.instance.uiGame.generatorWindow;

            if (window.generatorId == 0 || window.factory == null || window.player == null) return;

            ref PowerGeneratorComponent component = ref window.powerSystem.genPool[window.generatorId];

            if (component.id != window.generatorId) return;

            if (window.player.inhandItemId > 0 && window.player.inhandItemCount == 0)
            {
                UIRealtimeTip.Popup("不能手动放入物品".Translate());
                window.player.SetHandItems(0, 0);
            }
            else if (window.player.inhandItemId > 0 && window.player.inhandItemCount > 0)
                UIRealtimeTip.Popup("不能手动放入物品".Translate());
            else
            {
                int productId = ProtoID.I空燃料棒;
                var productCount = (int)component.productCount;

                if (window.player.inhandItemId != 0 || window.player.inhandItemCount != 0 || productCount == 0) return;

                component.productCount -= productCount;

                if (VFInput.shift || VFInput.control)
                {
                    int package = window.player.TryAddItemToPackage(productId, productCount, 0, false);
                    UIItemup.Up(productId, package);
                }
                else
                {
                    window.player.SetHandItemId_Unsafe(productId);
                    window.player.SetHandItemCount_Unsafe(productCount);
                    window.player.SetHandItemInc_Unsafe(0);
                }
            }
        }
    }
}
