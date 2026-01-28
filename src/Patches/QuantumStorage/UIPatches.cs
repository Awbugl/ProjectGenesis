using System;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static partial class QuantumStoragePatches
    {
        private static UIOrbitPicker orbitPicker;

        [HarmonyPatch(typeof(UIGame), nameof(UIGame._OnInit))]
        [HarmonyPostfix]
        public static void Init()
        {
            if (orbitPicker) return;

            orbitPicker = Object.Instantiate(UIRoot.instance.uiGame.ejectorWindow.orbitPicker,
                UIRoot.instance.uiGame.storageWindow.transform);

            Transform orbitPickerTransform = orbitPicker.transform;
            ((RectTransform)orbitPickerTransform).localPosition = new Vector3(90, -410, 0);

            //orbitPicker
            //child(0): title
            //child(1): auto-orbit-label
            //child(2): auto-orbit-switch[1]
            //child(3): apply-planet-button[1]
            //child(4): btn-group[22]
            
            Transform titleTransform = orbitPickerTransform.GetChild(0).transform;
            ((RectTransform)titleTransform).localPosition = new Vector3(-120, -65, 0);

            Object.DestroyImmediate(titleTransform.GetComponent<Localizer>());
            Text component = titleTransform.GetComponent<Text>();
            component.fontSize = 16;
            component.text = "选择量子频道".TranslateFromJson();

            orbitPickerTransform.GetChild(1).gameObject.SetActive(false);
            orbitPickerTransform.GetChild(2).gameObject.SetActive(false);
            orbitPickerTransform.GetChild(3).gameObject.SetActive(false);
            
            var btn_group_Transform = orbitPickerTransform.GetChild(4);

            for (var i = 0; i < btn_group_Transform.childCount; i++)
            {
                // btn-group结构是：无、编辑轨道、20个轨道
                // 量子箱只要10个频道，所以保留索引2到11
                Transform transform = btn_group_Transform.GetChild(i);
                transform.gameObject.SetActive(i >= 2 && i <= 11);
            }

            Array.Resize(ref orbitPicker.orbitButtons, 11);

            orbitPicker.UnregInnerEvents();
            orbitPicker.RegInnerEvents();
            orbitPicker.onOrbitButtonClick += OnOrbitPickerButtonClick;
        }

        private static void OnOrbitPickerButtonClick(int orbitId)
        {
            UIStorageWindow uiGameStorageWindow = UIRoot.instance.uiGame.storageWindow;

            if (!uiGameStorageWindow.active || uiGameStorageWindow.factory == null) return;

            QuantumStorageOrbitChange(uiGameStorageWindow.factory.planetId, uiGameStorageWindow.storageId, orbitId);
            uiGameStorageWindow.OnStorageIdChange();
        }

        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnStorageIdChange))]
        [HarmonyPostfix]
        public static void UIStorageWindow_OnStorageIdChange_Postfix(UIStorageWindow __instance)
        {
            if (!__instance.active || __instance.factory == null) return;

            StorageComponent component = __instance.factoryStorage.storagePool[__instance.storageId];

            bool isQuantumStorage = component.size == QuantumStorageSize;

            __instance.bansSlider.transform.parent.gameObject.SetActive(!isQuantumStorage);
            orbitPicker.gameObject.SetActive(isQuantumStorage);

            if (isQuantumStorage) orbitPicker.SetOrbitId(QueryOrbitId(__instance.factory.planetId, __instance.storageId));
        }
    }
}
