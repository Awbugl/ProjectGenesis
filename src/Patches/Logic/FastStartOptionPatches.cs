using System.Linq;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.Logic
{
    public static class FastStartOptionPatches
    {
        private static UIToggle _fastStartToggle;

        private static void Init()
        {
            GameObject gameObject = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/setting-group");
            GameObject fastStartObj = Object.Instantiate(GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/setting-group/sandbox-mode/"),
                                                         gameObject.transform, false);

            for (int i = 5; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);
                Transform transform1 = transform.transform;
                Vector3 localPosition = transform1.localPosition;

                localPosition = new Vector3(localPosition.x, localPosition.y - 36, localPosition.z);
                transform1.localPosition = localPosition;
            }
            
            fastStartObj.name = "fast-start-mode";
            fastStartObj.transform.localPosition = new Vector3(0, -244, 0);
            Object.DestroyImmediate(fastStartObj.GetComponent<Localizer>());
            var button = fastStartObj.GetComponentInChildren<UIButton>();
            button.tips.tipTitle = "快速开局".TranslateFromJson();
            button.tips.tipText = "快速开局描述".TranslateFromJson();
            fastStartObj.GetComponent<Text>().text = "快速开局".TranslateFromJson();
            _fastStartToggle = fastStartObj.GetComponentInChildren<UIToggle>();
            _fastStartToggle.isOn = false;
            _fastStartToggle.toggle.onValueChanged.RemoveAllListeners();
            _fastStartToggle.toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private static void OnToggleValueChanged(bool value) => UIRoot._instance.galaxySelect.gameDesc.isFastStartMode = value;

        [HarmonyPatch(typeof(UIGalaxySelect), "UpdateUIDisplay")]
        [HarmonyPostfix]
        public static void UIGalaxySelect_UpdateUIDisplay_Postfix(UIGalaxySelect __instance)
        {
            if (_fastStartToggle) _fastStartToggle.isOn = __instance.gameDesc.isFastStartMode;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGalaxySelect), "_OnOpen")]
        public static void UIGalaxySelect_OnOpen()
        {
            if (!_fastStartToggle) Init();

            _fastStartToggle.isOn = false;
        }

        [HarmonyPatch(typeof(GameData), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            if (DSPGame.IsMenuDemo || !__instance.gameDesc.isFastStartMode) return;

            foreach (TechProto proto in LDB.techs.dataArray)
            {
                if (!GameMain.data.history.TechUnlocked(proto.ID) && proto.Items.All((e) => e < 6003 && e != 5201))
                    GameMain.data.history.UnlockTechUnlimited(proto.ID, true);
            }

            AddItemToPackage(1131, 2000); // 地基
            AddItemToPackage(2003, 900);  // 三级带
            AddItemToPackage(2013, 400);  // 三级爪
            AddItemToPackage(2103, 20);   // 小塔
            AddItemToPackage(5001, 800);  // 小船
            AddItemToPackage(6267, 10);   // 大气
            AddItemToPackage(2201, 98);   // 电线杆
            AddItemToPackage(2202, 4);    // 充电杆
            AddItemToPackage(2107, 20);   // 配送器
            AddItemToPackage(5003, 200);  // 配送小飞机
            AddItemToPackage(2203, 46);   // 风电
            AddItemToPackage(2204, 19);   // 火电
            AddItemToPackage(2205, 49);   // 太阳能
            AddItemToPackage(6221, 50);   // 温差
            AddItemToPackage(2301, 96);   // 矿机
            AddItemToPackage(2302, 96);   // 熔炉
            AddItemToPackage(6230, 30);   // 处理厂
            AddItemToPackage(2303, 46);   // 制造台MK1
            AddItemToPackage(2304, 46);   // 制造台MK2
            AddItemToPackage(2306, 20);   // 抽水站
            AddItemToPackage(2307, 10);   // 抽油机
            AddItemToPackage(2308, 30);   // 精炼厂
            AddItemToPackage(2309, 30);   // 化工厂
            AddItemToPackage(2901, 99);   // 研究站
            AddItemToPackage(6216, 50);   // 燃料棒

            TakeItemFromPlayer(1401, 10);
            TakeItemFromPlayer(2001, 20);
            TakeItemFromPlayer(2011, 5);
            TakeItemFromPlayer(2012, 5);
            TakeItemFromPlayer(2020, 1);
            TakeItemFromPlayer(2030, 5);
            TakeItemFromPlayer(2106, 1);

            GameMain.data.mainPlayer.package.Sort();
        }

        private static void AddItemToPackage(int itemId, int count) => GameMain.data.mainPlayer.TryAddItemToPackage(itemId, count, 0, false);

        private static void TakeItemFromPlayer(int itemId, int count) => GameMain.data.mainPlayer.package.TakeTailItems(ref itemId, ref count, out _);
    }
}
