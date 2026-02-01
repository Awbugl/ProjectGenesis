using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches
{
    public static class FastStartOptionPatches
    {
        private static UIToggle _fastStartToggle;

        private static void Init()
        {
            var gameObject = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/setting-group/stretch-transform/");
            GameObject fastStartObj =
                Object.Instantiate(GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/setting-group/stretch-transform/sandbox-mode/"),
                    gameObject.transform, false);

            for (var i = 5; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);
                Transform transform1 = transform.transform;
                Vector3 localPosition = transform1.localPosition;

                localPosition = new Vector3(localPosition.x, localPosition.y - 36, localPosition.z);
                transform1.localPosition = localPosition;
            }

            fastStartObj.name = "fast-start-mode";
            fastStartObj.transform.localPosition = new Vector3(0, -180, 0);
            Object.DestroyImmediate(fastStartObj.GetComponent<Localizer>());
            UIButton button = fastStartObj.GetComponentInChildren<UIButton>();
            button.tips.tipTitle = "快速开局".TranslateFromJson();
            button.tips.tipText = "快速开局描述".TranslateFromJson();
            fastStartObj.GetComponent<Text>().text = "快速开局".TranslateFromJson();
            _fastStartToggle = fastStartObj.GetComponentInChildren<UIToggle>();
            _fastStartToggle.isOn = false;
            _fastStartToggle.toggle.onValueChanged.RemoveAllListeners();
            _fastStartToggle.toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private static void OnToggleValueChanged(bool value) => UIRoot._instance.galaxySelect.gameDesc.isFastStartMode = value;

        [HarmonyPatch(typeof(UIGalaxySelect), nameof(UIGalaxySelect.UpdateUIDisplay))]
        [HarmonyPostfix]
        public static void UIGalaxySelect_UpdateUIDisplay_Postfix(UIGalaxySelect __instance)
        {
            if (_fastStartToggle) _fastStartToggle.isOn = __instance.gameDesc.isFastStartMode;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGalaxySelect), nameof(UIGalaxySelect._OnOpen))]
        public static void UIGalaxySelect_OnOpen()
        {
            if (!_fastStartToggle) Init();

            _fastStartToggle.isOn = false;
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.SetForNewGame))]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo || !__instance.gameDesc.isFastStartMode) return;

            foreach (TechProto proto in LDB.techs.dataArray)
            {
                if (!GameMain.data.history.TechUnlocked(proto.ID) && NeedFastUnlock(proto.Items))
                    GameMain.data.history.UnlockTechUnlimited(proto.ID, true);
            }

            AddItemToPackage(1131, 2000); // 地基
            AddItemToPackage(2003, 1200); // 三级带
            AddItemToPackage(2013, 200);  // 三级爪
            AddItemToPackage(2103, 20);   // 小塔
            AddItemToPackage(2104, 3);    // 大塔
            AddItemToPackage(5001, 400);  // 小船
            AddItemToPackage(5002, 8);    // 大船
            AddItemToPackage(6267, 10);   // 大气
            AddItemToPackage(2201, 97);   // 电线杆
            AddItemToPackage(2202, 7);    // 充电杆
            AddItemToPackage(2107, 20);   // 配送器
            AddItemToPackage(5003, 100);  // 配送小飞机
            AddItemToPackage(2203, 46);   // 风电
            AddItemToPackage(2204, 19);   // 火电
            AddItemToPackage(2205, 49);   // 太阳能
            AddItemToPackage(2211, 10);   // 裂变
            AddItemToPackage(2301, 45);   // 矿机
            AddItemToPackage(2302, 36);   // 熔炉
            AddItemToPackage(6230, 30);   // 处理厂
            AddItemToPackage(2303, 16);   // 制造台MK1
            AddItemToPackage(2304, 16);   // 制造台MK2
            AddItemToPackage(2305, 16);   // 制造台MK3
            AddItemToPackage(2306, 20);   // 抽水站
            AddItemToPackage(2307, 10);   // 抽油机
            AddItemToPackage(2308, 30);   // 精炼厂
            AddItemToPackage(2309, 30);   // 化工厂
            AddItemToPackage(2901, 49);   // 研究站
            AddItemToPackage(6241, 60);   // 燃料棒
            AddItemToPackage(3001, 10);   // 机枪
            AddItemToPackage(3002, 20);   // 激光
            AddItemToPackage(3003, 10);   // 加农炮
            AddItemToPackage(3005, 10);   // 导弹
            AddItemToPackage(5102, 24);   // 精准无人机
            AddItemToPackage(1605, 100);  // 炮弹
            AddItemToPackage(1609, 100);  // 导弹
            AddItemToPackage(1603, 100);  // 钛弹
            AddItemToPackage(2020, 49);   // 分流
            AddItemToPackage(2101, 50);   // 小箱子
            AddItemToPackage(2102, 50);   // 大箱子
            AddItemToPackage(2206, 50);   // 电池
            AddItemToPackage(3007, 5);    // 分析基站
            AddItemToPackage(3009, 5);    // 信号塔

            TakeItemFromPlayer(1601, 10);
            TakeItemFromPlayer(1401, 10);
            TakeItemFromPlayer(2001, 20);
            TakeItemFromPlayer(2011, 5);
            TakeItemFromPlayer(2012, 5);
            TakeItemFromPlayer(2030, 5);
            TakeItemFromPlayer(2106, 1);

            GameMain.data.mainPlayer.package.Sort();
        }

        private static bool NeedFastUnlock(int[] items)
        {
            foreach (int item in items)
            {
                switch (item)
                {
                    case ProtoID.I结构矩阵:
                    case ProtoID.I信息矩阵:
                    case ProtoID.I耗散矩阵:
                    case ProtoID.I引力矩阵:
                    case ProtoID.I奇点矩阵:
                    case ProtoID.I宇宙矩阵:
                    case ProtoID.I黑雾矩阵:
                        return false;
                }
            }

            return true;
        }

        private static void AddItemToPackage(int itemId, int count) =>
            GameMain.data.mainPlayer.TryAddItemToPackage(itemId, count, 0, false);

        private static void TakeItemFromPlayer(int itemId, int count) =>
            GameMain.data.mainPlayer.package.TakeTailItems(ref itemId, ref count, out _);
    }
}
