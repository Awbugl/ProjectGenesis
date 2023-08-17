using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Patches.Logic.LithographyAssembler;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;
using Object = UnityEngine.Object;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class LithographyPatches
    {
        private static GameObject _obj;
        private static GameObject _iconObj;
        private static Text _text;
        private static Text _count;
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
                Object.DestroyImmediate(_textobj.GetComponent<Localizer>());
                _text.text = "透镜".TranslateFromJson();
                _text.fontSize = 16;

                oriIconObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel/fuel-icon");
                _iconObj = Object.Instantiate(oriIconObj, parent);
                _iconObj.transform.localScale = Vector3.one;
                _icon = _iconObj.GetComponent<Image>();
                _count = _iconObj.transform.Find("cnt-text").GetComponent<Text>();
                _count.text = "0";
                _count.fontSize = 16;

                var oriButtonObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Generator Window/produce-2/fuel/button");
                var emptyRodButtonObj = Object.Instantiate(oriButtonObj, parent);
                emptyRodButtonObj.transform.localScale = Vector3.one;
                _uiButton = emptyRodButtonObj.GetComponent<UIButton>();
                _uiButton.tips.itemCount = 0;
                _uiButton.tips.itemInc = 0;
                _uiButton.tips.type = UIButton.ItemTipType.IgnoreIncPoint;

                _button = emptyRodButtonObj.GetComponent<Button>();
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnLithographyIconClick);
            }
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), "OnServingBoxChange")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIAssemblerWindow_OnServingBoxChange_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldloc_1),
                                                                     new CodeMatch(OpCodes.Newobj,
                                                                                   AccessTools.Constructor(typeof(StorageComponent),
                                                                                                           new[] { typeof(int) })));


            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Add));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), "SyncServingStorage", new[] { typeof(AssemblerComponent) }, new ArgumentType[] { ArgumentType.Ref })]
        [HarmonyPostfix]
        public static void UIAssemblerWindow_SyncServingStorage_Postfix(
            UIAssemblerWindow __instance,
            ref AssemblerComponent assembler,
            ref StorageComponent ___servingStorage)
        {
            if (assembler.recipeType != (ERecipeType_1)Utils_ERecipeType.电路蚀刻) return;

            var data = LithographyAssemblerPatches.GetLithographyData(__instance.factorySystem.planet.id, __instance.assemblerId);

            ref var grid = ref ___servingStorage.grids[___servingStorage.grids.Length - 1];

            grid.filter = data.ItemId;
            grid.itemId = data.ItemId;
            grid.stackSize = data.NeedCount;
            grid.count = data.ItemCount;
            grid.inc = data.ItemInc;
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), "OnManualServingContentChange")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIAssemblerWindow_OnManualServingContentChange_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).End().Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                                                           new CodeInstruction(OpCodes.Ldarg_0),
                                                                                           new CodeInstruction(OpCodes.Ldfld,
                                                                                                               AccessTools
                                                                                                                  .Field(typeof(UIAssemblerWindow),
                                                                                                                         "servingStorage")),
                                                                                           new CodeInstruction(OpCodes.Ldloc_0),
                                                                                           new CodeInstruction(OpCodes.Call,
                                                                                                               AccessTools
                                                                                                                  .Method(typeof(LithographyPatches),
                                                                                                                          nameof(
                                                                                                                              OnManualServingContentChange_Patch_Method))));

            return matcher.InstructionEnumeration();
        }

        public static void OnManualServingContentChange_Patch_Method(
            UIAssemblerWindow window,
            StorageComponent servingStorage,
            AssemblerComponent assembler)
        {
            if (assembler.recipeType != (ERecipeType_1)Utils_ERecipeType.电路蚀刻) return;

            var data = LithographyAssemblerPatches.GetLithographyData(window.factorySystem.planet.id, window.assemblerId);

            ref var grid = ref servingStorage.grids[servingStorage.grids.Length - 1];

            if (data.ItemId == grid.itemId)
            {
                data.ItemCount = grid.count;
                data.ItemInc = grid.inc;
                data.NeedCount = grid.stackSize;
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

            var value = assemblerComponent.recipeType == (ERecipeType_1)Utils_ERecipeType.电路蚀刻;

            if (value)
                ChangeLithographyData(__instance, assemblerComponent);
            else
                _uiButton.tips.itemId = 0;

            _iconObj.SetActive(value);
            _obj.SetActive(value);
        }

        private static void ChangeLithographyData(UIAssemblerWindow __instance, AssemblerComponent assemblerComponent)
        {
            var data = LithographyAssemblerPatches.GetLithographyData(__instance.factorySystem.planet.id, __instance.assemblerId);
            var lithographyLensId = LithographyAssemblerPatches.GetLithographyLenId(assemblerComponent.recipeId);

            if (data.ItemId != lithographyLensId)
            {
                if (data.ItemCount != 0) LithographyAssemblerPatches.SetEmpty(__instance.factorySystem.planet.id, __instance.assemblerId);

                data.ItemId = lithographyLensId;
                data.ItemCount = 0;
                data.ItemInc = 0;
                data.NeedCount = __instance.factorySystem.assemblerPool[__instance.assemblerId].speed > 60000 ? 20 : 1;

                LithographyAssemblerPatches.SetLithographyData(__instance.factorySystem.planet.id, __instance.assemblerId, data);
            }

            var itemProto = LDB.items.Select(lithographyLensId);
            _icon.sprite = itemProto.iconSprite;
            _text.text = itemProto.name;
            _uiButton.tips.itemId = lithographyLensId;
            _count.text = data.ItemCount.ToString();
            _count.color = data.NeedCount == data.ItemCount ? __instance.workNormalColor : __instance.workStoppedColor;
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

                if (data.ItemCount < data.NeedCount)
                {
                    __instance.stateText.text = "缺少透镜".TranslateFromJson();
                    __instance.stateText.color = __instance.workStoppedColor;
                }
            }
        }

        private static void OnLithographyIconClick()
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

            if (player.inhandItemId == 0 || player.inhandItemCount <= 0 || player.inhandItemId == 1000) return;

            var lithographyLensId = LithographyAssemblerPatches.GetLithographyLenId(assemblerComponent.recipeId);
            var flag = lithographyLensId != 0 && lithographyLensId == player.inhandItemId;

            if (!flag)
            {
                UIRealtimeTip.Popup("不相符的物品".TranslateFromJson());
                return;
            }

            if (data.NeedCount == 0) data.NeedCount = assemblerComponent.speed > 60000 ? 20 : 1;

            if (data.ItemCount >= data.NeedCount)
            {
                UIRealtimeTip.Popup("栏位已满".Translate());
                return;
            }

            var inhandItemCount = player.inhandItemCount;
            var useCount = data.NeedCount - data.ItemCount;
            useCount = useCount > inhandItemCount ? inhandItemCount : useCount;

            var inhandItemInc = player.inhandItemInc;
            var itemInc = inhandItemCount == 0 ? 0 : inhandItemInc * useCount / inhandItemCount;

            data.ItemId = player.inhandItemId;
            data.ItemCount += useCount;
            data.ItemInc = itemInc;

            _count.text = data.ItemCount.ToString();
            _count.color = data.NeedCount == data.ItemCount ? assemblerWindow.workNormalColor : assemblerWindow.workStoppedColor;
            LithographyAssemblerPatches.SetLithographyData(assemblerWindow.factorySystem.planet.id, assemblerWindow.assemblerId, data);

            player.AddHandItemCount_Unsafe(-useCount);
            player.SetHandItemInc_Unsafe(player.inhandItemInc - itemInc);

            if (player.inhandItemCount <= 0)
            {
                player.SetHandItemId_Unsafe(0);
                player.SetHandItemCount_Unsafe(0);
                player.SetHandItemInc_Unsafe(0);
                return;
            }
        }
    }
}
