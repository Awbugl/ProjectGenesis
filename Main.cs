using xiaoye97;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using CommonAPI;
using CommonAPI.Systems;
using ProjectGenesis.Patches;
using EditRecipe = ProjectGenesis.CustomFunction.VanillaRecipeEdit;
using EditItem = ProjectGenesis.CustomFunction.VanillaItemEdit;
using AddRecipe = ProjectGenesis.CustomFunction.CreateRecipe;
using AddItem = ProjectGenesis.CustomFunction.CreateItem;
using EditTech = ProjectGenesis.CustomFunction.VanillaTechEdit;
using AddTech = ProjectGenesis.CustomFunction.CreateTech;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace ProjectGenesis
{
    [BepInPlugin("org.LoShin.GenesisBook", "Genesis", "1.0.0")]
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool")]
    [BepInDependency("dsp.common-api.CommonAPI")]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem))]
    public class Main : BaseUnityPlugin
    {
        public void Awake()
        {
            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", pluginfolder);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);
            int TableID = TabSystem.RegisterTab($@"{"org.LoShin.GenesisBook"}:{"org.LoShin.GenesisBook"}Tab",
                                  new TabData("3", "Assets/texpack/主机科技"));
            int TableID2 = TabSystem.RegisterTab($@"{"org.LoShin.GenesisBook"}:{"org.LoShin.GenesisBook"}Tab2",
                                  new TabData("4", "Assets/texpack/化工科技"));
            LDBTool.PreAddDataAction += LoadCustomData;
            LDBTool.PostAddDataAction += PostAddDataAction;

            TechTreeClear();
            AddRecipe.LoadSprites();

            Harmony.CreateAndPatchAll(typeof(UIPatches));
            Harmony.CreateAndPatchAll(typeof(MultiProductionPatches));
            Harmony.CreateAndPatchAll(typeof(PlanetGasPatches));
            Harmony.CreateAndPatchAll(typeof(OceanDischargePatches));

            //ProtoRegistry.onLoadingFinished += ModifySpeed;
        }

        private void PostAddDataAction()
        {
            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();

            var dataArray = LDB.items.dataArray;

            foreach (var proto in dataArray)
            {
                StorageComponent.itemIsFuel[proto.ID] = proto.HeatValue > 0L;
                StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            var logo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            var image = logo.GetComponent<RawImage>();
            var rectTransform = logo.GetComponent<RectTransform>();

            image.texture = Localization.language == Language.zhCN
                ? AddRecipe.Sprites["中文图标"].texture
                : AddRecipe.Sprites["英文图标"].texture;

            rectTransform.sizeDelta = new Vector2(800f, 500f);
            rectTransform.anchoredPosition
                = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 50f);
        }


        //清除科技树逻辑
        private void TechTreeClear()
        {
            foreach (var tech in 科技.科技树)
            {
                LDB.techs.Select(tech).preTechArray = new TechProto[] { };
                LDB.techs.Select(tech).PreTechs = new int[] { };
                LDB.techs.Select(tech).postTechArray = new TechProto[] { };
            }
        }

        //原版配方修改
        private void LoadCustomData()
        {
            //科技类定义
            var 电路基板 = 科技类.半导体材料.Copy();
            var 电路板 = 科技类.半导体材料.Copy();
            var 基础组件 = 科技类.半导体材料.Copy();
            var 先进组件 = 科技类.半导体材料.Copy();
            var 尖端组件 = 科技类.半导体材料.Copy();
            var 玻璃科技 = 科技类.半导体材料.Copy();
            var 精炼科技 = 科技类.半导体材料.Copy();
            var 生物神经 = 科技类.半导体材料.Copy();
            var 主机科技 = 科技类.半导体材料.Copy();
            var 主机科技2 = 科技类.半导体材料.Copy();
            var 钨钢 = 科技类.半导体材料.Copy();
            var 虚拟技术革新 = 科技类.半导体材料.Copy();
            var 化工科技 = 科技类.半导体材料.Copy();
            var 化工升级 = 科技类.半导体材料.Copy();

            //化工线物品类

            LDB.items.Select(物品.单极磁石).ID = 6980;
            LDB.items.Select(物品.单极磁石).GridIndex = 3516;

            LDB.items.Select(物品.硫酸).ID = 6998;
            LDB.items.Select(物品.硫酸).GridIndex = 4615;

            LDB.strings.Select(2314).name = "剧毒液体海洋";
            LDB.strings.Select(2314).Name = "剧毒液体海洋";

            LDB.items.Select(物品.一级制造台).prefabDesc.assemblerSpeed = 20000;

            var 剧毒有机液体 = ProtoRegistry.RegisterItem(物品.硫酸, "剧毒有机液体",
                                                    "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/剧毒有机液体", 3406, 20);
            剧毒有机液体.miningFrom = "剧毒液体海洋";
            剧毒有机液体.MiningFrom = "剧毒液体海洋";
            剧毒有机液体.preTech = LDB.techs.Select(1);
            剧毒有机液体.DescFields = new int[] { 0 };
            剧毒有机液体.PreTechOverride = 1;
            剧毒有机液体.UnlockKey = -1;


            var 硫酸 = ProtoRegistry.RegisterItem(6999, "硫酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/硫酸", 1207, 20);

            var 乙烯 = ProtoRegistry.RegisterItem(7001, "乙烯", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/乙烯", 3101, 20);
            var 氨 = ProtoRegistry.RegisterItem(7002, "氨", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                               "Assets/texpack/氨", 3102, 20);
            var 苯酚 = ProtoRegistry.RegisterItem(7003, "苯酚", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/苯酚", 3103, 20);
            var 丙酮 = ProtoRegistry.RegisterItem(7004, "丙酮", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/丙酮", 3104, 20);
            var 二氨基联苯胺 = ProtoRegistry.RegisterItem(7005, "二氨基联苯胺",
                                                    "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/二氨基联苯胺", 3105, 20);
            var 二甲苯 = ProtoRegistry.RegisterItem(7006, "二甲苯", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/二甲苯", 3106, 20);
            var 二氯联苯胺 = ProtoRegistry.RegisterItem(7007, "二氯联苯胺", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/二氯联苯胺", 3107, 20);
            var 二硝基氯苯 = ProtoRegistry.RegisterItem(7008, "二硝基氯苯", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/二硝基氯苯", 3108, 20);
            var 甘油 = ProtoRegistry.RegisterItem(7009, "甘油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/甘油", 3109, 20);
            var 环氧氯丙烷 = ProtoRegistry.RegisterItem(7010, "环氧氯丙烷", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/环氧氯丙烷", 3110, 20);
            var 间苯二甲酸二苯酯 = ProtoRegistry.RegisterItem(7011, "间苯二甲酸二苯酯",
                                                      "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                      "Assets/texpack/间苯二甲酸二苯酯", 3111, 20);
            var 邻苯二甲酸 = ProtoRegistry.RegisterItem(7012, "邻苯二甲酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/邻苯二甲酸", 3112, 20);
            var 氯苯 = ProtoRegistry.RegisterItem(7013, "氯苯", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/氯苯", 3201, 20);
            var 氢氯酸 = ProtoRegistry.RegisterItem(7014, "氢氯酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/氢氯酸", 3202, 20);
            var 三氯化铁 = ProtoRegistry.RegisterItem(7015, "三氯化铁", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/三氯化铁", 3203, 20);
            var 双酚A = ProtoRegistry.RegisterItem(7016, "双酚A", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/双酚A", 3204, 20);
            var 硝酸 = ProtoRegistry.RegisterItem(7017, "硝酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/硝酸", 3205, 20);
            var 盐水 = ProtoRegistry.RegisterItem(7018, "盐水", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/盐水", 3206, 20);
            var 煤焦油 = ProtoRegistry.RegisterItem(7218, "煤焦油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/煤焦油", 3207, 20);
            var 塑料基板 = ProtoRegistry.RegisterItem(7501, "塑料基板", "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/塑料基板", 3501, 100);
            var 覆层塑料基板 = ProtoRegistry.RegisterItem(7502, "覆层塑料基板", "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/覆层塑料基板", 3502, 100);
            var 钛蚀刻覆层塑料基板 = ProtoRegistry.RegisterItem(7503, "钛蚀刻覆层塑料基板",
                                                       "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                       "Assets/texpack/钛蚀刻覆层塑料基板", 3503, 200);
            var 生物基板 = ProtoRegistry.RegisterItem(7504, "生物基板", "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/生物基板", 3504, 200);
            var 覆层生物基板 = ProtoRegistry.RegisterItem(7505, "覆层生物基板", "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/覆层生物基板", 3505, 200);
            var 钨蚀刻覆层生物基板 = ProtoRegistry.RegisterItem(7506, "钨蚀刻覆层生物基板",
                                                       "这是一种新的材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                       "Assets/texpack/钨蚀刻覆层生物基板", 3506, 200);
            var 超低功率IC晶片 = ProtoRegistry.RegisterItem(7601, "超低功率IC晶片",
                                                      "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                      "Assets/texpack/超低功率IC晶片", 3601, 200);
            var 低功率IC晶片 = ProtoRegistry.RegisterItem(7602, "低功率IC晶片",
                                                     "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                     "Assets/texpack/低功率IC晶片", 3602, 200);
            var 标准功率IC晶片 = ProtoRegistry.RegisterItem(7603, "标准功率IC晶片",
                                                      "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                      "Assets/texpack/标准功率IC晶片", 3603, 200);
            var 高功率IC晶片 = ProtoRegistry.RegisterItem(7604, "高功率IC晶片",
                                                     "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                     "Assets/texpack/高功率IC晶片", 3604, 200);
            var 超高功率IC晶片 = ProtoRegistry.RegisterItem(7605, "超高功率IC晶片",
                                                      "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                      "Assets/texpack/超高功率IC晶片", 3605, 200);
            var 生物IC晶片 = ProtoRegistry.RegisterItem(7606, "生物IC晶片",
                                                    "一片非金属晶片,世界上精度最高的人造物.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/生物IC晶片", 3606, 200);
            var 硅晶圆 = ProtoRegistry.RegisterItem(7701, "硅晶圆", "电子工业的基础.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/硅晶圆", 3701, 100);
            var 钛掺杂的硅晶圆 = ProtoRegistry.RegisterItem(7702, "钛掺杂的硅晶圆",
                                                     "电子工业的基础.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                     "Assets/texpack/钛掺杂的硅晶圆", 3702, 100);
            var 钨掺杂的硅晶圆 = ProtoRegistry.RegisterItem(7703, "钨掺杂的硅晶圆",
                                                     "电子工业的基础.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                     "Assets/texpack/钨掺杂的硅晶圆", 3703, 100);

            var 钨矿 = ProtoRegistry.RegisterItem(1016, "钨矿", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/钨矿", 1512, 100);
            var 钨块 = ProtoRegistry.RegisterItem(7705, "钨块", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/钨块", 3705, 100);
            var 钨强化玻璃 = ProtoRegistry.RegisterItem(7706, "钨强化玻璃", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/钨强化玻璃", 3706, 100);
            var 钨复合金属 = ProtoRegistry.RegisterItem(7707, "钨复合金属", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/钨复合金属", 3707, 100);
            var 环氧树脂 = ProtoRegistry.RegisterItem(7708, "环氧树脂", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/环氧树脂", 3708, 100);
            var 聚苯并咪唑 = ProtoRegistry.RegisterItem(7709, "聚苯并咪唑", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/聚苯并咪唑", 3709, 100);
            var 活性生物纤维 = ProtoRegistry.RegisterItem(7710, "活性生物纤维", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/活性生物纤维", 3710, 100);
            var 生物微晶元件 = ProtoRegistry.RegisterItem(7801, "生物微晶元件",
                                                    "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/生物微晶元件", 3607, 100);
            var 先进生物电路 = ProtoRegistry.RegisterItem(7802, "先进生物电路",
                                                    "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/先进生物电路", 3608, 100);
            var 超生物计算集群 = ProtoRegistry.RegisterItem(7803, "超生物计算集群",
                                                     "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                     "Assets/texpack/超生物计算集群", 3609, 100);
            var 生物神经纤维 = ProtoRegistry.RegisterItem(7804, "生物神经纤维",
                                                    "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/生物神经纤维", 3610, 100);
            var 超级纳米电脑 = ProtoRegistry.RegisterItem(7805, "超级纳米电脑",
                                                    "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/超级纳米电脑", 3507, 50);
            var 湿件主机 = ProtoRegistry.RegisterItem(7806, "湿件主机",
                                                  "这是一个人类未曾涉及的领域.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/湿件主机", 3508, 50);
            var 钛齿轮 = ProtoRegistry.RegisterItem(7821, "钛齿轮", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/钛齿轮", 3509, 100);
            var 钨齿轮 = ProtoRegistry.RegisterItem(7822, "钨齿轮", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/钨齿轮", 3510, 100);
            var 铜线圈 = ProtoRegistry.RegisterItem(7823, "铜线圈", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/铜线圈", 3305, 100);
            var 石墨线圈 = ProtoRegistry.RegisterItem(7824, "石墨线圈", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/石墨线圈", 3306, 100);
            var 钛线圈 = ProtoRegistry.RegisterItem(7825, "钛线圈", "一种新的制造材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/钛线圈", 3307, 100);
            var 基础机械组件 = ProtoRegistry.RegisterItem(6501, "基础机械组件",
                                                    "一个通用组件包,用途广泛.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/基础机械组件", 3308, 100);
            var 先进机械组件 = ProtoRegistry.RegisterItem(6502, "先进机械组件",
                                                    "一个通用组件包,用途广泛.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/先进机械组件", 3309, 100);
            var 尖端机械组件 = ProtoRegistry.RegisterItem(6503, "尖端机械组件",
                                                    "一个通用组件包,用途广泛.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/尖端机械组件", 3310, 100);
            var 超级机械组件 = ProtoRegistry.RegisterItem(6504, "超级机械组件",
                                                    "这是......什么?\n(To be announced)\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/超级机械组件", 3313, 100);
            var 终极机械组件 = ProtoRegistry.RegisterItem(6505, "终极机械组件",
                                                    "一个通用组件包,用途广泛.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                    "Assets/texpack/终极机械组件", 3314, 100);
            var 创世之书 = ProtoRegistry.RegisterItem(6506, "创世之书",
                                                  "如果你拿到这本书,那我想,你应该已经试着接受这个MOD了.欢迎来到第一阶段,开拓者.\n在这个阶段,你需要做的并不是很多,也不会很难,\n我没有什么能留给你的指点,希望我能在下一个阶段见到你.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/创世之书", 3315, 1);
            var 虚空之书 = ProtoRegistry.RegisterItem(6507, "虚空之书",
                                                  "我曾经无数次仰望星空,幻想银河彼端是否存在另一个文明,\n而现在,你已经踏出了远航星海的第一步.欢迎来到第二阶段,开拓者.\n在这个阶段,钛和硅将成为重要的资源,希望我能在下一个阶段见到你.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/虚空之书", 3316, 1);
            var 起源之书 = ProtoRegistry.RegisterItem(6508, "起源之书",
                                                  "我很高兴你能走到这一步,开拓者.\n欢迎来到最终阶段,宇宙的终点对你来说已经触手可及.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/起源之书", 3317, 1);
            var 空燃料棒 = ProtoRegistry.RegisterItem(6509, "空燃料棒", "呃,也许它还差点什么?\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/空燃料棒", 3311, 30);
            var 氢燃料 = ProtoRegistry.RegisterItem(6531, "氢燃料", "这玩意看上去好像也差点啥.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/氢燃料", 3401, 20);
            var 氘核燃料 = ProtoRegistry.RegisterItem(6532, "氘核燃料", "这玩意看上去好像也差点啥.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/氘核燃料", 3402, 20);
            var 反物质燃料 = ProtoRegistry.RegisterItem(6533, "反物质燃料",
                                                   "这玩意看上去好像也差点啥.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                   "Assets/texpack/反物质燃料", 3403, 20);
            var 氯化钠 = ProtoRegistry.RegisterItem(7101, "氯化钠", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/氯化钠", 3302, 100);
            var 硝酸钠 = ProtoRegistry.RegisterItem(7102, "硝酸钠", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/硝酸钠", 3303, 100);
            var 硫酸钠 = ProtoRegistry.RegisterItem(7103, "硫酸钠", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                 "Assets/texpack/硫酸钠", 3304, 100);
            var 氢氧化钠 = ProtoRegistry.RegisterItem(7020, "氢氧化钠", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                  "Assets/texpack/氢氧化钠", 3301, 100);
            var 氧气 = ProtoRegistry.RegisterItem(7019, "氧气", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>",
                                                "Assets/texpack/氧气", 3404, 20);
            var 重燃油 = ProtoRegistry.RegisterItem(6201, "重燃油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/重燃油", 4101, 20);
            var 轻燃油 = ProtoRegistry.RegisterItem(6202, "轻燃油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/轻燃油", 4102, 20);
            var 含硫重燃油 = ProtoRegistry.RegisterItem(6203, "含硫重燃油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/含硫重燃油", 4103, 20);
            var 含硫轻燃油 = ProtoRegistry.RegisterItem(6204, "含硫轻燃油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/含硫轻燃油", 4104, 20);
            var 一氧化碳 = ProtoRegistry.RegisterItem(6205, "一氧化碳", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/一氧化碳", 4105, 20);
            var 二氧化碳 = ProtoRegistry.RegisterItem(6206, "二氧化碳", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/二氧化碳", 4106, 20);
            var 甲醇 = ProtoRegistry.RegisterItem(6207, "甲醇", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/甲醇", 4107, 20);
            var 乙酸 = ProtoRegistry.RegisterItem(6208, "乙酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/乙酸", 4108, 20);
            var 乙酸甲酯 = ProtoRegistry.RegisterItem(6209, "乙酸甲酯", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/乙酸甲酯", 4109, 20);
            var 次氯酸 = ProtoRegistry.RegisterItem(6210, "次氯酸", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/次氯酸", 4110, 20);
            var 四硝基甲烷 = ProtoRegistry.RegisterItem(6211, "四硝基甲烷", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/四硝基甲烷", 4111, 20);
            var 硝基柴油 = ProtoRegistry.RegisterItem(6212, "硝基柴油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/硝基柴油", 4112, 20);
            var 偏二甲肼 = ProtoRegistry.RegisterItem(6213, "偏二甲肼", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/偏二甲肼", 4201, 20);
            var 火箭燃料 = ProtoRegistry.RegisterItem(6214, "火箭燃料", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/火箭燃料", 4202, 20);
            var 硫化氢 = ProtoRegistry.RegisterItem(6215, "硫化氢", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/硫化氢", 4203, 20);
            var 硝基柴油燃料棒 = ProtoRegistry.RegisterItem(6216, "硝基柴油燃料棒", "它蕴含着巨大的能量....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/硝基柴油燃料棒", 4204, 100);
            var 偏二甲肼燃料棒 = ProtoRegistry.RegisterItem(6217, "偏二甲肼燃料棒", "它蕴含着巨大的能量....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/偏二甲肼燃料棒", 4205, 100);
            var 火箭燃料棒 = ProtoRegistry.RegisterItem(6218, "火箭燃料棒", "它蕴含着巨大的能量....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/火箭燃料棒", 4206, 100);
            var 柴油 = ProtoRegistry.RegisterItem(6219, "柴油", "这是一种新的化工材料.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", "Assets/texpack/柴油", 4207, 20);


            var 氧气配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 乙烯配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 氨配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 苯酚配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 丙酮配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 二氨基联苯胺配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 二甲苯配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 二氯联苯胺配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 二硝基氯苯配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 甘油配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 环氧氯丙烷配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 间苯二甲酸二苯酯配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 邻苯二甲酸配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 氯苯配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 氢氯酸配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 三氯化铁配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 双酚A配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 硝酸配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 盐水配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 煤焦油配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 石油裂化 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 焦油分馏 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 塑料基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 覆层塑料基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 钛蚀刻覆层塑料基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 生物基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 覆层生物基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 钨蚀刻覆层生物基板配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 超低功率IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 低功率IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 标准功率IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 高功率IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 超高功率IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 生物IC晶片配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 硅晶圆配方 = LDB.recipes.Select(配方.铁块).Copy();
            var 钛掺杂的硅晶圆配方 = LDB.recipes.Select(配方.铁块).Copy();
            var 钨掺杂的硅晶圆配方 = LDB.recipes.Select(配方.铁块).Copy();
            var 钨矿配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 钨块配方 = LDB.recipes.Select(配方.铁块).Copy();
            var 钨强化玻璃配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 钨复合金属配方 = LDB.recipes.Select(配方.铁块).Copy();
            var 环氧树脂配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 聚苯并咪唑配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 活性生物纤维配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 生物微晶元件配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 先进生物电路配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 超生物计算集群配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 生物神经纤维配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 超级纳米电脑配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 湿件主机配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 钛齿轮配方 = LDB.recipes.Select(配方.齿轮).Copy();
            var 钨齿轮配方 = LDB.recipes.Select(配方.齿轮).Copy();
            var 铜线圈配方 = LDB.recipes.Select(配方.齿轮).Copy();
            var 石墨线圈配方 = LDB.recipes.Select(配方.齿轮).Copy();
            var 钛线圈配方 = LDB.recipes.Select(配方.齿轮).Copy();
            var 基础机械组件配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 先进机械组件配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 尖端机械组件配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 空燃料棒配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 氢燃料配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 氘核燃料配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 反物质燃料配方 = LDB.recipes.Select(配方.星际物流运输站).Copy();
            var 氢氧化钠配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 氯化钠配方 = LDB.recipes.Select(配方.高效粒子容器).Copy();
            var 高效反物质 = LDB.recipes.Select(配方.光子物质化).Copy();
            var 高效奇异物质 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 极效石墨烯 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 高效重氢 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 精炼氢 = LDB.recipes.Select(配方.等离子精炼).Copy();
            var 剧毒液体配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 高效硅晶圆 = LDB.recipes.Select(配方.高效金刚石).Copy();
            var 高效钛晶圆 = LDB.recipes.Select(配方.高效金刚石).Copy();
            var 高效钨晶圆 = LDB.recipes.Select(配方.高效金刚石).Copy();
            var 剧毒液体缓存 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 燃油精炼 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 高效微晶元件 = LDB.recipes.Select(配方.高效粒子容器).Copy();
            var 高效处理器 = LDB.recipes.Select(配方.高效粒子容器).Copy();
            var 高效量子芯片 = LDB.recipes.Select(配方.高效粒子容器).Copy();
            var 高效硫酸 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 轻燃油脱硫 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 重燃油脱硫 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 柴油配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 四硝基甲烷配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 硝基柴油配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 一氧化碳配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 二氧化碳配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 甲醇配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 偏二甲肼配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 火箭燃料配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 乙酸配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 乙酸甲酯配方 = LDB.recipes.Select(配方.高效石墨烯).Copy();
            var 次氯酸配方 = LDB.recipes.Select(配方.塑料).Copy();
            var 硝基柴油燃料棒配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 偏二甲肼燃料棒配方 = LDB.recipes.Select(配方.电路板).Copy();
            var 火箭燃料棒配方 = LDB.recipes.Select(配方.电路板).Copy();



            //流体
            乙烯.IsFluid = true;
            氨.IsFluid = true;
            苯酚.IsFluid = true;
            丙酮.IsFluid = true;
            二氨基联苯胺.IsFluid = true;
            二甲苯.IsFluid = true;
            二氯联苯胺.IsFluid = true;
            二硝基氯苯.IsFluid = true;
            甘油.IsFluid = true;
            环氧氯丙烷.IsFluid = true;
            间苯二甲酸二苯酯.IsFluid = true;
            邻苯二甲酸.IsFluid = true;
            氯苯.IsFluid = true;
            氢氯酸.IsFluid = true;
            三氯化铁.IsFluid = true;
            双酚A.IsFluid = true;
            硝酸.IsFluid = true;
            盐水.IsFluid = true;
            煤焦油.IsFluid = true;
            剧毒有机液体.IsFluid = true;
            硫酸.IsFluid = true;
            氢燃料.IsFluid = true;
            反物质燃料.IsFluid = true;
            氘核燃料.IsFluid = true;
            氧气.IsFluid = true;
            含硫轻燃油.IsFluid = true;
            含硫重燃油.IsFluid = true;
            轻燃油.IsFluid = true;
            重燃油.IsFluid = true;
            柴油.IsFluid = true;
            四硝基甲烷.IsFluid = true;
            硝基柴油.IsFluid = true;
            偏二甲肼.IsFluid = true;
            火箭燃料.IsFluid = true;
            甲醇.IsFluid = true;
            乙酸.IsFluid = true;
            乙酸甲酯.IsFluid = true;
            硫化氢.IsFluid = true;
            一氧化碳.IsFluid = true;
            次氯酸.IsFluid = true;
            钨复合金属配方.Handcraft = false;
            LDB.items.Select(物品.火力发电机).prefabDesc.useFuelPerTick = 200000;

            //配方数据,载入

            //ProtoRegistry.RegisterRecipe(701, global::ERecipeType.Refine, 120, new int[] { 物品.原油 }, new int[] { 1 },
            //                             new int[] { 乙烯.ID, 甘油.ID, 氯苯.ID }, new int[] { 1, 1, 1 },
            //                             "这是一个新的化学公式.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>", 1812, 2502, "石油裂化",
            //                             "Assets/genesistexpack/石油裂化");

            //石油裂化
            AddRecipe.RecipeID(石油裂化, 701);
            AddRecipe.RecipeName(石油裂化, "石油裂化");
            AddRecipe.RecipeDesc(石油裂化, "这是一个新的化学公式.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(石油裂化, 3402);
            AddRecipe.RecipeData(石油裂化, new int[] { 物品.原油 }, new int[] { 1 }, new int[] { 乙烯.ID, 甘油.ID, 氯苯.ID },
                                 new int[] { 1, 1, 1 });
            AddRecipe.RecipeNeedTime(石油裂化, 2);
            AddRecipe.RecipeIcon(石油裂化, "石油裂化");
            石油裂化.Type = global::ERecipeType.Refine;
            LDBTool.PreAddProto(ProtoType.Recipe, 石油裂化);

            //物品

            //氧气
            AddRecipe.RecipeID(氧气配方, 702);
            AddRecipe.RecipeName(氧气配方, "水电解");
            AddRecipe.RecipeDesc(氧气配方, "这个公式可以为你提供氧气.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(氧气配方, 3405);
            AddRecipe.RecipeData(氧气配方, new int[] { 物品.水 }, new int[] { 2 }, new int[] { 物品.氢, 氧气.ID },
                                 new int[] { 4, 2 });
            AddRecipe.RecipeNeedTime(氧气配方, 2);
            AddRecipe.RecipePreTech(氧气配方, 氧气.preTech);

            AddRecipe.RecipeIcon(氧气配方, "电解水");
            LDBTool.PreAddProto(ProtoType.Recipe, 氧气配方);

            //氢氧化钠
            AddRecipe.RecipeID(氢氧化钠配方, 703);
            AddRecipe.RecipeName(氢氧化钠配方, 氢氧化钠.Name);
            AddRecipe.RecipeDesc(氢氧化钠配方, 氢氧化钠.Description);
            AddRecipe.RecipeGridIndex(氢氧化钠配方, 3403);
            AddRecipe.RecipeData(氢氧化钠配方, new int[] { 盐水.ID }, new int[] { 2 }, new int[] { 氢氧化钠.ID, 氢氯酸.ID },
                                 new int[] { 2, 2 });
            AddRecipe.RecipeNeedTime(氢氧化钠配方, 2);
            AddRecipe.RecipePreTech(氢氧化钠配方, 氢氧化钠.preTech);

            AddRecipe.RecipeIcon(氢氧化钠配方, "氢氧化钠");
            LDBTool.PreAddProto(ProtoType.Recipe, 氢氧化钠配方);
            //三种盐
            AddRecipe.RecipeID(氯化钠配方, 801);
            AddRecipe.RecipeName(氯化钠配方, 氯化钠.Name);
            AddRecipe.RecipeDesc(氯化钠配方, 氯化钠.Description);
            AddRecipe.RecipeGridIndex(氯化钠配方, 3404);
            AddRecipe.RecipeData(氯化钠配方, new int[] { 物品.石矿 }, new int[] { 1 }, new int[] { 氯化钠.ID, 硝酸钠.ID, 硫酸钠.ID },
                                 new int[] { 1, 1, 1 });
            AddRecipe.RecipeNeedTime(氯化钠配方, 3);
            AddRecipe.RecipePreTech(氯化钠配方, 氯化钠.preTech);
            AddRecipe.RecipeHandCraft(氯化钠配方, true);
            LDBTool.PreAddProto(ProtoType.Recipe, 氯化钠配方);
            AddRecipe.RecipeIcon(氯化钠配方, "氯化钠");


            //环氧氯丙烷
            AddRecipe.RecipeID(环氧氯丙烷配方, 704);
            AddRecipe.RecipeName(环氧氯丙烷配方, 环氧氯丙烷.Name);
            AddRecipe.RecipeDesc(环氧氯丙烷配方, 环氧氯丙烷.Description);
            AddRecipe.RecipeGridIndex(环氧氯丙烷配方, 环氧氯丙烷.GridIndex);
            AddRecipe.RecipeData(环氧氯丙烷配方, new int[] { 甘油.ID, 氢氯酸.ID }, new int[] { 1, 1 }, new int[] { 环氧氯丙烷.ID, 物品.水 },
                                 new int[] { 1, 2 });
            AddRecipe.RecipeNeedTime(环氧氯丙烷配方, 2);
            AddRecipe.RecipePreTech(环氧氯丙烷配方, 环氧氯丙烷.preTech);

            AddRecipe.RecipeIcon(环氧氯丙烷配方, "环氧氯丙烷");
            LDBTool.PreAddProto(ProtoType.Recipe, 环氧氯丙烷配方);

            //盐水
            AddRecipe.RecipeID(盐水配方, 705);
            AddRecipe.RecipeName(盐水配方, 盐水.Name);
            AddRecipe.RecipeDesc(盐水配方, 盐水.Description);
            AddRecipe.RecipeGridIndex(盐水配方, 3106);
            AddRecipe.RecipeData(盐水配方, new int[] { 物品.水, 氯化钠.ID }, new int[] { 1, 1 }, new int[] { 盐水.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(盐水配方, 1);
            AddRecipe.RecipePreTech(盐水配方, 盐水.preTech);
            AddRecipe.RecipeIcon(盐水配方, "盐水");

            LDBTool.PreAddProto(ProtoType.Recipe, 盐水配方);

            //双酚A
            AddRecipe.RecipeID(双酚A配方, 706);
            AddRecipe.RecipeName(双酚A配方, 双酚A.Name);
            AddRecipe.RecipeDesc(双酚A配方, 双酚A.Description);
            AddRecipe.RecipeGridIndex(双酚A配方, 3103);
            AddRecipe.RecipeData(双酚A配方, new int[] { 氢氯酸.ID, 苯酚.ID, 丙酮.ID }, new int[] { 1, 2, 1 },
                                 new int[] { 双酚A.ID, 物品.水 }, new int[] { 1, 1 });
            AddRecipe.RecipeNeedTime(双酚A配方, 2);
            AddRecipe.RecipePreTech(双酚A配方, 双酚A.preTech);

            AddRecipe.RecipeIcon(双酚A配方, "双酚A");
            LDBTool.PreAddProto(ProtoType.Recipe, 双酚A配方);

            //硝酸
            AddRecipe.RecipeID(硝酸配方, 707);
            AddRecipe.RecipeName(硝酸配方, 硝酸.Name);
            AddRecipe.RecipeDesc(硝酸配方, 硝酸.Description);
            AddRecipe.RecipeGridIndex(硝酸配方, 3104);
            AddRecipe.RecipeData(硝酸配方, new int[] { 硝酸钠.ID, 6999 }, new int[] { 2, 1 }, new int[] { 硝酸.ID, 硫酸钠.ID },
                                 new int[] { 2, 1 });
            AddRecipe.RecipeNeedTime(硝酸配方, 2);
            AddRecipe.RecipePreTech(硝酸配方, 硝酸.preTech);

            AddRecipe.RecipeIcon(硝酸配方, "硝酸");
            LDBTool.PreAddProto(ProtoType.Recipe, 硝酸配方);
            //二硝基氯苯
            AddRecipe.RecipeID(二硝基氯苯配方, 708);
            AddRecipe.RecipeName(二硝基氯苯配方, 二硝基氯苯.Name);
            AddRecipe.RecipeDesc(二硝基氯苯配方, 二硝基氯苯.Description);
            AddRecipe.RecipeGridIndex(二硝基氯苯配方, 二硝基氯苯.GridIndex);
            AddRecipe.RecipeData(二硝基氯苯配方, new int[] { 硝酸.ID, 氯苯.ID }, new int[] { 1, 1 }, new int[] { 二硝基氯苯.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(二硝基氯苯配方, 1);
            AddRecipe.RecipePreTech(二硝基氯苯配方, 二硝基氯苯.preTech);
            AddRecipe.RecipeIcon(二硝基氯苯配方, "二硝基氯苯");
            LDBTool.PreAddProto(ProtoType.Recipe, 二硝基氯苯配方);
            //二氯联苯胺
            AddRecipe.RecipeID(二氯联苯胺配方, 709);
            AddRecipe.RecipeName(二氯联苯胺配方, 二氯联苯胺.Name);
            AddRecipe.RecipeDesc(二氯联苯胺配方, 二氯联苯胺.Description);
            AddRecipe.RecipeGridIndex(二氯联苯胺配方, 二氯联苯胺.GridIndex);
            AddRecipe.RecipeData(二氯联苯胺配方, new int[] { 二硝基氯苯.ID, 物品.铜块 }, new int[] { 1, 1 }, new int[] { 二氯联苯胺.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(二氯联苯胺配方, 1);
            AddRecipe.RecipePreTech(二氯联苯胺配方, 二氯联苯胺.preTech);

            AddRecipe.RecipeIcon(二氯联苯胺配方, "二氯联苯胺");
            LDBTool.PreAddProto(ProtoType.Recipe, 二氯联苯胺配方);
            //氨
            AddRecipe.RecipeID(氨配方, 710);
            AddRecipe.RecipeName(氨配方, 氨.Name);
            AddRecipe.RecipeDesc(氨配方, 氨.Description);
            AddRecipe.RecipeGridIndex(氨配方, 氨.GridIndex);
            AddRecipe.RecipeData(氨配方, new int[] { 硝酸钠.ID, 物品.水 }, new int[] { 2, 8 },
                                 new int[] { 氨.ID, 氢氧化钠.ID, 氧气.ID }, new int[] { 2, 2, 12 });
            AddRecipe.RecipeNeedTime(氨配方, 1);
            AddRecipe.RecipePreTech(氨配方, 氨.preTech);

            AddRecipe.RecipeIcon(氨配方, "氨");
            LDBTool.PreAddProto(ProtoType.Recipe, 氨配方);
            //二氨基联苯胺
            AddRecipe.RecipeID(二氨基联苯胺配方, 711);
            AddRecipe.RecipeName(二氨基联苯胺配方, 二氨基联苯胺.Name);
            AddRecipe.RecipeDesc(二氨基联苯胺配方, 二氨基联苯胺.Description);
            AddRecipe.RecipeGridIndex(二氨基联苯胺配方, 二氨基联苯胺.GridIndex);
            AddRecipe.RecipeData(二氨基联苯胺配方, new int[] { 二氯联苯胺.ID, 氨.ID }, new int[] { 1, 2 },
                                 new int[] { 二氨基联苯胺.ID, 氢氯酸.ID }, new int[] { 1, 1 });
            AddRecipe.RecipeNeedTime(二氨基联苯胺配方, 2);
            AddRecipe.RecipePreTech(二氨基联苯胺配方, 二氨基联苯胺.preTech);

            AddRecipe.RecipeIcon(二氨基联苯胺配方, "二氨基联苯胺");
            LDBTool.PreAddProto(ProtoType.Recipe, 二氨基联苯胺配方);
            //煤焦油
            AddRecipe.RecipeID(煤焦油配方, 712);
            AddRecipe.RecipeName(煤焦油配方, 煤焦油.Name);
            AddRecipe.RecipeDesc(煤焦油配方, 煤焦油.Description);
            AddRecipe.RecipeGridIndex(煤焦油配方, 3109);
            AddRecipe.RecipeData(煤焦油配方, new int[] { 物品.煤矿 }, new int[] { 1 }, new int[] { 煤焦油.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(煤焦油配方, 1);
            AddRecipe.RecipePreTech(煤焦油配方, 煤焦油.preTech);

            AddRecipe.RecipeIcon(煤焦油配方, "煤焦油");
            LDBTool.PreAddProto(ProtoType.Recipe, 煤焦油配方);
            煤焦油配方.Type = global::ERecipeType.Refine;
            //分馏煤焦油
            AddRecipe.RecipeID(焦油分馏, 713);
            AddRecipe.RecipeName(焦油分馏, "焦油分馏");
            AddRecipe.RecipeDesc(焦油分馏, "这是一个新的化学公式.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(焦油分馏, 3401);
            AddRecipe.RecipeData(焦油分馏, new int[] { 煤焦油.ID }, new int[] { 1 }, new int[] { 苯酚.ID, 二甲苯.ID, 丙酮.ID },
                                 new int[] { 1, 1, 1 });
            AddRecipe.RecipeNeedTime(焦油分馏, 1);
            AddRecipe.RecipePreTech(焦油分馏, 煤焦油.preTech);

            AddRecipe.RecipeIcon(焦油分馏, "煤焦油");
            LDBTool.PreAddProto(ProtoType.Recipe, 焦油分馏);
            //邻苯二甲酸
            AddRecipe.RecipeID(邻苯二甲酸配方, 714);
            AddRecipe.RecipeName(邻苯二甲酸配方, 邻苯二甲酸.Name);
            AddRecipe.RecipeDesc(邻苯二甲酸配方, 邻苯二甲酸.Description);
            AddRecipe.RecipeGridIndex(邻苯二甲酸配方, 邻苯二甲酸.GridIndex);
            AddRecipe.RecipeData(邻苯二甲酸配方, new int[] { 氧气.ID, 二甲苯.ID }, new int[] { 2, 1 }, new int[] { 邻苯二甲酸.ID, 物品.水 },
                                 new int[] { 1, 2 });
            AddRecipe.RecipeNeedTime(邻苯二甲酸配方, 2);
            AddRecipe.RecipePreTech(邻苯二甲酸配方, 邻苯二甲酸.preTech);

            AddRecipe.RecipeIcon(邻苯二甲酸配方, "邻苯二甲酸");
            LDBTool.PreAddProto(ProtoType.Recipe, 邻苯二甲酸配方);
            //间苯二甲酸二苯酯
            AddRecipe.RecipeID(间苯二甲酸二苯酯配方, 715);
            AddRecipe.RecipeName(间苯二甲酸二苯酯配方, 间苯二甲酸二苯酯.Name);
            AddRecipe.RecipeDesc(间苯二甲酸二苯酯配方, 间苯二甲酸二苯酯.Description);
            AddRecipe.RecipeGridIndex(间苯二甲酸二苯酯配方, 间苯二甲酸二苯酯.GridIndex);
            AddRecipe.RecipeData(间苯二甲酸二苯酯配方, new int[] { 苯酚.ID, 邻苯二甲酸.ID }, new int[] { 2, 1 },
                                 new int[] { 间苯二甲酸二苯酯.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(间苯二甲酸二苯酯配方, 1);
            AddRecipe.RecipePreTech(间苯二甲酸二苯酯配方, 间苯二甲酸二苯酯.preTech);

            AddRecipe.RecipeIcon(间苯二甲酸二苯酯配方, "间苯二甲酸二苯酯");
            LDBTool.PreAddProto(ProtoType.Recipe, 间苯二甲酸二苯酯配方);
            //三氯化铁
            AddRecipe.RecipeID(三氯化铁配方, 716);
            AddRecipe.RecipeName(三氯化铁配方, 三氯化铁.Name);
            AddRecipe.RecipeDesc(三氯化铁配方, 三氯化铁.Description);
            AddRecipe.RecipeGridIndex(三氯化铁配方, 3101); //3101
            AddRecipe.RecipeData(三氯化铁配方, new int[] { 氢氯酸.ID, 物品.铁块, 氧气.ID }, new int[] { 12, 4, 6 },
                                 new int[] { 三氯化铁.ID, 物品.水 }, new int[] { 4, 6 });
            AddRecipe.RecipeNeedTime(三氯化铁配方, 6);
            AddRecipe.RecipePreTech(三氯化铁配方, 三氯化铁.preTech);

            AddRecipe.RecipeIcon(三氯化铁配方, "三氯化铁");
            LDBTool.PreAddProto(ProtoType.Recipe, 三氯化铁配方);


            //原版配方修改
            EditRecipe.Item(配方.塑料, new int[] { 乙烯.ID, 氧气.ID });
            EditRecipe.ItemCount(配方.塑料, new int[] { 2, 2 });
            EditRecipe.ResultCount(配方.塑料, new int[] { 1 });
            EditRecipe.NeedTime(配方.塑料, 2);
            EditItem.Name(物品.塑料, "聚乙烯");
            EditItem.Desc(物品.塑料, "一种初级化工产品,广泛应用于各种行业.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");

            //化工线物品制造渲染

            //电路配方
            //生物微晶元件
            AddRecipe.RecipeID(生物微晶元件配方, 791);
            AddRecipe.RecipeName(生物微晶元件配方, 生物微晶元件.Name);
            AddRecipe.RecipeDesc(生物微晶元件配方, 生物微晶元件.Description);
            AddRecipe.RecipeGridIndex(生物微晶元件配方, 生物微晶元件.GridIndex);
            AddRecipe.RecipeData(生物微晶元件配方, new int[] { 生物基板.ID, 物品.微晶元件, 高功率IC晶片.ID }, new int[] { 2, 2, 2 },
                                 new int[] { 生物微晶元件.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(生物微晶元件配方, 2);
            AddRecipe.RecipePreTech(生物微晶元件配方, 生物微晶元件.preTech);
            AddRecipe.RecipeIcon(生物微晶元件配方, "生物微晶元件");
            LDBTool.PreAddProto(ProtoType.Recipe, 生物微晶元件配方);
            //先进生物电路
            AddRecipe.RecipeID(先进生物电路配方, 792);
            AddRecipe.RecipeName(先进生物电路配方, 先进生物电路.Name);
            AddRecipe.RecipeDesc(先进生物电路配方, 先进生物电路.Description);
            AddRecipe.RecipeGridIndex(先进生物电路配方, 先进生物电路.GridIndex);
            AddRecipe.RecipeData(先进生物电路配方, new int[] { 覆层生物基板.ID, 生物微晶元件.ID, 超高功率IC晶片.ID }, new int[] { 2, 2, 2 },
                                 new int[] { 先进生物电路.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(先进生物电路配方, 4);
            AddRecipe.RecipePreTech(先进生物电路配方, 先进生物电路.preTech);
            AddRecipe.RecipeIcon(先进生物电路配方, "先进生物电路");
            LDBTool.PreAddProto(ProtoType.Recipe, 先进生物电路配方);
            //超生物计算集群
            AddRecipe.RecipeID(超生物计算集群配方, 793);
            AddRecipe.RecipeName(超生物计算集群配方, 超生物计算集群.Name);
            AddRecipe.RecipeDesc(超生物计算集群配方, 超生物计算集群.Description);
            AddRecipe.RecipeGridIndex(超生物计算集群配方, 超生物计算集群.GridIndex);
            AddRecipe.RecipeData(超生物计算集群配方, new int[] { 钨蚀刻覆层生物基板.ID, 先进生物电路.ID, 生物IC晶片.ID }, new int[] { 2, 2, 2 },
                                 new int[] { 超生物计算集群.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(超生物计算集群配方, 8);
            AddRecipe.RecipeHandCraft(超生物计算集群配方, false);
            AddRecipe.RecipePreTech(超生物计算集群配方, 超生物计算集群.preTech);
            AddRecipe.RecipeIcon(超生物计算集群配方, "超生物计算集群");
            LDBTool.PreAddProto(ProtoType.Recipe, 超生物计算集群配方);
            //生物神经纤维
            AddRecipe.RecipeID(生物神经纤维配方, 794);
            AddRecipe.RecipeName(生物神经纤维配方, 生物神经纤维.Name);
            AddRecipe.RecipeDesc(生物神经纤维配方, 生物神经纤维.Description);
            AddRecipe.RecipeGridIndex(生物神经纤维配方, 生物神经纤维.GridIndex);
            AddRecipe.RecipeData(生物神经纤维配方, new int[] { 物品.粒子宽带, 活性生物纤维.ID, 物品.信息矩阵 }, new int[] { 2, 2, 2 },
                                 new int[] { 生物神经纤维.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(生物神经纤维配方, 4);
            AddRecipe.RecipePreTech(生物神经纤维配方, 生物神经纤维.preTech);
            AddRecipe.RecipeIcon(生物神经纤维配方, "生物神经纤维");
            LDBTool.PreAddProto(ProtoType.Recipe, 生物神经纤维配方);
            //超级纳米电脑
            AddRecipe.RecipeID(超级纳米电脑配方, 795);
            AddRecipe.RecipeName(超级纳米电脑配方, 超级纳米电脑.Name);
            AddRecipe.RecipeDesc(超级纳米电脑配方, 超级纳米电脑.Description);
            AddRecipe.RecipeGridIndex(超级纳米电脑配方, 超级纳米电脑.GridIndex);
            AddRecipe.RecipeData(超级纳米电脑配方, new int[] { 物品.钛合金, 物品.粒子宽带, 物品.量子芯片 }, new int[] { 50, 20, 20 },
                                 new int[] { 超级纳米电脑.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(超级纳米电脑配方, 60);
            AddRecipe.RecipeHandCraft(超级纳米电脑配方, false);
            AddRecipe.RecipePreTech(超级纳米电脑配方, 超级纳米电脑.preTech);
            AddRecipe.RecipeIcon(超级纳米电脑配方, "超级纳米电脑");
            LDBTool.PreAddProto(ProtoType.Recipe, 超级纳米电脑配方);
            //湿件主机
            AddRecipe.RecipeID(湿件主机配方, 569);
            AddRecipe.RecipeName(湿件主机配方, 湿件主机.Name);
            AddRecipe.RecipeDesc(湿件主机配方, 湿件主机.Description);
            AddRecipe.RecipeGridIndex(湿件主机配方, 湿件主机.GridIndex);
            AddRecipe.RecipeData(湿件主机配方, new int[] { 钨复合金属.ID, 生物神经纤维.ID, 超生物计算集群.ID }, new int[] { 60, 40, 40 },
                                 new int[] { 湿件主机.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(湿件主机配方, 90);
            AddRecipe.RecipeHandCraft(湿件主机配方, false);
            AddRecipe.RecipePreTech(湿件主机配方, 湿件主机.preTech);
            AddRecipe.RecipeIcon(湿件主机配方, "湿件主机");
            LDBTool.PreAddProto(ProtoType.Recipe, 湿件主机配方);
            生物微晶元件配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            先进生物电路配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            超生物计算集群配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            生物神经纤维配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            超级纳米电脑配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            湿件主机配方.Type = (global::ERecipeType)ERecipeType.高精度加工;

            //建立配方

            //塑料基板
            AddRecipe.RecipeID(塑料基板配方, 751);
            AddRecipe.RecipeName(塑料基板配方, 塑料基板.Name);
            AddRecipe.RecipeDesc(塑料基板配方, 塑料基板.Description);
            AddRecipe.RecipeGridIndex(塑料基板配方, 塑料基板.GridIndex);
            AddRecipe.RecipeData(塑料基板配方, new int[] { 物品.塑料, 6999 }, new int[] { 1, 1 }, new int[] { 塑料基板.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(塑料基板配方, 2);
            AddRecipe.RecipePreTech(塑料基板配方, 塑料基板.preTech);
            AddRecipe.RecipeIcon(塑料基板配方, "塑料基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 塑料基板配方);
            //覆层塑料基板
            AddRecipe.RecipeID(覆层塑料基板配方, 752);
            AddRecipe.RecipeName(覆层塑料基板配方, 覆层塑料基板.Name);
            AddRecipe.RecipeDesc(覆层塑料基板配方, 覆层塑料基板.Description);
            AddRecipe.RecipeGridIndex(覆层塑料基板配方, 覆层塑料基板.GridIndex); //覆层塑料基板.GridIndex
            AddRecipe.RecipeData(覆层塑料基板配方, new int[] { 塑料基板.ID, 环氧树脂.ID, 6999 }, new int[] { 1, 2, 1 },
                                 new int[] { 覆层塑料基板.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(覆层塑料基板配方, 2);
            AddRecipe.RecipePreTech(覆层塑料基板配方, 覆层塑料基板.preTech);
            AddRecipe.RecipeIcon(覆层塑料基板配方, "覆层塑料基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 覆层塑料基板配方);
            //钛蚀刻覆层塑料基板
            AddRecipe.RecipeID(钛蚀刻覆层塑料基板配方, 753);
            AddRecipe.RecipeName(钛蚀刻覆层塑料基板配方, 钛蚀刻覆层塑料基板.Name);
            AddRecipe.RecipeDesc(钛蚀刻覆层塑料基板配方, 钛蚀刻覆层塑料基板.Description);
            AddRecipe.RecipeGridIndex(钛蚀刻覆层塑料基板配方, 钛蚀刻覆层塑料基板.GridIndex);
            AddRecipe.RecipeData(钛蚀刻覆层塑料基板配方, new int[] { 覆层塑料基板.ID, 物品.钛块, 6999 }, new int[] { 1, 2, 1 },
                                 new int[] { 钛蚀刻覆层塑料基板.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钛蚀刻覆层塑料基板配方, 2);
            AddRecipe.RecipePreTech(钛蚀刻覆层塑料基板配方, 钛蚀刻覆层塑料基板.preTech);
            AddRecipe.RecipeIcon(钛蚀刻覆层塑料基板配方, "钛蚀刻覆层塑料基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 钛蚀刻覆层塑料基板配方);
            //生物基板
            AddRecipe.RecipeID(生物基板配方, 754);
            AddRecipe.RecipeName(生物基板配方, 生物基板.Name);
            AddRecipe.RecipeDesc(生物基板配方, 生物基板.Description);
            AddRecipe.RecipeGridIndex(生物基板配方, 生物基板.GridIndex);
            AddRecipe.RecipeData(生物基板配方, new int[] { 聚苯并咪唑.ID, 三氯化铁.ID }, new int[] { 2, 2 }, new int[] { 生物基板.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(生物基板配方, 4);
            AddRecipe.RecipePreTech(生物基板配方, 生物基板.preTech);
            AddRecipe.RecipeIcon(生物基板配方, "生物基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 生物基板配方);
            //覆层生物基板
            AddRecipe.RecipeID(覆层生物基板配方, 755);
            AddRecipe.RecipeName(覆层生物基板配方, 覆层生物基板.Name);
            AddRecipe.RecipeDesc(覆层生物基板配方, 覆层生物基板.Description);
            AddRecipe.RecipeGridIndex(覆层生物基板配方, 覆层生物基板.GridIndex);
            AddRecipe.RecipeData(覆层生物基板配方, new int[] { 生物基板.ID, 活性生物纤维.ID, 三氯化铁.ID }, new int[] { 1, 2, 2 },
                                 new int[] { 覆层生物基板.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(覆层生物基板配方, 4);
            AddRecipe.RecipePreTech(覆层生物基板配方, 覆层生物基板.preTech);
            AddRecipe.RecipeIcon(覆层生物基板配方, "覆层生物基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 覆层生物基板配方);
            //钨蚀刻覆层生物基板
            AddRecipe.RecipeID(钨蚀刻覆层生物基板配方, 756);
            AddRecipe.RecipeName(钨蚀刻覆层生物基板配方, 钨蚀刻覆层生物基板.Name);
            AddRecipe.RecipeDesc(钨蚀刻覆层生物基板配方, 钨蚀刻覆层生物基板.Description);
            AddRecipe.RecipeGridIndex(钨蚀刻覆层生物基板配方, 钨蚀刻覆层生物基板.GridIndex);
            AddRecipe.RecipeData(钨蚀刻覆层生物基板配方, new int[] { 覆层生物基板.ID, 钨块.ID, 三氯化铁.ID }, new int[] { 1, 2, 2 },
                                 new int[] { 钨蚀刻覆层生物基板.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钨蚀刻覆层生物基板配方, 4);
            AddRecipe.RecipePreTech(钨蚀刻覆层生物基板配方, 钨蚀刻覆层生物基板.preTech);
            AddRecipe.RecipeIcon(钨蚀刻覆层生物基板配方, "钨蚀刻覆层生物基板");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨蚀刻覆层生物基板配方);
            //超低功率IC晶片
            AddRecipe.RecipeID(超低功率IC晶片配方, 761);
            AddRecipe.RecipeName(超低功率IC晶片配方, 超低功率IC晶片.Name);
            AddRecipe.RecipeDesc(超低功率IC晶片配方, 超低功率IC晶片.Description);
            AddRecipe.RecipeGridIndex(超低功率IC晶片配方, 超低功率IC晶片.GridIndex);
            AddRecipe.RecipeData(超低功率IC晶片配方, new int[] { 硅晶圆.ID }, new int[] { 1 }, new int[] { 超低功率IC晶片.ID },
                                 new int[] { 16 });
            AddRecipe.RecipeNeedTime(超低功率IC晶片配方, 2);
            AddRecipe.RecipePreTech(超低功率IC晶片配方, 超低功率IC晶片.preTech);
            AddRecipe.RecipeIcon(超低功率IC晶片配方, "超低功率IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 超低功率IC晶片配方);
            //低功率IC晶片
            AddRecipe.RecipeID(低功率IC晶片配方, 762);
            AddRecipe.RecipeName(低功率IC晶片配方, 低功率IC晶片.Name);
            AddRecipe.RecipeDesc(低功率IC晶片配方, 低功率IC晶片.Description);
            AddRecipe.RecipeGridIndex(低功率IC晶片配方, 低功率IC晶片.GridIndex);
            AddRecipe.RecipeData(低功率IC晶片配方, new int[] { 硅晶圆.ID }, new int[] { 1 }, new int[] { 低功率IC晶片.ID },
                                 new int[] { 8 });
            AddRecipe.RecipeNeedTime(低功率IC晶片配方, 2);
            AddRecipe.RecipePreTech(低功率IC晶片配方, 低功率IC晶片.preTech);
            AddRecipe.RecipeIcon(低功率IC晶片配方, "低功率IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 低功率IC晶片配方);
            //标准功率IC晶片
            AddRecipe.RecipeID(标准功率IC晶片配方, 763);
            AddRecipe.RecipeName(标准功率IC晶片配方, 标准功率IC晶片.Name);
            AddRecipe.RecipeDesc(标准功率IC晶片配方, 标准功率IC晶片.Description);
            AddRecipe.RecipeGridIndex(标准功率IC晶片配方, 标准功率IC晶片.GridIndex);
            AddRecipe.RecipeData(标准功率IC晶片配方, new int[] { 钛掺杂的硅晶圆.ID }, new int[] { 1 }, new int[] { 标准功率IC晶片.ID },
                                 new int[] { 16 });
            AddRecipe.RecipeNeedTime(标准功率IC晶片配方, 2);
            AddRecipe.RecipePreTech(标准功率IC晶片配方, 低功率IC晶片.preTech);
            AddRecipe.RecipeIcon(标准功率IC晶片配方, "标准功率IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 标准功率IC晶片配方);
            //高功率IC晶片
            AddRecipe.RecipeID(高功率IC晶片配方, 764);
            AddRecipe.RecipeName(高功率IC晶片配方, 高功率IC晶片.Name);
            AddRecipe.RecipeDesc(高功率IC晶片配方, 高功率IC晶片.Description);
            AddRecipe.RecipeGridIndex(高功率IC晶片配方, 高功率IC晶片.GridIndex);
            AddRecipe.RecipeData(高功率IC晶片配方, new int[] { 钛掺杂的硅晶圆.ID }, new int[] { 1 }, new int[] { 高功率IC晶片.ID },
                                 new int[] { 8 });
            AddRecipe.RecipeNeedTime(高功率IC晶片配方, 2);
            AddRecipe.RecipePreTech(高功率IC晶片配方, 高功率IC晶片.preTech);
            AddRecipe.RecipeIcon(高功率IC晶片配方, "高功率IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 高功率IC晶片配方);
            //超高功率IC晶片
            AddRecipe.RecipeID(超高功率IC晶片配方, 765);
            AddRecipe.RecipeName(超高功率IC晶片配方, 超高功率IC晶片.Name);
            AddRecipe.RecipeDesc(超高功率IC晶片配方, 超高功率IC晶片.Description);
            AddRecipe.RecipeGridIndex(超高功率IC晶片配方, 超高功率IC晶片.GridIndex);
            AddRecipe.RecipeData(超高功率IC晶片配方, new int[] { 钨掺杂的硅晶圆.ID }, new int[] { 1 }, new int[] { 超高功率IC晶片.ID },
                                 new int[] { 16 });
            AddRecipe.RecipeNeedTime(超高功率IC晶片配方, 2);
            AddRecipe.RecipePreTech(超高功率IC晶片配方, 超高功率IC晶片.preTech);
            AddRecipe.RecipeIcon(超高功率IC晶片配方, "超高功率IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 超高功率IC晶片配方);
            //生物IC晶片
            AddRecipe.RecipeID(生物IC晶片配方, 766);
            AddRecipe.RecipeName(生物IC晶片配方, 生物IC晶片.Name);
            AddRecipe.RecipeDesc(生物IC晶片配方, 生物IC晶片.Description);
            AddRecipe.RecipeGridIndex(生物IC晶片配方, 生物IC晶片.GridIndex);
            AddRecipe.RecipeData(生物IC晶片配方, new int[] { 钨掺杂的硅晶圆.ID }, new int[] { 1 }, new int[] { 生物IC晶片.ID },
                                 new int[] { 8 });
            AddRecipe.RecipeNeedTime(生物IC晶片配方, 2);
            AddRecipe.RecipePreTech(生物IC晶片配方, 生物IC晶片.preTech);
            AddRecipe.RecipeIcon(生物IC晶片配方, "生物IC晶片");
            LDBTool.PreAddProto(ProtoType.Recipe, 生物IC晶片配方);
            //环氧树脂
            AddRecipe.RecipeID(环氧树脂配方, 771);
            AddRecipe.RecipeName(环氧树脂配方, 环氧树脂.Name);
            AddRecipe.RecipeDesc(环氧树脂配方, 环氧树脂.Description);
            AddRecipe.RecipeGridIndex(环氧树脂配方, 环氧树脂.GridIndex);
            AddRecipe.RecipeData(环氧树脂配方, new int[] { 双酚A.ID, 环氧氯丙烷.ID, 氢氧化钠.ID }, new int[] { 1, 1, 1 },
                                 new int[] { 环氧树脂.ID, 盐水.ID }, new int[] { 1, 1 });
            AddRecipe.RecipeNeedTime(环氧树脂配方, 2);
            AddRecipe.RecipePreTech(环氧树脂配方, 环氧树脂.preTech);
            AddRecipe.RecipeIcon(环氧树脂配方, "环氧树脂");
            LDBTool.PreAddProto(ProtoType.Recipe, 环氧树脂配方);
            //聚苯并咪唑
            AddRecipe.RecipeID(聚苯并咪唑配方, 772);
            AddRecipe.RecipeName(聚苯并咪唑配方, 聚苯并咪唑.Name);
            AddRecipe.RecipeDesc(聚苯并咪唑配方, 聚苯并咪唑.Description);
            AddRecipe.RecipeGridIndex(聚苯并咪唑配方, 聚苯并咪唑.GridIndex);
            AddRecipe.RecipeData(聚苯并咪唑配方, new int[] { 二氨基联苯胺.ID, 间苯二甲酸二苯酯.ID }, new int[] { 1, 1 },
                                 new int[] { 聚苯并咪唑.ID, 苯酚.ID }, new int[] { 1, 1 });
            AddRecipe.RecipeNeedTime(聚苯并咪唑配方, 2);
            AddRecipe.RecipePreTech(聚苯并咪唑配方, 聚苯并咪唑.preTech);
            AddRecipe.RecipeIcon(聚苯并咪唑配方, "聚苯并咪唑");
            LDBTool.PreAddProto(ProtoType.Recipe, 聚苯并咪唑配方);
            //钨矿
            AddRecipe.RecipeID(钨矿配方, 773);
            AddRecipe.RecipeName(钨矿配方, 钨矿.Name);
            AddRecipe.RecipeDesc(钨矿配方, 钨矿.Description);
            AddRecipe.RecipeGridIndex(钨矿配方, 3704);
            AddRecipe.RecipeData(钨矿配方, new int[] { 物品.钛矿, 硝酸.ID }, new int[] { 10, 4 }, new int[] { 钨矿.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(钨矿配方, 6);
            AddRecipe.RecipePreTech(钨矿配方, 钨矿.preTech);
            AddRecipe.RecipeIcon(钨矿配方, "钨矿");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨矿配方);
            //钨块
            AddRecipe.RecipeID(钨块配方, 774);
            AddRecipe.RecipeName(钨块配方, 钨块.Name);
            AddRecipe.RecipeDesc(钨块配方, 钨块.Description);
            AddRecipe.RecipeGridIndex(钨块配方, 钨块.GridIndex);
            AddRecipe.RecipeData(钨块配方, new int[] { 钨矿.ID }, new int[] { 1 }, new int[] { 钨块.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钨块配方, 1);
            AddRecipe.RecipePreTech(钨块配方, 钨块.preTech);
            AddRecipe.RecipeIcon(钨块配方, "钨块");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨块配方);
            //钨强化玻璃
            AddRecipe.RecipeID(钨强化玻璃配方, 775);
            AddRecipe.RecipeName(钨强化玻璃配方, 钨强化玻璃.Name);
            AddRecipe.RecipeDesc(钨强化玻璃配方, 钨强化玻璃.Description);
            AddRecipe.RecipeGridIndex(钨强化玻璃配方, 钨强化玻璃.GridIndex);
            AddRecipe.RecipeData(钨强化玻璃配方, new int[] { 物品.玻璃, 钨块.ID }, new int[] { 2, 1 }, new int[] { 钨强化玻璃.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(钨强化玻璃配方, 2);
            AddRecipe.RecipePreTech(钨强化玻璃配方, 钨强化玻璃.preTech);
            AddRecipe.RecipeIcon(钨强化玻璃配方, "钨强化玻璃");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨强化玻璃配方);
            //钨复合金属
            AddRecipe.RecipeID(钨复合金属配方, 776);
            AddRecipe.RecipeName(钨复合金属配方, 钨复合金属.Name);
            AddRecipe.RecipeDesc(钨复合金属配方, 钨复合金属.Description);
            AddRecipe.RecipeGridIndex(钨复合金属配方, 钨复合金属.GridIndex);
            AddRecipe.RecipeData(钨复合金属配方, new int[] { 物品.钢铁, 物品.钛合金, 钨块.ID }, new int[] { 2, 4, 8 },
                                 new int[] { 钨复合金属.ID }, new int[] { 4 });
            AddRecipe.RecipeNeedTime(钨复合金属配方, 8);
            AddRecipe.RecipePreTech(钨复合金属配方, 钨复合金属.preTech);
            AddRecipe.RecipeIcon(钨复合金属配方, "钨复合金属");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨复合金属配方);

            LDB.veins.Select(14).Name = "钨矿";
            LDB.veins.Select(14).name = "钨矿";
            LDB.items.Select(物品.单极磁石).GridIndex = 3704;

            //电路蚀刻初始化
            LDB.items.Select(物品.二级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.电路蚀刻;
            塑料基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            覆层塑料基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            钛蚀刻覆层塑料基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            生物基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            覆层生物基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            钨蚀刻覆层生物基板配方.Type = (global::ERecipeType)ERecipeType.电路蚀刻;
            //蚀刻机
            EditItem.Name(物品.二级制造台, "电路蚀刻机");
            EditItem.Desc(物品.二级制造台, "一台专门用于生产电路基板的机器.\n <color=\"#7AF0FFC8\">——来自创世之书.</color>");
            LDB.items.Select(物品.二级制造台).prefabDesc.assemblerSpeed = 20000;
            //高精度加工
            LDB.items.Select(物品.三级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.高精度加工;
            EditItem.Name(物品.三级制造台, "高精度装配线");
            EditItem.Desc(物品.三级制造台, "一台用于生产精密仪器的机器,当然,不包括它自己.\n <color=\"#7AF0FFC8\">——来自创世之书.</color>");
            LDB.items.Select(物品.三级制造台).prefabDesc.assemblerSpeed = 20000;
            标准功率IC晶片配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            高功率IC晶片配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            超高功率IC晶片配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            生物IC晶片配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            //晶圆
            AddRecipe.RecipeID(硅晶圆配方, 777);
            AddRecipe.RecipeName(硅晶圆配方, 硅晶圆.Name);
            AddRecipe.RecipeDesc(硅晶圆配方, 硅晶圆.Description);
            AddRecipe.RecipeGridIndex(硅晶圆配方, 硅晶圆.GridIndex);
            AddRecipe.RecipeData(硅晶圆配方, new int[] { 物品.硅块 }, new int[] { 16 }, new int[] { 硅晶圆.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(硅晶圆配方, 8);
            AddRecipe.RecipePreTech(硅晶圆配方, 硅晶圆.preTech);
            AddRecipe.RecipeIcon(硅晶圆配方, "硅晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 硅晶圆配方);
            AddRecipe.RecipeID(钛掺杂的硅晶圆配方, 778);
            AddRecipe.RecipeName(钛掺杂的硅晶圆配方, 钛掺杂的硅晶圆.Name);
            AddRecipe.RecipeDesc(钛掺杂的硅晶圆配方, 钛掺杂的硅晶圆.Description);
            AddRecipe.RecipeGridIndex(钛掺杂的硅晶圆配方, 钛掺杂的硅晶圆.GridIndex);
            AddRecipe.RecipeData(钛掺杂的硅晶圆配方, new int[] { 物品.硅块, 物品.钛块 }, new int[] { 64, 16 }, new int[] { 钛掺杂的硅晶圆.ID },
                                 new int[] { 2 });
            AddRecipe.RecipeNeedTime(钛掺杂的硅晶圆配方, 8);
            AddRecipe.RecipePreTech(钛掺杂的硅晶圆配方, 钛掺杂的硅晶圆.preTech);
            AddRecipe.RecipeIcon(钛掺杂的硅晶圆配方, "钛掺杂的硅晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 钛掺杂的硅晶圆配方);
            AddRecipe.RecipeID(钨掺杂的硅晶圆配方, 779);
            AddRecipe.RecipeName(钨掺杂的硅晶圆配方, 钨掺杂的硅晶圆.Name);
            AddRecipe.RecipeDesc(钨掺杂的硅晶圆配方, 钨掺杂的硅晶圆.Description);
            AddRecipe.RecipeGridIndex(钨掺杂的硅晶圆配方, 钨掺杂的硅晶圆.GridIndex);
            AddRecipe.RecipeData(钨掺杂的硅晶圆配方, new int[] { 物品.硅块, 钨块.ID }, new int[] { 128, 32 }, new int[] { 钨掺杂的硅晶圆.ID },
                                 new int[] { 4 });
            AddRecipe.RecipeNeedTime(钨掺杂的硅晶圆配方, 8);
            AddRecipe.RecipePreTech(钨掺杂的硅晶圆配方, 钨掺杂的硅晶圆.preTech);
            AddRecipe.RecipeIcon(钨掺杂的硅晶圆配方, "钨掺杂的硅晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨掺杂的硅晶圆配方);
            //生物纤维
            AddRecipe.RecipeID(活性生物纤维配方, 780);
            AddRecipe.RecipeName(活性生物纤维配方, 活性生物纤维.Name);
            AddRecipe.RecipeDesc(活性生物纤维配方, 活性生物纤维.Description);
            AddRecipe.RecipeGridIndex(活性生物纤维配方, 活性生物纤维.GridIndex);
            AddRecipe.RecipeData(活性生物纤维配方, new int[] { 物品.有机晶体 }, new int[] { 2 }, new int[] { 活性生物纤维.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(活性生物纤维配方, 2);
            AddRecipe.RecipePreTech(活性生物纤维配方, 活性生物纤维.preTech);
            AddRecipe.RecipeIcon(活性生物纤维配方, "活性生物纤维");
            LDBTool.PreAddProto(ProtoType.Recipe, 活性生物纤维配方);
            //配方初始化
            //钛齿轮
            AddRecipe.RecipeID(钛齿轮配方, 821);
            AddRecipe.RecipeName(钛齿轮配方, 钛齿轮.Name);
            AddRecipe.RecipeDesc(钛齿轮配方, 钛齿轮.Description);
            AddRecipe.RecipeGridIndex(钛齿轮配方, 钛齿轮.GridIndex);
            AddRecipe.RecipeData(钛齿轮配方, new int[] { 物品.钛块 }, new int[] { 1 }, new int[] { 钛齿轮.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钛齿轮配方, 1);
            AddRecipe.RecipePreTech(钛齿轮配方, 钛齿轮.preTech);
            AddRecipe.RecipeIcon(钛齿轮配方, "钛齿轮");
            LDBTool.PreAddProto(ProtoType.Recipe, 钛齿轮配方);
            //钛齿轮
            AddRecipe.RecipeID(钨齿轮配方, 822);
            AddRecipe.RecipeName(钨齿轮配方, 钨齿轮.Name);
            AddRecipe.RecipeDesc(钨齿轮配方, 钨齿轮.Description);
            AddRecipe.RecipeGridIndex(钨齿轮配方, 钨齿轮.GridIndex);
            AddRecipe.RecipeData(钨齿轮配方, new int[] { 钨块.ID }, new int[] { 1 }, new int[] { 钨齿轮.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钨齿轮配方, 1);
            AddRecipe.RecipePreTech(钨齿轮配方, 钨齿轮.preTech);
            AddRecipe.RecipeIcon(钨齿轮配方, "钨齿轮");
            LDBTool.PreAddProto(ProtoType.Recipe, 钨齿轮配方);
            //铜线圈
            AddRecipe.RecipeID(铜线圈配方, 623);
            AddRecipe.RecipeName(铜线圈配方, 铜线圈.Name);
            AddRecipe.RecipeDesc(铜线圈配方, 铜线圈.Description);
            AddRecipe.RecipeGridIndex(铜线圈配方, 3201);
            AddRecipe.RecipeData(铜线圈配方, new int[] { 物品.铜块 }, new int[] { 2 }, new int[] { 铜线圈.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(铜线圈配方, 1);
            AddRecipe.RecipePreTech(铜线圈配方, 铜线圈.preTech);
            AddRecipe.RecipeIcon(铜线圈配方, "铜线圈");
            LDBTool.PreAddProto(ProtoType.Recipe, 铜线圈配方);
            //石墨线圈
            AddRecipe.RecipeID(石墨线圈配方, 624);
            AddRecipe.RecipeName(石墨线圈配方, 石墨线圈.Name);
            AddRecipe.RecipeDesc(石墨线圈配方, 石墨线圈.Description);
            AddRecipe.RecipeGridIndex(石墨线圈配方, 3202);
            AddRecipe.RecipeData(石墨线圈配方, new int[] { 物品.石墨 }, new int[] { 2 }, new int[] { 石墨线圈.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(石墨线圈配方, 1);
            AddRecipe.RecipePreTech(石墨线圈配方, 石墨线圈.preTech);
            AddRecipe.RecipeIcon(石墨线圈配方, "石墨线圈");
            LDBTool.PreAddProto(ProtoType.Recipe, 石墨线圈配方);
            //钛线圈
            AddRecipe.RecipeID(钛线圈配方, 625);
            AddRecipe.RecipeName(钛线圈配方, 钛线圈.Name);
            AddRecipe.RecipeDesc(钛线圈配方, 钛线圈.Description);
            AddRecipe.RecipeGridIndex(钛线圈配方, 3203);
            AddRecipe.RecipeData(钛线圈配方, new int[] { 物品.钛块 }, new int[] { 2 }, new int[] { 钛线圈.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(钛线圈配方, 1);
            AddRecipe.RecipePreTech(钛线圈配方, 钛线圈.preTech);
            AddRecipe.RecipeIcon(钛线圈配方, "钛线圈");
            LDBTool.PreAddProto(ProtoType.Recipe, 钛线圈配方);
            //清除配方
            //配方
            //基础机械组件
            AddRecipe.RecipeID(基础机械组件配方, 641);
            AddRecipe.RecipeName(基础机械组件配方, 基础机械组件.Name);
            AddRecipe.RecipeDesc(基础机械组件配方, 基础机械组件.Description);
            AddRecipe.RecipeGridIndex(基础机械组件配方, 3301);
            AddRecipe.RecipeData(基础机械组件配方, new int[] { 物品.铁块, 物品.齿轮, 物品.电路板 }, new int[] { 2, 2, 1 },
                                 new int[] { 基础机械组件.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(基础机械组件配方, 2);
            AddRecipe.RecipePreTech(基础机械组件配方, 基础机械组件.preTech);
            AddRecipe.RecipeIcon(基础机械组件配方, "基础机械组件");
            LDBTool.PreAddProto(ProtoType.Recipe, 基础机械组件配方);
            //先进机械组件
            AddRecipe.RecipeID(先进机械组件配方, 642);
            AddRecipe.RecipeName(先进机械组件配方, 先进机械组件.Name);
            AddRecipe.RecipeDesc(先进机械组件配方, 先进机械组件.Description);
            AddRecipe.RecipeGridIndex(先进机械组件配方, 3302);
            AddRecipe.RecipeData(先进机械组件配方, new int[] { 基础机械组件.ID, 钛齿轮.ID, 物品.处理器 }, new int[] { 2, 2, 1 },
                                 new int[] { 先进机械组件.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(先进机械组件配方, 4);
            AddRecipe.RecipePreTech(先进机械组件配方, 先进机械组件.preTech);
            AddRecipe.RecipeIcon(先进机械组件配方, "先进机械组件");
            LDBTool.PreAddProto(ProtoType.Recipe, 先进机械组件配方);
            //尖端机械组件
            AddRecipe.RecipeID(尖端机械组件配方, 643);
            AddRecipe.RecipeName(尖端机械组件配方, 尖端机械组件.Name);
            AddRecipe.RecipeDesc(尖端机械组件配方, 尖端机械组件.Description);
            AddRecipe.RecipeGridIndex(尖端机械组件配方, 3303);
            AddRecipe.RecipeData(尖端机械组件配方, new int[] { 先进机械组件.ID, 钨齿轮.ID, 物品.量子芯片 }, new int[] { 2, 2, 1 },
                                 new int[] { 尖端机械组件.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(尖端机械组件配方, 6);
            AddRecipe.RecipePreTech(尖端机械组件配方, 尖端机械组件.preTech);
            AddRecipe.RecipeIcon(尖端机械组件配方, "尖端机械组件");
            LDBTool.PreAddProto(ProtoType.Recipe, 尖端机械组件配方);
            //空燃料棒
            AddRecipe.RecipeID(空燃料棒配方, 644);
            AddRecipe.RecipeName(空燃料棒配方, 空燃料棒.Name);
            AddRecipe.RecipeDesc(空燃料棒配方, 空燃料棒.Description);
            AddRecipe.RecipeGridIndex(空燃料棒配方, 3304);
            AddRecipe.RecipeData(空燃料棒配方, new int[] { 物品.铁块, 物品.玻璃 }, new int[] { 1, 1 }, new int[] { 空燃料棒.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(空燃料棒配方, 2);
            AddRecipe.RecipePreTech(空燃料棒配方, 空燃料棒.preTech);
            AddRecipe.RecipeIcon(空燃料棒配方, "空燃料棒");
            LDBTool.PreAddProto(ProtoType.Recipe, 空燃料棒配方);
            //工业燃料
            //氢燃料
            AddRecipe.RecipeID(氢燃料配方, 681);
            AddRecipe.RecipeName(氢燃料配方, 氢燃料.Name);
            AddRecipe.RecipeDesc(氢燃料配方, 氢燃料.Description);
            AddRecipe.RecipeGridIndex(氢燃料配方, 3406);
            AddRecipe.RecipeData(氢燃料配方, new int[] { 物品.氢 }, new int[] { 6 }, new int[] { 氢燃料.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(氢燃料配方, 2);
            AddRecipe.RecipePreTech(氢燃料配方, 氢燃料.preTech);
            AddRecipe.RecipeIcon(氢燃料配方, "氢燃料");
            LDBTool.PreAddProto(ProtoType.Recipe, 氢燃料配方);
            //氘核燃料
            AddRecipe.RecipeID(氘核燃料配方, 682);
            AddRecipe.RecipeName(氘核燃料配方, 氘核燃料.Name);
            AddRecipe.RecipeDesc(氘核燃料配方, 氘核燃料.Description);
            AddRecipe.RecipeGridIndex(氘核燃料配方, 3407);
            AddRecipe.RecipeData(氘核燃料配方, new int[] { 物品.重氢 }, new int[] { 6 }, new int[] { 氘核燃料.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(氘核燃料配方, 4);
            AddRecipe.RecipePreTech(氘核燃料配方, 氘核燃料.preTech);
            AddRecipe.RecipeIcon(氘核燃料配方, "氘核燃料");
            LDBTool.PreAddProto(ProtoType.Recipe, 氘核燃料配方);
            //反物质燃料
            AddRecipe.RecipeID(反物质燃料配方, 683);
            AddRecipe.RecipeName(反物质燃料配方, 反物质燃料.Name);
            AddRecipe.RecipeDesc(反物质燃料配方, 反物质燃料.Description);
            AddRecipe.RecipeGridIndex(反物质燃料配方, 3408);
            AddRecipe.RecipeData(反物质燃料配方, new int[] { 物品.反物质 }, new int[] { 6 }, new int[] { 反物质燃料.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(反物质燃料配方, 6);
            AddRecipe.RecipePreTech(反物质燃料配方, 反物质燃料.preTech);
            AddRecipe.RecipeIcon(反物质燃料配方, "反物质燃料");
            LDBTool.PreAddProto(ProtoType.Recipe, 反物质燃料配方);

            //杂项配方
            //初始化
            //反物质
            AddRecipe.RecipeID(高效反物质, 541);
            AddRecipe.RecipeName(高效反物质, "质能储存(高效)");
            AddRecipe.RecipeDesc(高效反物质, "这个公式能让你更加高效的生产反物质.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效反物质, 3101);
            AddRecipe.RecipeData(高效反物质, new int[] { 物品.临界光子 }, new int[] { 2 }, new int[] { 物品.反物质 }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(高效反物质, 1);
            AddRecipe.RecipePreTech(高效反物质, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效反物质, "高效反物质");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效反物质);
            //奇异物质
            AddRecipe.RecipeID(高效奇异物质, 542);
            AddRecipe.RecipeName(高效奇异物质, "奇异物质(高效)");
            AddRecipe.RecipeDesc(高效奇异物质, "这个公式能让你更加高效的生产奇异物质.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效奇异物质, 3102);
            AddRecipe.RecipeData(高效奇异物质, new int[] { 物品.硅块, 物品.金刚石 }, new int[] { 1, 1 }, new int[] { 物品.奇异物质 },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(高效奇异物质, 1);
            AddRecipe.RecipePreTech(高效奇异物质, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效奇异物质, "高效奇异物质");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效奇异物质);
            高效奇异物质.Type = global::ERecipeType.Assemble;
            //石墨烯
            AddRecipe.RecipeID(极效石墨烯, 543);
            AddRecipe.RecipeName(极效石墨烯, "石墨烯(极效)");
            AddRecipe.RecipeDesc(极效石墨烯, "这个公式能让你更加高效的生产石墨烯.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(极效石墨烯, 3103);
            AddRecipe.RecipeData(极效石墨烯, new int[] { 物品.可燃冰 }, new int[] { 2 }, new int[] { 物品.石墨烯 }, new int[] { 4 });
            AddRecipe.RecipeNeedTime(极效石墨烯, 2);
            AddRecipe.RecipePreTech(极效石墨烯, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(极效石墨烯, "极效石墨烯");
            LDBTool.PreAddProto(ProtoType.Recipe, 极效石墨烯);
            //重氢
            AddRecipe.RecipeID(高效重氢, 544);
            AddRecipe.RecipeName(高效重氢, "重氢(高效)");
            AddRecipe.RecipeDesc(高效重氢, "这个公式能让你更加高效的生产重氢.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效重氢, 3104);
            AddRecipe.RecipeData(高效重氢, new int[] { 物品.氢 }, new int[] { 2 }, new int[] { 物品.重氢 }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(高效重氢, 1);
            AddRecipe.RecipePreTech(高效重氢, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效重氢, "高效重氢");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效重氢);
            高效重氢.Type = global::ERecipeType.Chemical;
            //精炼氢
            AddRecipe.RecipeID(精炼氢, 545);
            AddRecipe.RecipeName(精炼氢, "等离子精炼(高效)");
            AddRecipe.RecipeDesc(精炼氢, "这个公式能让你更加高效的生产氢.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(精炼氢, 3105);
            AddRecipe.RecipeData(精炼氢, new int[] { 物品.原油 }, new int[] { 2 }, new int[] { 物品.氢 }, new int[] { 4 });
            AddRecipe.RecipeNeedTime(精炼氢, 2);
            AddRecipe.RecipePreTech(精炼氢, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(精炼氢, "精炼氢");
            LDBTool.PreAddProto(ProtoType.Recipe, 精炼氢);
            //有机液体
            AddRecipe.RecipeID(剧毒液体配方, 546);
            AddRecipe.RecipeName(剧毒液体配方, "有机液体离心");
            AddRecipe.RecipeDesc(剧毒液体配方, "这个公式能让你离心有机液体.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(剧毒液体配方, 2512);
            AddRecipe.RecipeData(剧毒液体配方, new int[] { 剧毒有机液体.ID }, new int[] { 2 },
                                 new int[] { 硝酸.ID, 氨.ID, 氢氯酸.ID, 煤焦油.ID }, new int[] { 1, 1, 1, 1 });
            AddRecipe.RecipeNeedTime(剧毒液体配方, 2);
            AddRecipe.RecipePreTech(剧毒液体配方, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(剧毒液体配方, "剧毒有机液体");
            LDBTool.PreAddProto(ProtoType.Recipe, 剧毒液体配方);
            //高效硅晶圆
            AddRecipe.RecipeID(高效硅晶圆, 547);
            AddRecipe.RecipeName(高效硅晶圆, "硅晶圆(高效)");
            AddRecipe.RecipeDesc(高效硅晶圆, "这个公式能让你更加高效的生产晶圆.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效硅晶圆, 2410);
            AddRecipe.RecipeData(高效硅晶圆, new int[] { 物品.硅块, 物品.光栅石 }, new int[] { 8, 4 }, new int[] { 硅晶圆.ID },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(高效硅晶圆, 4);
            AddRecipe.RecipePreTech(高效硅晶圆, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效硅晶圆, "高效硅晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效硅晶圆);

            //高效钛晶圆
            AddRecipe.RecipeID(高效钛晶圆, 548);
            AddRecipe.RecipeName(高效钛晶圆, "含钛硅晶圆(高效)");
            AddRecipe.RecipeDesc(高效钛晶圆, "这个公式能让你更加高效的生产晶圆.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效钛晶圆, 2411);
            AddRecipe.RecipeData(高效钛晶圆, new int[] { 物品.硅块, 物品.钛块, 物品.光栅石 }, new int[] { 32, 8, 8 },
                                 new int[] { 钛掺杂的硅晶圆.ID }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(高效钛晶圆, 4);
            AddRecipe.RecipePreTech(高效钛晶圆, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效钛晶圆, "高效钛晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效钛晶圆);
            //高效钨晶圆
            AddRecipe.RecipeID(高效钨晶圆, 549);
            AddRecipe.RecipeName(高效钨晶圆, "含钨硅晶圆(高效)");
            AddRecipe.RecipeDesc(高效钨晶圆, "这个公式能让你更加高效的生产晶圆.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效钨晶圆, 2412);
            AddRecipe.RecipeData(高效钨晶圆, new int[] { 物品.硅块, 钨块.ID, 物品.光栅石 }, new int[] { 64, 16, 16 },
                                 new int[] { 钨掺杂的硅晶圆.ID }, new int[] { 4 });
            AddRecipe.RecipeNeedTime(高效钨晶圆, 4);
            AddRecipe.RecipePreTech(高效钨晶圆, LDB.techs.Select(科技.宇宙矩阵));
            AddRecipe.RecipeIcon(高效钨晶圆, "高效钨晶圆");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效钨晶圆);

            //高效聚乙烯
            AddRecipe.RecipeID(剧毒液体缓存, 551);
            AddRecipe.RecipeName(剧毒液体缓存, "聚乙烯(高效)");
            AddRecipe.RecipeDesc(剧毒液体缓存, "这个公式能让你更高效的生产聚乙烯.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(剧毒液体缓存, 3101);
            AddRecipe.RecipeData(剧毒液体缓存, new int[] { 物品.有机晶体, 乙烯.ID, 氧气.ID }, new int[] { 3, 1, 1 },
                                 new int[] { 物品.塑料 }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(剧毒液体缓存, 1);
            AddRecipe.RecipePreTech(剧毒液体缓存, LDB.techs.Select(科技.高分子化工));
            AddRecipe.RecipeIcon(剧毒液体缓存, "高效塑料");
            LDBTool.PreAddProto(ProtoType.Recipe, 剧毒液体缓存);
            //燃油精炼
            AddRecipe.RecipeID(燃油精炼, 501);
            AddRecipe.RecipeName(燃油精炼, "燃油精炼");
            AddRecipe.RecipeDesc(燃油精炼, "有人说这是工业的血液....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(燃油精炼, 3201);
            AddRecipe.RecipeData(燃油精炼, new int[] { 物品.原油 }, new int[] { 1 },new int[] { 含硫轻燃油.ID, 含硫重燃油.ID }, new int[] { 1, 3 });
            AddRecipe.RecipeNeedTime(燃油精炼, 4);
            AddRecipe.RecipePreTech(燃油精炼, 精炼科技);
            AddRecipe.RecipeIcon(燃油精炼, "燃油精炼");
            LDBTool.PreAddProto(ProtoType.Recipe, 燃油精炼);
            //轻燃油
            AddRecipe.RecipeID(轻燃油脱硫, 502);
            AddRecipe.RecipeName(轻燃油脱硫, "轻燃油脱硫");
            AddRecipe.RecipeDesc(轻燃油脱硫, "有人说这是工业的血液....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(轻燃油脱硫, 3202);
            AddRecipe.RecipeData(轻燃油脱硫, new int[] { 含硫轻燃油.ID, 物品.氢 }, new int[] { 2, 2 }, new int[] { 轻燃油.ID, 硫化氢.ID }, new int[] { 2, 1 });
            AddRecipe.RecipeNeedTime(轻燃油脱硫, 3);
            AddRecipe.RecipePreTech(轻燃油脱硫, 精炼科技);
            AddRecipe.RecipeIcon(轻燃油脱硫, "轻燃油");
            LDBTool.PreAddProto(ProtoType.Recipe, 轻燃油脱硫);
            //重燃油
            AddRecipe.RecipeID(重燃油脱硫, 503);
            AddRecipe.RecipeName(重燃油脱硫, "重燃油脱硫");
            AddRecipe.RecipeDesc(重燃油脱硫, "有人说这是工业的血液....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(重燃油脱硫, 3203);
            AddRecipe.RecipeData(重燃油脱硫, new int[] { 含硫重燃油.ID, 物品.氢 }, new int[] { 2, 2 }, new int[] { 重燃油.ID, 硫化氢.ID }, new int[] { 2, 1 });
            AddRecipe.RecipeNeedTime(重燃油脱硫, 1);
            AddRecipe.RecipePreTech(重燃油脱硫, 精炼科技);
            AddRecipe.RecipeIcon(重燃油脱硫, "重燃油");
            LDBTool.PreAddProto(ProtoType.Recipe, 重燃油脱硫);
            //柴油
            AddRecipe.RecipeID(柴油配方, 504);
            AddRecipe.RecipeName(柴油配方, "柴油搅拌");
            AddRecipe.RecipeDesc(柴油配方, "有人说这是工业的血液....\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(柴油配方, 3204);
            AddRecipe.RecipeData(柴油配方, new int[] { 轻燃油.ID, 重燃油.ID }, new int[] { 1, 3 }, new int[] { 柴油.ID }, new int[] { 4 });
            AddRecipe.RecipeNeedTime(柴油配方, 3);
            AddRecipe.RecipePreTech(柴油配方, 精炼科技);
            AddRecipe.RecipeIcon(柴油配方, "柴油");
            LDBTool.PreAddProto(ProtoType.Recipe, 柴油配方);
            //一氧化碳
            AddRecipe.RecipeID(一氧化碳配方, 505);
            AddRecipe.RecipeName(一氧化碳配方, "一氧化碳");
            AddRecipe.RecipeDesc(一氧化碳配方, "未充分燃烧的煤会形成一氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(一氧化碳配方, 3301);
            AddRecipe.RecipeData(一氧化碳配方, new int[] { 物品.煤矿, 氧气.ID }, new int[] { 2, 2 }, new int[] { 一氧化碳.ID }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(一氧化碳配方, 4);
            AddRecipe.RecipePreTech(一氧化碳配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(一氧化碳配方, "一氧化碳");
            LDBTool.PreAddProto(ProtoType.Recipe, 一氧化碳配方);
            /*//二氧化碳
            AddRecipe.RecipeID(二氧化碳配方, 506);
            AddRecipe.RecipeName(二氧化碳配方, "二氧化碳");
            AddRecipe.RecipeDesc(二氧化碳配方, "充分燃烧的煤会形成二氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(二氧化碳配方, 3302);
            AddRecipe.RecipeData(二氧化碳配方, new int[] { 物品.煤矿, 氧气.ID }, new int[] { 1, 2 }, new int[] { 一氧化碳.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(二氧化碳配方, 2);
            AddRecipe.RecipePreTech(二氧化碳配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(二氧化碳配方, "二氧化碳");
            LDBTool.PreAddProto(ProtoType.Recipe, 二氧化碳配方);*/
            //甲醇
            AddRecipe.RecipeID(甲醇配方, 507);
            AddRecipe.RecipeName(甲醇配方, "甲醇");
            AddRecipe.RecipeDesc(甲醇配方, "充分燃烧的煤会形成二氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(甲醇配方, 3302);
            AddRecipe.RecipeData(甲醇配方, new int[] { 一氧化碳.ID, 物品.氢 }, new int[] { 1, 4 }, new int[] { 甲醇.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(甲醇配方, 4);
            AddRecipe.RecipePreTech(甲醇配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(甲醇配方, "甲醇");
            LDBTool.PreAddProto(ProtoType.Recipe, 甲醇配方);
            //乙酸
            AddRecipe.RecipeID(乙酸配方, 508);
            AddRecipe.RecipeName(乙酸配方, "乙酸");
            AddRecipe.RecipeDesc(乙酸配方, "充分燃烧的煤会形成二氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(乙酸配方, 3303);
            AddRecipe.RecipeData(乙酸配方, new int[] { 一氧化碳.ID, 物品.氢 }, new int[] { 2, 4 }, new int[] { 乙酸.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(乙酸配方, 4);
            AddRecipe.RecipePreTech(乙酸配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(乙酸配方, "乙酸");
            LDBTool.PreAddProto(ProtoType.Recipe, 乙酸配方);
            //乙酸甲酯
            AddRecipe.RecipeID(乙酸甲酯配方, 509);
            AddRecipe.RecipeName(乙酸甲酯配方, "乙酸甲酯");
            AddRecipe.RecipeDesc(乙酸甲酯配方, "充分燃烧的煤会形成二氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(乙酸甲酯配方, 3304);
            AddRecipe.RecipeData(乙酸甲酯配方, new int[] { 乙酸.ID, 甲醇.ID }, new int[] { 1, 1 }, new int[] { 乙酸甲酯.ID, 物品.水 }, new int[] { 1, 1 });
            AddRecipe.RecipeNeedTime(乙酸甲酯配方, 4);
            AddRecipe.RecipePreTech(乙酸甲酯配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(乙酸甲酯配方, "乙酸甲酯");
            LDBTool.PreAddProto(ProtoType.Recipe, 乙酸甲酯配方);
            //次氯酸
            AddRecipe.RecipeID(次氯酸配方, 510);
            AddRecipe.RecipeName(次氯酸配方, "次氯酸");
            AddRecipe.RecipeDesc(次氯酸配方, "充分燃烧的煤会形成二氧化碳.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(次氯酸配方, 3305);
            AddRecipe.RecipeData(次氯酸配方, new int[] { 氢氯酸.ID, 氧气.ID }, new int[] { 2, 2 }, new int[] { 次氯酸.ID }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(次氯酸配方, 4);
            AddRecipe.RecipePreTech(次氯酸配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(次氯酸配方, "次氯酸");
            LDBTool.PreAddProto(ProtoType.Recipe, 次氯酸配方);
            //四硝基甲烷
            AddRecipe.RecipeID(四硝基甲烷配方, 511);
            AddRecipe.RecipeName(四硝基甲烷配方, "四硝基甲烷");
            AddRecipe.RecipeDesc(四硝基甲烷配方, "它蕴含着巨大的能量....但好像还差些什么\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(四硝基甲烷配方, 3306);
            AddRecipe.RecipeData(四硝基甲烷配方, new int[] { 硝酸.ID, 乙酸甲酯.ID }, new int[] { 4, 4 }, new int[] { 四硝基甲烷.ID, 物品.石墨, 物品.水 }, new int[] { 2, 10, 16 });
            AddRecipe.RecipeNeedTime(四硝基甲烷配方, 6);
            AddRecipe.RecipePreTech(四硝基甲烷配方, 科技类.基础化工);
            AddRecipe.RecipeIcon(四硝基甲烷配方, "四硝基甲烷");
            LDBTool.PreAddProto(ProtoType.Recipe, 四硝基甲烷配方);
            //硝基柴油
            AddRecipe.RecipeID(硝基柴油配方, 512);
            AddRecipe.RecipeName(硝基柴油配方, "硝基柴油");
            AddRecipe.RecipeDesc(硝基柴油配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(硝基柴油配方, 3307);
            AddRecipe.RecipeData(硝基柴油配方, new int[] { 四硝基甲烷.ID, 柴油.ID }, new int[] { 2, 2 }, new int[] { 硝基柴油.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(硝基柴油配方, 8);
            AddRecipe.RecipePreTech(硝基柴油配方, 精炼科技);
            AddRecipe.RecipeIcon(硝基柴油配方, "硝基柴油");
            LDBTool.PreAddProto(ProtoType.Recipe, 硝基柴油配方);
            //偏二甲肼
            AddRecipe.RecipeID(偏二甲肼配方, 513);
            AddRecipe.RecipeName(偏二甲肼配方, "偏二甲肼");
            AddRecipe.RecipeDesc(偏二甲肼配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(偏二甲肼配方, 3401);
            AddRecipe.RecipeData(偏二甲肼配方, new int[] { 次氯酸.ID, 氨.ID, 甲醇.ID }, new int[] { 1, 2, 2 }, new int[] { 偏二甲肼.ID, 氢氯酸.ID }, new int[] { 1, 2 });
            AddRecipe.RecipeNeedTime(偏二甲肼配方, 10);
            AddRecipe.RecipePreTech(偏二甲肼配方, 精炼科技);
            AddRecipe.RecipeIcon(偏二甲肼配方, "偏二甲肼");
            LDBTool.PreAddProto(ProtoType.Recipe, 偏二甲肼配方);
            //火箭燃料
            AddRecipe.RecipeID(火箭燃料配方, 514);
            AddRecipe.RecipeName(火箭燃料配方, "火箭燃料");
            AddRecipe.RecipeDesc(火箭燃料配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(火箭燃料配方, 3402);
            AddRecipe.RecipeData(火箭燃料配方, new int[] { 偏二甲肼.ID, 氧气.ID }, new int[] { 4, 10 }, new int[] { 火箭燃料.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(火箭燃料配方, 15);
            AddRecipe.RecipePreTech(火箭燃料配方, 精炼科技);
            AddRecipe.RecipeIcon(火箭燃料配方, "火箭燃料");
            LDBTool.PreAddProto(ProtoType.Recipe, 火箭燃料配方);
            //柴油棒
            AddRecipe.RecipeID(硝基柴油燃料棒配方, 515);
            AddRecipe.RecipeName(硝基柴油燃料棒配方, "硝基柴油燃料棒");
            AddRecipe.RecipeDesc(硝基柴油燃料棒配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(硝基柴油燃料棒配方, 3403);
            AddRecipe.RecipeData(硝基柴油燃料棒配方, new int[] { 空燃料棒.ID, 硝基柴油.ID }, new int[] { 1, 1 }, new int[] { 硝基柴油燃料棒.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(硝基柴油燃料棒配方, 10);
            AddRecipe.RecipePreTech(硝基柴油燃料棒配方, 精炼科技);
            AddRecipe.RecipeIcon(硝基柴油燃料棒配方, "硝基柴油燃料棒");
            LDBTool.PreAddProto(ProtoType.Recipe, 硝基柴油燃料棒配方);
            //偏二甲肼棒
            AddRecipe.RecipeID(偏二甲肼燃料棒配方, 516);
            AddRecipe.RecipeName(偏二甲肼燃料棒配方, "偏二甲肼燃料棒");
            AddRecipe.RecipeDesc(偏二甲肼燃料棒配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(偏二甲肼燃料棒配方, 3404);
            AddRecipe.RecipeData(偏二甲肼燃料棒配方, new int[] { 空燃料棒.ID, 偏二甲肼.ID }, new int[] { 1, 1 }, new int[] { 偏二甲肼燃料棒.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(偏二甲肼燃料棒配方, 10);
            AddRecipe.RecipePreTech(偏二甲肼燃料棒配方, 精炼科技);
            AddRecipe.RecipeIcon(偏二甲肼燃料棒配方, "偏二甲肼燃料棒");
            LDBTool.PreAddProto(ProtoType.Recipe, 偏二甲肼燃料棒配方);
            //火箭燃料棒
            AddRecipe.RecipeID(火箭燃料棒配方, 517);
            AddRecipe.RecipeName(火箭燃料棒配方, "火箭燃料棒");
            AddRecipe.RecipeDesc(火箭燃料棒配方, "它蕴含着巨大的能量....但好像还差一步\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(火箭燃料棒配方, 3405);
            AddRecipe.RecipeData(火箭燃料棒配方, new int[] { 空燃料棒.ID, 火箭燃料.ID }, new int[] { 1, 1 }, new int[] { 火箭燃料棒.ID }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(火箭燃料棒配方, 10);
            AddRecipe.RecipePreTech(火箭燃料棒配方, 精炼科技);
            AddRecipe.RecipeIcon(火箭燃料棒配方, "火箭燃料棒");
            LDBTool.PreAddProto(ProtoType.Recipe, 火箭燃料棒配方);
            //高效微晶元件
            AddRecipe.RecipeID(高效微晶元件, 518);
            AddRecipe.RecipeName(高效微晶元件, "微晶元件(高效)");
            AddRecipe.RecipeDesc(高效微晶元件, "这个公式能让你更加高效的生产微晶元件.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效微晶元件, 3406);
            AddRecipe.RecipeData(高效微晶元件, new int[] { 覆层塑料基板.ID, 低功率IC晶片.ID }, new int[] { 1, 2 }, new int[] { 物品.微晶元件 }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(高效微晶元件, 1);
            AddRecipe.RecipePreTech(高效微晶元件, 虚拟技术革新);
            AddRecipe.RecipeIcon(高效微晶元件, "高效微晶元件");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效微晶元件);
            //高效处理器
            AddRecipe.RecipeID(高效处理器, 519);
            AddRecipe.RecipeName(高效处理器, "处理器(高效)");
            AddRecipe.RecipeDesc(高效处理器, "这个公式能让你更加高效的生产处理器.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效处理器, 3407);
            AddRecipe.RecipeData(高效处理器, new int[] { 钛蚀刻覆层塑料基板.ID, 标准功率IC晶片.ID }, new int[] { 1, 2 }, new int[] { 物品.处理器 }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(高效处理器, 2);
            AddRecipe.RecipePreTech(高效处理器, 虚拟技术革新);
            AddRecipe.RecipeIcon(高效处理器, "高效处理器");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效处理器); ;
            //高效量子芯片
            AddRecipe.RecipeID(高效量子芯片, 520);
            AddRecipe.RecipeName(高效量子芯片, "量子芯片(高效)");
            AddRecipe.RecipeDesc(高效量子芯片, "这个公式能让你更加高效的生产量子芯片.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效量子芯片, 3408);
            AddRecipe.RecipeData(高效量子芯片, new int[] { 生物基板.ID, 高功率IC晶片.ID }, new int[] { 1, 2 }, new int[] { 物品.量子芯片 }, new int[] { 2 });
            AddRecipe.RecipeNeedTime(高效量子芯片, 3);
            AddRecipe.RecipePreTech(高效量子芯片, 虚拟技术革新);
            AddRecipe.RecipeIcon(高效量子芯片, "高效量子芯片");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效量子芯片);
            //高效硫酸
            AddRecipe.RecipeID(高效硫酸, 521);
            AddRecipe.RecipeName(高效硫酸, "硫酸(高效)");
            AddRecipe.RecipeDesc(高效硫酸, "这个公式能让你更加高效的生产硫酸.\n<color=\"#7AF0FFC8\">——来自创世之书.</color>");
            AddRecipe.RecipeGridIndex(高效硫酸, 3205);
            AddRecipe.RecipeData(高效硫酸, new int[] { 硫化氢.ID, 氧气.ID }, new int[] { 1, 4 }, new int[] { 6999 }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(高效硫酸, 1);
            AddRecipe.RecipePreTech(高效硫酸, 科技类.高分子化工);
            AddRecipe.RecipeIcon(高效硫酸, "高效硫酸");
            LDBTool.PreAddProto(ProtoType.Recipe, 高效硫酸);

            long 初级热值 = LDB.items.Select(物品.金刚石).HeatValue;
            long 中级热值 = LDB.items.Select(物品.氢).HeatValue;
            long 高级热值 = LDB.items.Select(物品.氢燃料棒).HeatValue;
            float 初级效率 = LDB.items.Select(物品.煤矿).ReactorInc;
            float 中级效率 = LDB.items.Select(物品.石墨).ReactorInc;
            float 高级效率 = LDB.items.Select(物品.氢).ReactorInc;
            int[] 燃料显示 = new int[] { 2, 3, 1, 31, 40 };
            乙烯.FuelType = 1;
            乙烯.HeatValue = 初级热值;
            乙烯.ReactorInc = 初级效率;
            乙烯.DescFields = 燃料显示;
            苯酚.FuelType = 1;
            苯酚.HeatValue = 初级热值;
            苯酚.ReactorInc = 初级效率;
            苯酚.DescFields = 燃料显示;
            丙酮.FuelType = 1;
            丙酮.HeatValue = 初级热值;
            丙酮.ReactorInc = 初级效率;
            丙酮.DescFields = 燃料显示;
            二甲苯.FuelType = 1;
            二甲苯.HeatValue = 初级热值;
            二甲苯.ReactorInc = 初级效率;
            二甲苯.DescFields = 燃料显示;
            甘油.FuelType = 1;
            甘油.HeatValue = 中级热值;
            甘油.ReactorInc = 中级效率;
            甘油.DescFields = 燃料显示;
            氯苯.FuelType = 1;
            氯苯.HeatValue = 初级热值;
            氯苯.ReactorInc = 初级效率;
            氯苯.DescFields = 燃料显示;
            煤焦油.FuelType = 1;
            煤焦油.HeatValue = 中级热值;
            煤焦油.ReactorInc = 中级效率;
            煤焦油.DescFields = 燃料显示;
            硝酸.FuelType = 1;
            硝酸.HeatValue = 初级热值;
            硝酸.ReactorInc = 初级效率;
            硝酸.DescFields = 燃料显示;
            硫酸.FuelType = 1;
            硫酸.HeatValue = 初级热值;
            硫酸.ReactorInc = 初级效率;
            硫酸.DescFields = 燃料显示;
            氨.FuelType = 1;
            氨.HeatValue = 初级热值;
            氨.ReactorInc = 初级效率;
            氨.DescFields = 燃料显示;
            甲醇.FuelType = 1;
            甲醇.HeatValue = 中级热值*4;
            甲醇.ReactorInc = 初级效率;
            甲醇.DescFields = 燃料显示;
            乙酸.FuelType = 1;
            乙酸.HeatValue = 初级热值;
            乙酸.ReactorInc = 初级效率;
            乙酸.DescFields = 燃料显示;
            乙酸甲酯.FuelType = 1;
            乙酸甲酯.HeatValue = 中级热值*4;
            乙酸甲酯.ReactorInc = 初级效率;
            乙酸甲酯.DescFields = 燃料显示;
            次氯酸.FuelType = 1;
            次氯酸.HeatValue = 初级热值;
            次氯酸.ReactorInc = 初级效率;
            次氯酸.DescFields = 燃料显示;
            含硫轻燃油.FuelType = 1;
            含硫重燃油.FuelType = 1;
            轻燃油.FuelType = 1;
            重燃油.FuelType = 1;
            柴油.FuelType = 1;
            四硝基甲烷.FuelType = 1;
            四硝基甲烷.HeatValue = 中级热值*8;
            四硝基甲烷.ReactorInc = 中级效率;
            四硝基甲烷.DescFields = 燃料显示;
            硝基柴油.FuelType = 1;
            硝基柴油燃料棒.FuelType = 1;
            偏二甲肼.FuelType = 1;
            偏二甲肼燃料棒.FuelType = 1;
            火箭燃料.FuelType = 1;
            火箭燃料棒.FuelType = 1;
            氢燃料.FuelType = 1;
            氢燃料.ReactorInc = 1f;
            氢燃料.HeatValue = 9000000;
            氢燃料.DescFields = 燃料显示;
            氧气.FuelType = 1;
            氧气.ReactorInc = 1f;
            氧气.HeatValue = 54000000;
            氘核燃料.FuelType = 2;
            氘核燃料.ReactorInc = 1.5f;
            氘核燃料.HeatValue = 600000000;
            氘核燃料.DescFields = 燃料显示;
            反物质燃料.FuelType = 3;
            反物质燃料.ReactorInc = 2.5f;
            反物质燃料.HeatValue = 7200000000;
            反物质燃料.DescFields = 燃料显示;
            LDB.items.Select(物品.氢燃料棒).HeatValue = (long)(54000000 * 1.2f);
            LDB.items.Select(物品.核燃料棒).HeatValue = (long)(600000000 * 1.2f);
            含硫轻燃油.ReactorInc = 0.5f;
            含硫重燃油.ReactorInc = 0.5f;
            轻燃油.ReactorInc = 1f;
            重燃油.ReactorInc = 1f;
            柴油.ReactorInc = 1.5f;
            硝基柴油.ReactorInc = 2f;
            硝基柴油燃料棒.ReactorInc = 2.5f;
            偏二甲肼.ReactorInc = 3f;
            偏二甲肼燃料棒.ReactorInc = 3.5f;
            火箭燃料.ReactorInc = 3.5f;
            火箭燃料棒.ReactorInc = 4f;
            含硫轻燃油.HeatValue = 18000000;
            含硫重燃油.HeatValue = 18000000;
            轻燃油.HeatValue = 30000000;
            重燃油.HeatValue = 30000000;
            柴油.HeatValue = 45000000;
            硝基柴油.HeatValue = 120000000;
            硝基柴油燃料棒.HeatValue = (long)(120000000 * 1.2f);
            偏二甲肼.HeatValue = 150000000;
            偏二甲肼燃料棒.HeatValue = (long)(150000000 * 1.2f);
            火箭燃料.HeatValue = 600000000;
            火箭燃料棒.HeatValue = (long)(600000000 * 1.2f);
            含硫轻燃油.DescFields = 燃料显示;
            含硫重燃油.DescFields = 燃料显示;
            轻燃油.DescFields = 燃料显示;
            重燃油.DescFields = 燃料显示;
            柴油.DescFields = 燃料显示;
            硝基柴油.DescFields = 燃料显示;
            硝基柴油燃料棒.DescFields = 燃料显示;
            偏二甲肼.DescFields = 燃料显示;
            偏二甲肼燃料棒.DescFields = 燃料显示;
            火箭燃料.DescFields = 燃料显示;
            火箭燃料棒.DescFields = 燃料显示;
            氧气.DescFields = new int[] { 0, 1, 2, 3, 31, 40 };
            燃油精炼.Type = global::ERecipeType.Refine;






            //原版配方重做
            EditRecipe.ItemCount(配方.硅石, new int[] { 2 });
            EditRecipe.NeedTime(配方.硅石, 1);
            EditRecipe.ItemCount(配方.高纯硅块, new int[] { 1 });
            EditRecipe.NeedTime(配方.高纯硅块, 1);
            EditRecipe.ItemCount(配方.高能石墨, new int[] { 1 });
            EditRecipe.NeedTime(配方.高能石墨, 1);
            EditRecipe.NeedTime(配方.磁铁, 1);
            EditRecipe.Item(配方.钛合金, new int[] { 物品.钢铁, 物品.钛块 });
            EditRecipe.ItemCount(配方.钛合金, new int[] { 4, 4 });
            EditRecipe.NeedTime(配方.钛合金, 6);
            EditRecipe.NeedTime(配方.高效金刚石, 1);
            EditRecipe.Item(配方.钛化玻璃, new int[] { 物品.玻璃, 物品.钛块 });
            EditRecipe.ItemCount(配方.钛化玻璃, new int[] { 2, 1 });
            EditRecipe.ResultCount(配方.钛化玻璃, new int[] { 1 });
            EditRecipe.NeedTime(配方.钛化玻璃, 4);
            EditRecipe.ItemCount(配方.钢材, new int[] { 2 });
            EditRecipe.NeedTime(配方.钢材, 2);
            EditRecipe.Item(配方.电动机, new int[] { 物品.铁块, 铜线圈.ID });
            EditRecipe.ItemCount(配方.电动机, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.电动机, 2);
            EditRecipe.Item(配方.电磁涡轮, new int[] { 物品.电动机, 石墨线圈.ID });
            EditRecipe.ItemCount(配方.电磁涡轮, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.电磁涡轮, 2);
            EditRecipe.Item(配方.超级磁场环, new int[] { 物品.电磁涡轮, 钛线圈.ID });
            EditRecipe.ItemCount(配方.超级磁场环, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.超级磁场环, 2);
            EditRecipe.Item(配方.粒子宽带, new int[] { 物品.碳纳米管, 物品.钛化玻璃 });
            EditRecipe.ItemCount(配方.粒子宽带, new int[] { 2, 2 });
            EditRecipe.NeedTime(配方.粒子宽带, 4);
            EditRecipe.ItemCount(配方.电路板, new int[] { 1, 1 });
            EditRecipe.Item(配方.微晶元件, new int[] { 塑料基板.ID, 物品.电路板, 超低功率IC晶片.ID });
            EditRecipe.ItemCount(配方.微晶元件, new int[] { 2, 2, 4 });
            EditRecipe.NeedTime(配方.微晶元件, 2);
            EditRecipe.Item(配方.处理器, new int[] { 覆层塑料基板.ID, 物品.微晶元件, 低功率IC晶片.ID });
            EditRecipe.ItemCount(配方.处理器, new int[] { 2, 1, 4 });
            EditRecipe.NeedTime(配方.处理器, 4);
            EditRecipe.Item(配方.量子芯片, new int[] { 钛蚀刻覆层塑料基板.ID, 物品.处理器, 标准功率IC晶片.ID });
            EditRecipe.ItemCount(配方.量子芯片, new int[] { 2, 2, 8 });
            EditRecipe.NeedTime(配方.量子芯片, 6);
            EditRecipe.Item(配方.硫酸, new int[] { 硫酸钠.ID, 物品.水 });
            EditRecipe.ItemCount(配方.硫酸, new int[] { 1, 2 });
            EditRecipe.Result(配方.硫酸, new int[] { 6999, 氢氧化钠.ID });
            EditRecipe.ResultCount(配方.硫酸, new int[] { 1, 2 });
            EditRecipe.NeedTime(配方.硫酸, 1);
            EditRecipe.Item(配方.石墨烯, new int[] { 物品.石墨, 6999 });
            EditRecipe.ItemCount(配方.石墨烯, new int[] { 4, 1 });
            EditRecipe.NeedTime(配方.石墨烯, 2);
            EditRecipe.Item(配方.碳纳米管, new int[] { 物品.石墨烯 });
            EditRecipe.ItemCount(配方.碳纳米管, new int[] { 4 });
            EditRecipe.NeedTime(配方.碳纳米管, 2);
            EditRecipe.Item(配方.高效碳纳米管, new int[] { 物品.刺笋结晶 });
            EditRecipe.ItemCount(配方.高效碳纳米管, new int[] { 1 });
            EditRecipe.NeedTime(配方.高效碳纳米管, 1);
            EditRecipe.Item(配方.氢燃料棒, new int[] { 空燃料棒.ID, 氢燃料.ID });
            EditRecipe.ItemCount(配方.氢燃料棒, new int[] { 1, 1 });
            EditRecipe.ResultCount(配方.氢燃料棒, new int[] { 1 });
            EditRecipe.NeedTime(配方.氢燃料棒, 2);
            EditRecipe.Item(配方.氘核燃料棒, new int[] { 空燃料棒.ID, 氘核燃料.ID });
            EditRecipe.ItemCount(配方.氘核燃料棒, new int[] { 1, 1 });
            EditRecipe.ResultCount(配方.氘核燃料棒, new int[] { 1 });
            EditRecipe.NeedTime(配方.氘核燃料棒, 4);
            EditRecipe.Item(配方.反物质燃料棒, new int[] { 空燃料棒.ID, 反物质燃料.ID });
            EditRecipe.ItemCount(配方.反物质燃料棒, new int[] { 1, 1 });
            EditRecipe.ResultCount(配方.反物质燃料棒, new int[] { 1 });
            EditRecipe.NeedTime(配方.反物质燃料棒, 8);
            EditRecipe.Item(配方.推进器, new int[] { 物品.钢铁, 物品.电磁涡轮 });
            EditRecipe.ItemCount(配方.推进器, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.推进器, 2);
            EditRecipe.Item(配方.加力推进器, new int[] { 物品.推进器, 物品.钛合金, 物品.超级磁场环 });
            EditRecipe.ItemCount(配方.加力推进器, new int[] { 2, 1, 1 });
            EditRecipe.NeedTime(配方.加力推进器, 4);
            EditRecipe.Item(配方.物流运输机, new int[] { 物品.铁块, 物品.推进器 });
            EditRecipe.ItemCount(配方.物流运输机, new int[] { 4, 2 });
            EditRecipe.NeedTime(配方.物流运输机, 2);
            EditRecipe.Item(配方.星际物流运输船, new int[] { 物品.钛合金, 物品.加力推进器 });
            EditRecipe.ItemCount(配方.星际物流运输船, new int[] { 8, 2 });
            EditRecipe.NeedTime(配方.星际物流运输船, 4);
            EditRecipe.Item(配方.小型运载火箭, new int[] { 物品.戴森球组件, 物品.量子芯片, 物品.加力推进器 });
            EditRecipe.ItemCount(配方.小型运载火箭, new int[] { 4, 1, 2 });
            EditRecipe.NeedTime(配方.小型运载火箭, 2);
            EditRecipe.Item(配方.戴森球组件, new int[] { 物品.框架材料, 物品.太阳帆 });
            EditRecipe.ItemCount(配方.戴森球组件, new int[] { 4, 1 });
            EditRecipe.NeedTime(配方.戴森球组件, 1);
            EditRecipe.Item(配方.框架材料, new int[] { 钨复合金属.ID, 聚苯并咪唑.ID });
            EditRecipe.ItemCount(配方.框架材料, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.框架材料, 1);
            EditRecipe.Item(配方.太阳帆, new int[] { 聚苯并咪唑.ID, 物品.处理器 });
            EditRecipe.ItemCount(配方.太阳帆, new int[] { 1, 1 });
            EditRecipe.ResultCount(配方.太阳帆, new int[] { 6 });
            EditRecipe.NeedTime(配方.太阳帆, 2);
            EditRecipe.Item(配方.电力感应塔, new int[] { 物品.铁块, 铜线圈.ID });
            EditRecipe.ItemCount(配方.电力感应塔, new int[] { 1, 1 });
            LDB.items.Select(物品.电力感应塔).prefabDesc.powerConnectDistance = 44.5f;
            LDB.items.Select(物品.电力感应塔).prefabDesc.powerCoverRadius = 20.5f;
            EditRecipe.Item(配方.电浆激发器, new int[] { 物品.玻璃, 铜线圈.ID });
            EditRecipe.ItemCount(配方.电浆激发器, new int[] { 1, 2 });
            EditRecipe.Item(配方.无线输电塔, new int[] { 物品.电力感应塔, 物品.电浆激发器 });
            EditRecipe.ItemCount(配方.无线输电塔, new int[] { 1, 1 });
            EditRecipe.Item(配方.风力涡轮机, new int[] { 物品.铁块, 物品.电动机 });
            EditRecipe.ItemCount(配方.风力涡轮机, new int[] { 4, 1 });
            EditRecipe.NeedTime(配方.风力涡轮机, 2);
            LDB.items.Select(物品.风力涡轮机).prefabDesc.powerConnectDistance = 32.5f;
            LDB.items.Select(物品.风力涡轮机).prefabDesc.powerCoverRadius = 14.9f;
            LDB.items.Select(物品.风力涡轮机).prefabDesc.genEnergyPerTick = 50000;
            EditRecipe.Item(配方.火力发电厂, new int[] { 物品.钢铁, 物品.石材, 物品.电动机, 物品.电路板 });
            EditRecipe.ItemCount(配方.火力发电厂, new int[] { 10, 10, 4, 4 });
            EditRecipe.NeedTime(配方.火力发电厂, 4);
            LDB.items.Select(物品.火力发电机).prefabDesc.genEnergyPerTick = 200000;
            EditRecipe.Item(配方.太阳能板, new int[] { 物品.铁块, 物品.硅块, 物品.电路板 });
            EditRecipe.ItemCount(配方.太阳能板, new int[] { 4, 2, 1 });
            EditRecipe.NeedTime(配方.太阳能板, 4);
            LDB.items.Select(物品.太阳能板).prefabDesc.genEnergyPerTick = 40000;
            EditRecipe.Item(配方.蓄电器, new int[] { 物品.铁块, 物品.晶格硅, 物品.电路板 });
            EditRecipe.ItemCount(配方.蓄电器, new int[] { 4, 2, 1 });
            EditRecipe.NeedTime(配方.蓄电器, 4);
            EditRecipe.Item(配方.地热发电站, new int[] { 物品.火力发电机, 物品.钛合金, 物品.处理器 });
            EditRecipe.ItemCount(配方.地热发电站, new int[] { 2, 16, 4 });
            LDB.items.Select(物品.地热发电机).prefabDesc.genEnergyPerTick = 400000;
            EditRecipe.Item(配方.微型聚变发电站, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.超级磁场环, 物品.量子芯片 });
            EditRecipe.ItemCount(配方.微型聚变发电站, new int[] { 10, 40, 16, 6 });
            EditRecipe.NeedTime(配方.微型聚变发电站, 30);
            LDB.items.Select(物品.聚变发电机).prefabDesc.genEnergyPerTick = 2500000;
            EditRecipe.Item(配方.能量枢纽, new int[] { 物品.钛合金, 物品.电磁涡轮, 物品.处理器 });
            EditRecipe.ItemCount(配方.能量枢纽, new int[] { 20, 8, 2 });
            EditRecipe.NeedTime(配方.能量枢纽, 10);
            EditRecipe.Item(配方.人造恒星, new int[] { 尖端机械组件.ID, 物品.框架材料, 物品.湮灭约束球, 湿件主机.ID });
            EditRecipe.ItemCount(配方.人造恒星, new int[] { 60, 120, 60, 5 });
            EditRecipe.NeedTime(配方.人造恒星, 300);
            LDB.items.Select(物品.人造恒星).prefabDesc.genEnergyPerTick = 120000000;
            LDB.milestones.Select(9).name = "钨";
            LDB.milestones.Select(9).Name = "钨";
            LDB.milestones.Select(9).defaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";
            LDB.milestones.Select(9).DefaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";
            EditRecipe.Item(配方.射线接收站, new int[] { 尖端机械组件.ID, 钨复合金属.ID, 物品.太阳帆, 先进生物电路.ID });
            EditRecipe.ItemCount(配方.射线接收站, new int[] { 2, 10, 6, 2 });
            EditRecipe.NeedTime(配方.射线接收站, 6);
            //LDB.items.Select(物品.射线接收站).prefabDesc.powerProductHeat = 120000000;
            EditRecipe.Item(配方.低速传送带, new int[] { 物品.铁块, 物品.电动机 });
            EditRecipe.ItemCount(配方.低速传送带, new int[] { 1, 1 });
            EditRecipe.Item(配方.高速传送带, new int[] { 物品.低速传送带, 物品.电磁涡轮 });
            EditRecipe.ItemCount(配方.高速传送带, new int[] { 3, 1 });
            EditRecipe.Item(配方.极速传送带, new int[] { 物品.高速传送带, 物品.超级磁场环 });
            EditRecipe.ItemCount(配方.极速传送带, new int[] { 3, 1 });
            LDB.items.Select(物品.低速传送带).prefabDesc.beltSpeed = 3;
            LDB.items.Select(物品.高速传送带).prefabDesc.beltSpeed = 5;
            LDB.items.Select(物品.极速传送带).prefabDesc.beltSpeed = 10;
            EditRecipe.Item(配方.低速分拣器, new int[] { 物品.铁块, 物品.电动机 });
            EditRecipe.ItemCount(配方.低速分拣器, new int[] { 1, 1 });
            EditRecipe.Item(配方.高速分拣器, new int[] { 物品.低速分拣器, 物品.电磁涡轮 });
            EditRecipe.ItemCount(配方.高速分拣器, new int[] { 2, 1 });
            EditRecipe.Item(配方.极速分拣器, new int[] { 物品.高速分拣器, 物品.超级磁场环 });
            EditRecipe.ItemCount(配方.极速分拣器, new int[] { 2, 1 });
            LDB.items.Select(物品.低速分拣器).prefabDesc.inserterSTT = 100000;
            LDB.items.Select(物品.高速分拣器).prefabDesc.inserterSTT = 50000;
            LDB.items.Select(物品.极速分拣器).prefabDesc.inserterSTT = 25000;
            EditRecipe.Item(配方.制造台1, new int[] { 基础机械组件.ID, 物品.低速分拣器, 物品.电路板 });
            EditRecipe.ItemCount(配方.制造台1, new int[] { 1, 1, 1 });
            EditRecipe.NeedTime(配方.制造台1, 1);
            EditRecipe.Item(配方.制造台2, new int[] { 基础机械组件.ID, 物品.玻璃, 物品.电路板 });
            EditRecipe.ItemCount(配方.制造台2, new int[] { 1, 1, 1 });
            EditRecipe.NeedTime(配方.制造台2, 2);
            EditRecipe.Item(配方.制造台3, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.极速分拣器, 物品.处理器 });
            EditRecipe.ItemCount(配方.制造台3, new int[] { 6, 10, 12, 6 });
            EditRecipe.NeedTime(配方.制造台3, 6);
            EditRecipe.Item(配方.四向分流器, new int[] { 物品.铁块, 物品.低速传送带 });
            EditRecipe.ItemCount(配方.四向分流器, new int[] { 1, 1 });
            EditRecipe.Item(配方.自动集装机, new int[] { 基础机械组件.ID, 物品.低速传送带, 物品.电路板 });
            EditRecipe.ItemCount(配方.自动集装机, new int[] { 2, 2, 2 });
            EditRecipe.Item(配方.流速监测器, new int[] { 物品.铁块, 物品.电路板 });
            EditRecipe.ItemCount(配方.流速监测器, new int[] { 1, 1 });
            EditRecipe.Item(配方.小型储物仓, new int[] { 物品.铁块, 物品.石材 });
            EditRecipe.ItemCount(配方.小型储物仓, new int[] { 2, 2 });
            EditRecipe.Item(配方.大型储物仓, new int[] { 物品.铁块, 物品.石材 });
            EditRecipe.ItemCount(配方.大型储物仓, new int[] { 4, 4 });
            EditRecipe.Item(配方.储液罐, new int[] { 物品.铁块, 物品.石材, 物品.玻璃 });
            EditRecipe.ItemCount(配方.储液罐, new int[] { 2, 2, 2 });
            EditRecipe.Item(配方.物流运输站, new int[] { 基础机械组件.ID, 物品.钢铁, 物品.处理器 });
            EditRecipe.ItemCount(配方.物流运输站, new int[] { 20, 60, 10 });
            EditRecipe.NeedTime(配方.物流运输站, 15);
            EditRecipe.Item(配方.星际物流运输站, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.量子芯片 });
            EditRecipe.ItemCount(配方.星际物流运输站, new int[] { 20, 40, 5 });
            EditRecipe.Item(配方.轨道采集器, new int[] { 物品.星际物流运输站, 物品.超级磁场环 });
            EditRecipe.ItemCount(配方.轨道采集器, new int[] { 1, 20 });
            EditRecipe.Item(配方.采矿机, new int[] { 基础机械组件.ID, 物品.铁块, 物品.齿轮 });
            EditRecipe.ItemCount(配方.采矿机, new int[] { 1, 2, 2 });
            EditRecipe.NeedTime(配方.采矿机, 2);
            LDB.items.Select(物品.采矿机).prefabDesc.minerPeriod = 600000;
            EditRecipe.Item(配方.电弧熔炉, new int[] { 基础机械组件.ID, 物品.铁块, 铜线圈.ID });
            EditRecipe.ItemCount(配方.电弧熔炉, new int[] { 1, 2, 2 });
            EditRecipe.NeedTime(配方.电弧熔炉, 2);
            EditRecipe.Item(配方.大型采矿机, new int[] { 尖端机械组件.ID, 物品.钛合金, 钨强化玻璃.ID, 先进生物电路.ID });
            EditRecipe.ItemCount(配方.大型采矿机, new int[] { 20, 60, 20, 10 });
            EditRecipe.NeedTime(配方.大型采矿机, 10);
            LDB.items.Select(物品.大型采矿机).prefabDesc.minerPeriod = 300000;
            EditRecipe.Item(配方.位面熔炉, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.位面过滤器, 物品.量子芯片 });
            EditRecipe.ItemCount(配方.位面熔炉, new int[] { 4, 12, 4, 4 });
            EditRecipe.NeedTime(配方.位面熔炉, 4);
            LDB.items.Select(物品.位面熔炉).prefabDesc.assemblerSpeed = 40000;
            EditRecipe.Item(配方.抽水站, new int[] { 基础机械组件.ID, 物品.铁块, 物品.电动机 });
            EditRecipe.ItemCount(配方.抽水站, new int[] { 2, 4, 2 });
            EditRecipe.NeedTime(配方.抽水站, 4);
            EditRecipe.Item(配方.化工厂, new int[] { 基础机械组件.ID, 物品.铁块, 物品.玻璃, 物品.电路板 });
            EditRecipe.ItemCount(配方.化工厂, new int[] { 2, 4, 4, 2 });
            EditRecipe.NeedTime(配方.化工厂, 4);
            LDB.items.Select(物品.化工厂).prefabDesc.assemblerSpeed = 20000;
            EditRecipe.Item(配方.原油萃取站, new int[] { 基础机械组件.ID, 物品.钢铁, 物品.电浆激发器, 物品.电路板 });
            EditRecipe.ItemCount(配方.原油萃取站, new int[] { 4, 12, 4, 4 });
            EditRecipe.NeedTime(配方.原油萃取站, 4);
            LDB.items.Select(物品.原油采集站).prefabDesc.minerPeriod = 300000;
            EditRecipe.Item(配方.原油精炼厂, new int[] { 基础机械组件.ID, 物品.钢铁, 物品.电浆激发器, 物品.电路板 });
            EditRecipe.ItemCount(配方.原油精炼厂, new int[] { 4, 6, 4, 2 });
            EditRecipe.NeedTime(配方.原油精炼厂, 2);
            LDB.items.Select(物品.原油精炼厂).prefabDesc.assemblerSpeed = 20000;
            EditRecipe.Item(配方.矩阵研究站, new int[] { 基础机械组件.ID, 物品.铁块, 物品.玻璃, 物品.电路板 });
            EditRecipe.ItemCount(配方.矩阵研究站, new int[] { 1, 4, 2, 2 });
            EditRecipe.NeedTime(配方.矩阵研究站, 2);
            LDB.items.Select(物品.研究站).prefabDesc.labResearchSpeed = 2;
            LDB.items.Select(物品.研究站).prefabDesc.labAssembleSpeed = 2;
            EditRecipe.Item(配方.电磁轨道弹射器, new int[] { 尖端机械组件.ID, 钨复合金属.ID, 物品.超级磁场环, 超级纳米电脑.ID });
            EditRecipe.ItemCount(配方.电磁轨道弹射器, new int[] { 20, 40, 20, 2 });
            EditRecipe.NeedTime(配方.电磁轨道弹射器, 6);
            LDB.items.Select(物品.电磁轨道弹射器).prefabDesc.ejectorChargeFrame = 20;
            LDB.items.Select(物品.电磁轨道弹射器).prefabDesc.ejectorColdFrame = 10;
            EditRecipe.Item(配方.垂直发射井, new int[] { 尖端机械组件.ID, 钨复合金属.ID, 钨强化玻璃.ID, 湿件主机.ID });
            EditRecipe.ItemCount(配方.垂直发射井, new int[] { 20, 40, 20, 2 });
            EditRecipe.NeedTime(配方.垂直发射井, 15);
            LDB.items.Select(物品.垂直发射井).prefabDesc.siloChargeFrame = 24;
            LDB.items.Select(物品.垂直发射井).prefabDesc.siloColdFrame = 6;
            EditRecipe.NeedTime(配方.位面过滤器, 6);
            EditRecipe.Item(配方.卡西米尔晶体, new int[] { 物品.金刚石, 物品.电浆激发器 });
            EditRecipe.ItemCount(配方.卡西米尔晶体, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.卡西米尔晶体, 2);
            EditRecipe.Item(配方.湮灭约束球, new int[] { 物品.钛合金, 物品.超级磁场环, 超生物计算集群.ID });
            EditRecipe.ItemCount(配方.湮灭约束球, new int[] { 10, 10, 4 });
            EditRecipe.NeedTime(配方.湮灭约束球, 10);
            EditRecipe.Item(配方.微型粒子对撞机, new int[] { 先进机械组件.ID, 物品.框架材料, 物品.超级磁场环, 超级纳米电脑.ID });
            EditRecipe.ItemCount(配方.微型粒子对撞机, new int[] { 40, 120, 60, 5 });
            EditRecipe.NeedTime(配方.微型粒子对撞机, 60);
            LDB.items.Select(物品.粒子对撞机).prefabDesc.assemblerSpeed = 40000;
            EditRecipe.Item(配方.奇异物质, new int[] { 物品.晶格硅, 物品.卡西米尔晶体 });
            EditRecipe.ItemCount(配方.奇异物质, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.奇异物质, 4);
            EditRecipe.ItemCount(配方.引力透镜, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.引力透镜, 4);
            EditRecipe.ItemCount(配方.等离子精炼, new int[] { 2 });
            EditRecipe.Result(配方.等离子精炼, new int[] { 物品.氢, 煤焦油.ID });
            EditRecipe.ResultCount(配方.等离子精炼, new int[] { 2, 1 });
            EditRecipe.NeedTime(配方.等离子精炼, 2);
            EditRecipe.Item(配方.X射线裂解, new int[] { 煤焦油.ID, 物品.氢 });
            EditRecipe.NeedTime(配方.X射线裂解, 2);
            EditRecipe.Item(配方.能量枢纽, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.处理器 });
            EditRecipe.ItemCount(配方.能量枢纽, new int[] { 4, 18, 4 });
            EditRecipe.NeedTime(配方.能量枢纽, 6);
            EditRecipe.Item(配方.卫星配电站, new int[] { 尖端机械组件.ID, 物品.框架材料, 物品.湮灭约束球, 湿件主机.ID });
            EditRecipe.ItemCount(配方.卫星配电站, new int[] { 60, 120, 60, 5 });
            EditRecipe.NeedTime(配方.卫星配电站, 120);
            LDB.items.Select(物品.卫星配电站).prefabDesc.powerCoverRadius = 2600.5f;
            LDB.items.Select(物品.卫星配电站).prefabDesc.powerConnectDistance = 5300.5f;
            EditRecipe.ItemCount(配方.重氢, new int[] { 4 });
            EditRecipe.ResultCount(配方.重氢, new int[] { 2 });
            EditRecipe.NeedTime(配方.重氢, 2);
            LDB.recipes.Select(配方.重氢).Type = global::ERecipeType.Chemical;
            LDB.recipes.Select(配方.奇异物质).Type = global::ERecipeType.Assemble;


            //原版配方逻辑


            //算了,摸了
            //原版配方删除
            EditRecipe.GridIndex(配方.喷涂机, 2801);
            EditRecipe.GridIndex(配方.分馏塔, 2802);
            EditRecipe.GridIndex(配方.有机晶体, 2803);
            EditRecipe.GridIndex(配方.原始有机晶体, 2804);
            EditRecipe.GridIndex(配方.钛晶石, 2805);
            EditRecipe.GridIndex(配方.粒子容器, 2806);
            EditRecipe.GridIndex(配方.高效粒子容器, 2807);
            EditRecipe.GridIndex(配方.高效卡西米尔晶体, 2808);
            EditRecipe.GridIndex(配方.光子合并器, 2809);
            EditRecipe.GridIndex(配方.高效光子合并器, 2810);
            EditRecipe.GridIndex(配方.重氢分馏, 2811);
            EditItem.GridIndex(物品.有机晶体, 1309);
            EditItem.GridIndex(物品.单极磁石, 1512);
            EditRecipe.GridIndex(配方.棱镜, 2812);
            EditRecipe.GridIndex(配方.增产剂1, 2813);
            EditRecipe.GridIndex(配方.增产剂2, 2814);
            EditRecipe.GridIndex(配方.增产剂3, 2815);
            EditRecipe.GridIndex(配方.磁线圈, 2816);
            EditRecipe.GridIndex(配方.磁铁, 2817);

            //新配方逻辑梳理

            //配方位置重定向
            EditRecipe.GridIndex(配方.矩阵研究站, 2309);
            EditRecipe.GridIndex(配方.化工厂, 2406);
            EditRecipe.GridIndex(配方.铁块, 1101);
            EditRecipe.GridIndex(配方.铜块, 1102);
            EditRecipe.GridIndex(配方.高纯硅块, 1103);
            EditRecipe.GridIndex(配方.钛块, 1104);
            AddRecipe.RecipeGridIndex(钨块配方, 1105);
            EditRecipe.GridIndex(配方.高能石墨, 1106);
            EditRecipe.GridIndex(配方.石材, 1107);
            EditRecipe.GridIndex(配方.玻璃, 1108);
            EditRecipe.GridIndex(配方.塑料, 1109);
            AddRecipe.RecipeGridIndex(环氧树脂配方, 1110);
            AddRecipe.RecipeGridIndex(聚苯并咪唑配方, 1111);
            AddRecipe.RecipeGridIndex(活性生物纤维配方, 1112);
            EditRecipe.GridIndex(配方.钢材, 1201);
            EditRecipe.GridIndex(配方.钛合金, 1202);
            AddRecipe.RecipeGridIndex(钨复合金属配方, 1203);
            EditRecipe.GridIndex(配方.金刚石, 1204);
            EditRecipe.GridIndex(配方.石墨烯, 1205);
            EditRecipe.GridIndex(配方.碳纳米管, 1206);
            EditRecipe.GridIndex(配方.晶格硅, 1207);
            EditRecipe.GridIndex(配方.空间翘曲器, 1208);
            EditRecipe.GridIndex(配方.推进器, 1209);
            EditRecipe.GridIndex(配方.物流运输机, 1210);
            EditRecipe.GridIndex(配方.重氢, 1211);
            EditRecipe.GridIndex(配方.奇异物质, 1212);
            EditRecipe.GridIndex(配方.齿轮, 1301);
            AddRecipe.RecipeGridIndex(钛齿轮配方, 1302);
            AddRecipe.RecipeGridIndex(钨齿轮配方, 1303);
            EditRecipe.GridIndex(配方.高效金刚石, 1304);
            EditRecipe.GridIndex(配方.高效石墨烯, 1305);
            EditRecipe.GridIndex(配方.高效碳纳米管, 1306);
            EditRecipe.GridIndex(配方.高效晶格硅, 1307);
            EditRecipe.GridIndex(配方.高效空间翘曲器, 1308);
            EditRecipe.GridIndex(配方.加力推进器, 1309);
            EditRecipe.GridIndex(配方.星际物流运输船, 1310);
            AddRecipe.RecipeGridIndex(高效重氢, 1311);
            AddRecipe.RecipeGridIndex(高效奇异物质, 1312);
            AddRecipe.RecipeGridIndex(基础机械组件配方, 1401);
            AddRecipe.RecipeGridIndex(先进机械组件配方, 1402);
            AddRecipe.RecipeGridIndex(尖端机械组件配方, 1403);
            EditRecipe.GridIndex(配方.粒子宽带, 1404);
            EditRecipe.GridIndex(配方.钛化玻璃, 1405);
            AddRecipe.RecipeGridIndex(生物微晶元件配方, 1406);
            AddRecipe.RecipeGridIndex(配方类.微晶元件, 1407);
            EditRecipe.GridIndex(配方.氢燃料棒, 1408);
            EditRecipe.GridIndex(配方.太阳帆, 1409);
            EditRecipe.GridIndex(配方.引力透镜, 1410);
            EditRecipe.GridIndex(配方.X射线裂解, 1411);
            EditRecipe.GridIndex(配方.光子物质化, 1412);
            AddRecipe.RecipeGridIndex(硅晶圆配方, 1501);
            AddRecipe.RecipeGridIndex(钛掺杂的硅晶圆配方, 1502);
            AddRecipe.RecipeGridIndex(钨掺杂的硅晶圆配方, 1503);
            AddRecipe.RecipeGridIndex(生物神经纤维配方, 1504);
            AddRecipe.RecipeGridIndex(钨强化玻璃配方, 1505);
            AddRecipe.RecipeGridIndex(先进生物电路配方, 1506);
            AddRecipe.RecipeGridIndex(配方类.处理器, 1507);
            EditRecipe.GridIndex(配方.氘核燃料棒, 1508);
            EditRecipe.GridIndex(配方.框架材料, 1509);
            EditRecipe.GridIndex(配方.卡西米尔晶体, 1510);
            EditRecipe.GridIndex(配方.等离子精炼, 1511);
            AddRecipe.RecipeGridIndex(高效反物质, 1512);
            EditRecipe.GridIndex(配方.电动机, 1601);
            EditRecipe.GridIndex(配方.电磁涡轮, 1602);
            EditRecipe.GridIndex(配方.超级磁场环, 1603);
            AddRecipe.RecipeGridIndex(超级纳米电脑配方, 1604);
            AddRecipe.RecipeGridIndex(湿件主机配方, 1605);
            AddRecipe.RecipeGridIndex(超生物计算集群配方, 1606);
            AddRecipe.RecipeGridIndex(配方类.量子芯片, 1607);
            EditRecipe.GridIndex(配方.反物质燃料棒, 1608);
            EditRecipe.GridIndex(配方.戴森球组件, 1609);
            EditRecipe.GridIndex(配方.位面过滤器, 1610);
            EditRecipe.GridIndex(配方.硅石, 1611);
            AddRecipe.RecipeGridIndex(极效石墨烯, 1612);
            EditRecipe.GridIndex(配方.地基, 1707);
            AddRecipe.RecipeGridIndex(空燃料棒配方, 1708);
            EditRecipe.GridIndex(配方.小型运载火箭, 1709);
            EditRecipe.GridIndex(配方.湮灭约束球, 1710);
            AddRecipe.RecipeGridIndex(钨矿配方, 1711);
            AddRecipe.RecipeGridIndex(精炼氢, 1712);
            AddRecipe.RecipeGridIndex(焦油分馏, 2501);
            AddRecipe.RecipeGridIndex(石油裂化, 2502);
            AddRecipe.RecipeGridIndex(氧气配方, 2503);
            AddRecipe.RecipeGridIndex(氢氧化钠配方, 2504);
            AddRecipe.RecipeGridIndex(氯化钠配方, 2505);
            AddRecipe.RecipeGridIndex(铜线圈配方, 2506);
            AddRecipe.RecipeGridIndex(石墨线圈配方, 2507);
            AddRecipe.RecipeGridIndex(钛线圈配方, 2508);
            AddRecipe.RecipeGridIndex(塑料基板配方, 2601);
            AddRecipe.RecipeGridIndex(覆层塑料基板配方, 2602);
            AddRecipe.RecipeGridIndex(钛蚀刻覆层塑料基板配方, 2603);
            AddRecipe.RecipeGridIndex(生物基板配方, 2604);
            AddRecipe.RecipeGridIndex(覆层生物基板配方, 2605);
            AddRecipe.RecipeGridIndex(钨蚀刻覆层生物基板配方, 2606);
            AddRecipe.RecipeGridIndex(超低功率IC晶片配方, 2607);
            AddRecipe.RecipeGridIndex(低功率IC晶片配方, 2608);
            AddRecipe.RecipeGridIndex(标准功率IC晶片配方, 2609);
            AddRecipe.RecipeGridIndex(高功率IC晶片配方, 2610);
            AddRecipe.RecipeGridIndex(超高功率IC晶片配方, 2611);
            AddRecipe.RecipeGridIndex(生物IC晶片配方, 2612);
            AddRecipe.RecipeGridIndex(三氯化铁配方, 2701);
            AddRecipe.RecipeGridIndex(氨配方, 2702);
            AddRecipe.RecipeGridIndex(双酚A配方, 2703);
            AddRecipe.RecipeGridIndex(硝酸配方, 2704);
            AddRecipe.RecipeGridIndex(二氨基联苯胺配方, 2705);
            AddRecipe.RecipeGridIndex(盐水配方, 2706);
            AddRecipe.RecipeGridIndex(二氯联苯胺配方, 2707);
            AddRecipe.RecipeGridIndex(二硝基氯苯配方, 2708);
            AddRecipe.RecipeGridIndex(煤焦油配方, 2709);
            AddRecipe.RecipeGridIndex(环氧氯丙烷配方, 2710);
            AddRecipe.RecipeGridIndex(间苯二甲酸二苯酯配方, 2711);
            AddRecipe.RecipeGridIndex(邻苯二甲酸配方, 2712);
            AddRecipe.RecipeGridIndex(氢燃料配方, 2509);
            AddRecipe.RecipeGridIndex(氘核燃料配方, 2510);
            AddRecipe.RecipeGridIndex(反物质燃料配方, 2511);
            AddRecipe.RecipeGridIndex(配方类.电路板, 2407);
            AddRecipe.RecipeGridIndex(配方类.硫酸, 2408);
            LDB.items.Select(物品.卫星配电站).prefabDesc.idleEnergyPerTick = 1200000;
            LDB.items.Select(物品.卫星配电站).prefabDesc.workEnergyPerTick = 40000000;
            LDB.items.Select(物品.电弧熔炉).prefabDesc.assemblerSpeed = 20000;

            //科技测试
            //基础组件
            AddTech.ID(基础组件, 1801);
            AddTech.NameAndDesc(基础组件, "基础机械组件",
                                "开发日志#1\n嗯……现在是2022年，一月三十日，大年二十九的凌晨三点半。\n我不知道我是怀着怎样的心态写下这个企划。\n我只希望我能做到心目中的最好。\n凌晨四点。\n我给自己定下的目标是，二月底之前，一个月。\n我希望自己能做到。\n晚安。");
            AddTech.Hash(基础组件, 1800);
            AddTech.UnlockRecipe(基础组件, new int[] { 基础机械组件配方.ID });
            AddTech.UnlockRecipeArray(基础组件, new RecipeProto[] { });
            AddTech.PreTech(基础组件, new int[] { 科技.电磁学 });
            AddTech.PreTechArray(基础组件, new TechProto[] { 科技类.电磁学 });
            AddTech.PostTechArray(基础组件, new TechProto[] { 先进组件 });
            AddTech.Vector(基础组件, new Vector2(13f, 25f));
            AddRecipe.RecipeIcon(基础组件, "基础组件");
            LDBTool.PreAddProto(ProtoType.Tech, 基础组件);
            基础机械组件配方.preTech = 基础组件;
            基础机械组件.preTech = 基础机械组件配方.preTech;
            //先进组件
            AddTech.ID(先进组件, 1802);
            AddTech.NameAndDesc(先进组件, "先进机械组件",
                                "开发日志#2\n2022.01.30，晚上八点零三分。\n我感觉自己受到了双重打击……api作者做出了我梦寐以求的东西，他的源码我却看不懂。\n我还能坚持下去吗？这个企划会胎死腹中吗？\n……………………\n好吧，也许会。我很不愿意这么说……但恐怕这次，我只能做出一个半成品了。\n希望你不会责备我。");
            AddTech.Hash(先进组件, 36000);
            AddTech.ItemArray(先进组件, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵 });
            AddTech.ItemAndPoint(先进组件, new int[] { 6001, 6002, 6003 }, new int[] { 40, 40, 20 });
            AddTech.UnlockRecipe(先进组件, new int[] { 先进机械组件配方.ID });
            AddTech.UnlockRecipeArray(先进组件, new RecipeProto[] { });
            AddTech.PreTech(先进组件, new int[] { 基础组件.ID });
            AddTech.PreTechArray(先进组件, new TechProto[] { 基础组件 });
            AddTech.PostTechArray(先进组件, new TechProto[] { 尖端组件 });
            AddTech.Vector(先进组件, new Vector2(21f, 25f));
            AddRecipe.RecipeIcon(先进组件, "先进组件");
            LDBTool.PreAddProto(ProtoType.Tech, 先进组件);
            先进机械组件配方.preTech = 先进组件;
            先进机械组件.preTech = 先进机械组件配方.preTech;
            //尖端组件
            AddTech.ID(尖端组件, 1803);
            AddTech.NameAndDesc(尖端组件, "尖端机械组件",
                                "开发日志#3\n咕了好久终于正式启动了，图标，科技，新物品……\n现在是2022年，2月1日，大年初一。\n我又有了一个新的想法，不过要回去看看另一套源码才能决定能不能实现……我想改变火箭的结构点数。\n我能成功吗？\n血的教训。\n不要，TMD，犯，低级错误。还有，一定要debug。\n（死目）\n哦对，结构点数没改成。\n啊对，记得回去加一个配方类型的修改方法。");
            AddTech.Hash(尖端组件, 72000);
            AddTech.ItemArray(尖端组件, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(尖端组件, new int[] { 6001, 6002, 6004 }, new int[] { 40, 40, 20 });
            AddTech.UnlockRecipe(尖端组件, new int[] { 尖端机械组件配方.ID });
            AddTech.UnlockRecipeArray(尖端组件, new RecipeProto[] { });
            AddTech.PreTech(尖端组件, new int[] { 先进组件.ID });
            AddTech.PreTechArray(尖端组件, new TechProto[] { 先进组件 });
            AddTech.PostTechArray(尖端组件, new TechProto[] { });
            AddTech.Vector(尖端组件, new Vector2(29f, 25f));
            AddRecipe.RecipeIcon(尖端组件, "尖端组件");
            LDBTool.PreAddProto(ProtoType.Tech, 尖端组件);
            尖端机械组件配方.preTech = 尖端组件;
            尖端机械组件.preTech = 尖端机械组件配方.preTech;
            //钨钢
            AddTech.ID(钨钢, 1804);
            AddTech.NameAndDesc(钨钢, "钨强化金属",
                                "开发日志#4\n偏偏这个时候出现了一个更多页面的mod\n那自然是……拿来主义）\n成为我的前置吧！（笑）\n既然这样，那这个游戏就有更多的可能性了……钛晶圆，钨晶圆，功率IC，电路基板，化工……\n2022.02.03，现在是大年初三晚上十点五十分。化工线基本完成，接下来就是烦人的代码环节……\n顺便把科技树图标做完了。\n今晚应该又要熬夜吧。");
            AddTech.Hash(钨钢, 144000);
            AddTech.ItemArray(钨钢, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(钨钢, new int[] { 6001, 6002, 6004 }, new int[] { 20, 20, 20 });
            AddTech.UnlockRecipe(钨钢, new int[] { 钨复合金属配方.ID });
            AddTech.UnlockRecipeArray(钨钢, new RecipeProto[] { });
            AddTech.PreTech(钨钢, new int[] { });
            AddTech.PreTechArray(钨钢, new TechProto[] { });
            AddTech.PostTechArray(钨钢, new TechProto[] { });
            AddTech.Vector(钨钢, new Vector2(37f, 17f));
            AddRecipe.RecipeIcon(钨钢, "钨钢");
            LDBTool.PreAddProto(ProtoType.Tech, 钨钢);
            钨复合金属配方.preTech = 钨钢;
            钨复合金属.preTech = 钨钢;
            //电路基板
            AddTech.ID(电路基板, 1805);
            AddTech.NameAndDesc(电路基板, "电路基板",
                                "开发日志#5\n……坏事了，unity好像不支持中文名诶？！\n我需要，做个小小的实验……\n顺便，现在是2022.02.04，大年初四,凌晨零点二十分。\n……凌晨一点十三分，我好像又踩了一个坑。\n我希望这是个坑，因为那样我就不用继续debug了。");
            AddTech.Hash(电路基板, 18000);
            AddTech.ItemArray(电路基板, new ItemProto[] { 物品类.电磁矩阵 });
            AddTech.ItemAndPoint(电路基板, new int[] { 6001 }, new int[] { 10 });
            AddTech.UnlockRecipe(电路基板,
                                 new int[]
                                 {
                                     塑料基板配方.ID, 覆层塑料基板配方.ID, 钛蚀刻覆层塑料基板配方.ID, 生物基板配方.ID, 覆层生物基板配方.ID, 钨蚀刻覆层生物基板配方.ID
                                 });
            AddTech.UnlockRecipeArray(电路基板, new RecipeProto[] { });
            AddTech.PreTech(电路基板, new int[] { });
            AddTech.PreTechArray(电路基板, new TechProto[] { });
            AddTech.PostTechArray(电路基板, new TechProto[] { });
            AddTech.Vector(电路基板, new Vector2(13f, -3f));
            AddRecipe.RecipeIcon(电路基板, "电路基板");
            LDBTool.PreAddProto(ProtoType.Tech, 电路基板);
            塑料基板.preTech = 电路基板;
            覆层塑料基板.preTech = 电路基板;
            钛蚀刻覆层塑料基板.preTech = 电路基板;
            生物基板.preTech = 电路基板;
            覆层生物基板.preTech = 电路基板;
            钨蚀刻覆层生物基板.preTech = 电路基板;
            塑料基板配方.preTech = 电路基板;
            覆层塑料基板配方.preTech = 电路基板;
            钛蚀刻覆层塑料基板配方.preTech = 电路基板;
            生物基板配方.preTech = 电路基板;
            覆层生物基板配方.preTech = 电路基板;
            钨蚀刻覆层生物基板配方.preTech = 电路基板;
            //电路板
            AddTech.ID(电路板, 1806);
            AddTech.NameAndDesc(电路板, "IC晶片",
                                "开发日志#6\n现在是，2022.02.04，星期五，凌晨一点四十六分。\n我感觉我所学不多且不成体系的编程知识在此刻受到了严重冲击。\n写下上一条日志的时候，我的工程文件不管怎么编译，修改，都在报错。\n我debug了将近一个小时，始终无果。\n就在刚才，我第不知道多少次重新编译源码——\n嘿，您猜怎么着？\n我的代码跑起来辣！\n草。");
            AddTech.Hash(电路板, 36000);
            AddTech.ItemArray(电路板, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵 });
            AddTech.ItemAndPoint(电路板, new int[] { 6001, 6002 }, new int[] { 20, 10 });
            AddTech.UnlockRecipe(电路板,new int[]
                                 {
                                     硅晶圆配方.ID, 钛掺杂的硅晶圆配方.ID, 钨掺杂的硅晶圆配方.ID, 超低功率IC晶片配方.ID, 低功率IC晶片配方.ID, 标准功率IC晶片配方.ID,
                                     高功率IC晶片配方.ID, 超高功率IC晶片配方.ID, 生物IC晶片配方.ID
                                 });
            AddTech.UnlockRecipeArray(电路板, new RecipeProto[] { });
            AddTech.PreTech(电路板, new int[] { });
            AddTech.PreTechArray(电路板, new TechProto[] { });
            AddTech.PostTechArray(电路板, new TechProto[] { });
            AddTech.Vector(电路板, new Vector2(21f, -3f));
            AddRecipe.RecipeIcon(电路板, "电路板");
            LDBTool.PreAddProto(ProtoType.Tech, 电路板);
            硅晶圆.preTech = 电路板;
            钛掺杂的硅晶圆.preTech = 电路板;
            钨掺杂的硅晶圆.preTech = 电路板;
            超低功率IC晶片.preTech = 电路板;
            低功率IC晶片.preTech = 电路板;
            标准功率IC晶片.preTech = 电路板;
            高功率IC晶片.preTech = 电路板;
            超高功率IC晶片.preTech = 电路板;
            生物IC晶片.preTech = 电路板;
            硅晶圆配方.preTech = 电路板;
            钛掺杂的硅晶圆配方.preTech = 电路板;
            钨掺杂的硅晶圆配方.preTech = 电路板;
            超低功率IC晶片配方.preTech = 电路板;
            低功率IC晶片配方.preTech = 电路板;
            标准功率IC晶片配方.preTech = 电路板;
            高功率IC晶片配方.preTech = 电路板;
            超高功率IC晶片配方.preTech = 电路板;
            生物IC晶片配方.preTech = 电路板;
            //虚拟技术革新
            AddTech.ID(虚拟技术革新, 1807);
            AddTech.NameAndDesc(虚拟技术革新, "虚拟技术革新",
                                "开发日志#7\n现在是，2022.02.04，下午两点零八分。\n化工线的第一个配方终于成功写入。\n这是第一步，但绝不是最后一步。\n……又踩了个坑。\n戴森球的配方逻辑是，配方会继承第一个产物的信息，包括名字介绍这些。\n当然也不会显示副产物。\n想要修改必须使用公式配方作为元数据。\n草。");
            AddTech.Hash(虚拟技术革新, 144000);
            AddTech.ItemArray(虚拟技术革新, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(虚拟技术革新, new int[] { 6001, 6002, 6003, 6004 }, new int[] { 20, 20, 20, 20 });
            AddTech.UnlockRecipe(虚拟技术革新, new int[] { 生物微晶元件配方.ID, 先进生物电路配方.ID, 超生物计算集群配方.ID });
            AddTech.UnlockRecipeArray(虚拟技术革新, new RecipeProto[] { });
            AddTech.PreTech(虚拟技术革新, new int[] { });
            AddTech.PreTechArray(虚拟技术革新, new TechProto[] { });
            AddTech.PostTechArray(虚拟技术革新, new TechProto[] { });
            AddTech.Vector(虚拟技术革新, new Vector2(37f, -15f));
            AddRecipe.RecipeIcon(虚拟技术革新, "虚拟技术革新");
            LDBTool.PreAddProto(ProtoType.Tech, 虚拟技术革新);
            生物微晶元件.preTech = 虚拟技术革新;
            先进生物电路.preTech = 虚拟技术革新;
            超生物计算集群.preTech = 虚拟技术革新;
            生物微晶元件配方.preTech = 虚拟技术革新;
            先进生物电路配方.preTech = 虚拟技术革新;
            超生物计算集群配方.preTech = 虚拟技术革新;
            //生物神经
            AddTech.ID(生物神经, 1808);
            AddTech.NameAndDesc(生物神经, "生物神经技术",
                                "开发日志#8\n现在是，2022.02.04，晚上七点零四分。\n化工线做到一半发现一点问题。\n稍微思考了一下，我决定还是自己打开gtnh……然后把化工线重新捋一遍。\n因为群友提醒了我，虽然木头是不可再生的……\n——但是有个东西，叫煤焦油。");
            AddTech.Hash(生物神经, 144000);
            AddTech.ItemArray(生物神经, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(生物神经, new int[] { 6001, 6002, 6003, 6004 }, new int[] { 20, 20, 20, 20 });
            AddTech.UnlockRecipe(生物神经, new int[] { 生物神经纤维配方.ID });
            AddTech.UnlockRecipeArray(生物神经, new RecipeProto[] { });
            AddTech.PreTech(生物神经, new int[] { });
            AddTech.PreTechArray(生物神经, new TechProto[] { });
            AddTech.PostTechArray(生物神经, new TechProto[] { });
            AddTech.Vector(生物神经, new Vector2(37f, -19f));
            AddRecipe.RecipeIcon(生物神经, "生物神经");
            LDBTool.PreAddProto(ProtoType.Tech, 生物神经);
            生物神经纤维.preTech = 生物神经;
            生物神经纤维配方.preTech = 生物神经;
            //主机科技
            AddTech.ID(主机科技, 1809);
            AddTech.NameAndDesc(主机科技, "超级纳米计算机",
                                "开发日志#9\n现在是，2022.02.05，上午十点三十九分。\n做完彻底完成了化工线，只剩两个最终产物了，顺便把电路基板和IC晶片的配方做了一下。\n距离物品系统实装又近了一步。\n现在是，2022.02.05，下午五点五十六分。\n又踩了个坑。\n戴森球的配方上限是825……怪。");
            AddTech.Hash(主机科技, 144000);
            AddTech.ItemArray(主机科技, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(主机科技, new int[] { 6001, 6002, 6003, 6004 }, new int[] { 20, 20, 20, 20 });
            AddTech.UnlockRecipe(主机科技, new int[] { 超级纳米电脑配方.ID });
            AddTech.UnlockRecipeArray(主机科技, new RecipeProto[] { });
            AddTech.PreTech(主机科技, new int[] { });
            AddTech.PreTechArray(主机科技, new TechProto[] { });
            AddTech.PostTechArray(主机科技, new TechProto[] { });
            AddTech.Vector(主机科技, new Vector2(45f, -15f));
            AddRecipe.RecipeIcon(主机科技, "主机科技");
            LDBTool.PreAddProto(ProtoType.Tech, 主机科技);
            超级纳米电脑.preTech = 主机科技;
            超级纳米电脑配方.preTech = 主机科技;
            //主机科技2
            AddTech.ID(主机科技2, 1810);
            AddTech.NameAndDesc(主机科技2, "生物活性计算机",
                                "开发日志#10\n现在是，2022.02.06，凌晨零点五十七分。\n现实里遇到一些胃疼的事，开发进度可能会延缓一点。\n顺便给化工线加了点新东西。\n物品已经彻底完成，接下来就是覆写配方和科技树。\n这将是一个漫长的过程。\n现在是，2022.02.06，凌晨两点零三分。\n有一小部分配方出现了莫名其妙的数组越界现象，但是我找不到问题在哪。\n似乎并没有影响到游戏。");
            AddTech.Hash(主机科技2, 288000);
            AddTech.ItemArray(主机科技2, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(主机科技2, new int[] { 6001, 6002, 6003, 6004 }, new int[] { 20, 20, 20, 20 });
            AddTech.UnlockRecipe(主机科技2, new int[] { 湿件主机配方.ID });
            AddTech.UnlockRecipeArray(主机科技2, new RecipeProto[] { });
            AddTech.PreTech(主机科技2, new int[] { });
            AddTech.PreTechArray(主机科技2, new TechProto[] { });
            AddTech.PostTechArray(主机科技2, new TechProto[] { });
            AddTech.Vector(主机科技2, new Vector2(45f, -19f));
            AddRecipe.RecipeIcon(主机科技2, "主机科技");
            LDBTool.PreAddProto(ProtoType.Tech, 主机科技2);
            湿件主机.preTech = 主机科技2;
            湿件主机配方.preTech = 主机科技2;
            //玻璃科技
            AddTech.ID(玻璃科技, 1811);
            AddTech.NameAndDesc(玻璃科技, "钨强化玻璃",
                                "开发日志#11\n……凌晨两点二十三分。\n它莫名其妙被我修好了。\n程序，很奇妙罢）\n凌晨两点二十九分。\n我找到制造逻辑了，芜湖~\n是makes。\n现在是，2022.02.06，下午两点二十⑨分。\n我突然有个想法。\n我不能添加新矿物，但是原版有几个没啥大用的旧矿物……\n嘿嘿嘿。");
            AddTech.Hash(玻璃科技, 144000);
            AddTech.ItemArray(玻璃科技, new ItemProto[] { 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            AddTech.ItemAndPoint(玻璃科技, new int[] { 6002, 6003, 6004 }, new int[] { 20, 20, 20 });
            AddTech.UnlockRecipe(玻璃科技, new int[] { 钨强化玻璃配方.ID });
            AddTech.UnlockRecipeArray(玻璃科技, new RecipeProto[] { });
            AddTech.PreTech(玻璃科技, new int[] { });
            AddTech.PreTechArray(玻璃科技, new TechProto[] { });
            AddTech.PostTechArray(玻璃科技, new TechProto[] { });
            AddTech.Vector(玻璃科技, new Vector2(17f, -27f));
            AddRecipe.RecipeIcon(玻璃科技, "玻璃科技");
            LDBTool.PreAddProto(ProtoType.Tech, 玻璃科技);
            钨强化玻璃.preTech = 玻璃科技;
            钨强化玻璃配方.preTech = 玻璃科技;
            //精炼科技
            AddTech.ID(精炼科技, 1812);
            AddTech.NameAndDesc(精炼科技, "先进精炼技术",
                                "开发日志#12\n……凌晨两点二十三分。\n它莫名其妙被我修好了。\n程序，很奇妙罢）\n凌晨两点二十九分。\n我找到制造逻辑了，芜湖~\n是makes。\n现在是，2022.02.06，下午两点二十⑨分。\n我突然有个想法。\n我不能添加新矿物，但是原版有几个没啥大用的旧矿物……\n嘿嘿嘿。");
            AddTech.Hash(精炼科技, 36000);
            AddTech.ItemArray(精炼科技, new ItemProto[] { 物品类.电磁矩阵 });
            AddTech.ItemAndPoint(精炼科技, new int[] { 6001 }, new int[] { 10 });
            AddTech.UnlockRecipe(精炼科技, new int[] { 氧气配方.ID, 焦油分馏.ID, 石油裂化.ID });
            AddTech.UnlockRecipeArray(精炼科技, new RecipeProto[] { });
            AddTech.PreTech(精炼科技, new int[] { });
            AddTech.PreTechArray(精炼科技, new TechProto[] { });
            AddTech.PostTechArray(精炼科技, new TechProto[] { });
            AddTech.Vector(精炼科技, new Vector2(21f, -31f));
            AddRecipe.RecipeIcon(精炼科技, "精炼科技");
            LDBTool.PreAddProto(ProtoType.Tech, 精炼科技);
            焦油分馏.preTech = 精炼科技;
            石油裂化.preTech = 精炼科技;
            氧气.preTech = 精炼科技;
            氧气配方.preTech = 精炼科技;

            //垃圾站
            AddTech.ID(化工科技, 1813);
            AddTech.NameAndDesc(化工科技, "海洋排污", "这听起来好像不太环保....");
            AddTech.Hash(化工科技, 288000);
            AddTech.ItemArray(化工科技, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵, 物品类.引力矩阵 });
            AddTech.ItemAndPoint(化工科技, new int[] { 6001, 6002, 6003, 6004, 6005 }, new int[] { 20, 20, 20, 20, 20 });
            AddTech.UnlockRecipe(化工科技, new int[] { });
            AddTech.UnlockRecipeArray(化工科技, new RecipeProto[] { });
            AddTech.PreTech(化工科技, new int[] { 科技.X射线裂解 });
            AddTech.PreTechArray(化工科技, new TechProto[] { 科技类.X射线裂解 });
            AddTech.PostTechArray(化工科技, new TechProto[] { 化工升级 });
            AddTech.Vector(化工科技, new Vector2(29f, -31f));
            AddRecipe.RecipeIcon(化工科技, "化工科技");
            LDBTool.PreAddProto(ProtoType.Tech, 化工科技);

            //化工厂加速
            AddTech.ID(化工升级, 1814);
            AddTech.NameAndDesc(化工升级, "化工技术革新", "\"说真的,我一直很好奇为什么这样就能提升机器效率....\"");
            AddTech.Hash(化工升级, 576000);
            AddTech.ItemArray(化工升级, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵, 物品类.引力矩阵 });
            AddTech.ItemAndPoint(化工升级, new int[] { 6001, 6002, 6003, 6004, 6005 }, new int[] { 20, 20, 20, 20, 20 });
            AddTech.UnlockRecipe(化工升级, new int[] { });
            AddTech.UnlockRecipeArray(化工升级, new RecipeProto[] { });
            AddTech.PreTech(化工升级, new int[] { 化工科技.ID });
            AddTech.PreTechArray(化工升级, new TechProto[] { 化工科技 });
            AddTech.PostTechArray(化工升级, new TechProto[] { });
            AddTech.Vector(化工升级, new Vector2(33f, -31f));
            AddRecipe.RecipeIcon(化工升级, "化工科技");
            LDBTool.PreAddProto(ProtoType.Tech, 化工升级);


            //原版科技重塑
            AddTech.Vector(科技类.基础制造工艺制造台, new Vector2(13f, -7f));
            AddTech.Vector(科技类.高级制造工艺制台, new Vector2(21f, -7f));
            AddTech.Vector(科技类.量子打印技术制造台, new Vector2(29f, -7f));
            AddTech.Vector(科技类.火力发电, new Vector2(13f, -11f));
            AddTech.Vector(科技类.地热开采, new Vector2(21f, -11f));
            AddTech.Vector(科技类.微型核聚变发电, new Vector2(29f, -11f));
            AddTech.Vector(科技类.半导体材料, new Vector2(13f, -15f));
            AddTech.Vector(科技类.处理器, new Vector2(21f, -15f));
            AddTech.Vector(科技类.量子芯片, new Vector2(29f, -15f));
            AddTech.Vector(科技类.粒子可控技术, new Vector2(29f, -19f));
            AddTech.Vector(科技类.氢燃料棒, new Vector2(13f, -19f));
            AddTech.Vector(科技类.推进器, new Vector2(17f, -19f));
            AddTech.Vector(科技类.加力推进器, new Vector2(21f, -19f));
            AddTech.Vector(科技类.流体储存封装, new Vector2(13f, -23f));
            AddTech.Vector(科技类.基础化工, new Vector2(17f, -23f));
            AddTech.Vector(科技类.高分子化工, new Vector2(21f, -23f));
            AddTech.Vector(科技类.卡西米尔晶体, new Vector2(25f, -23f));
            AddTech.Vector(科技类.波函数干扰, new Vector2(29f, -23f));
            AddTech.Vector(科技类.位面冶金, new Vector2(33f, -23f));
            AddTech.Vector(科技类.高强度玻璃, new Vector2(13f, -27f));
            AddTech.Vector(科技类.应用型超导体, new Vector2(25f, -27f));
            AddTech.Vector(科技类.高强度材料, new Vector2(29f, -27f));
            AddTech.Vector(科技类.奇异物质, new Vector2(33f, -27f));
            AddTech.Vector(科技类.引力波折射, new Vector2(37f, -27f));
            AddTech.Vector(科技类.高效电浆控制, new Vector2(13f, -31f));
            AddTech.Vector(科技类.等离子萃取精炼, new Vector2(17f, -31f));
            AddTech.Vector(科技类.X射线裂解, new Vector2(25f, -31f));
            AddTech.Vector(科技类.电磁驱动, new Vector2(13f, -39f));
            AddTech.Vector(科技类.磁悬浮技术, new Vector2(21f, -39f));
            AddTech.Vector(科技类.超级磁场发生器, new Vector2(29f, -39f));
            AddTech.Vector(科技类.卫星配电系统, new Vector2(37f, -39f));
            AddTech.Vector(科技类.电磁矩阵, new Vector2(13f, -43f));
            AddTech.Vector(科技类.能量矩阵, new Vector2(17f, -43f));
            AddTech.Vector(科技类.结构矩阵, new Vector2(21f, -43f));
            AddTech.Vector(科技类.信息矩阵, new Vector2(25f, -43f));
            AddTech.Vector(科技类.引力矩阵, new Vector2(29f, -43f));
            AddTech.Vector(科技类.宇宙矩阵, new Vector2(33f, -43f));
            AddTech.Vector(科技类.自动化台金, new Vector2(13f, 9f));
            AddTech.Vector(科技类.冶炼提纯, new Vector2(21f, 9f));
            AddTech.Vector(科技类.晶体冶炼, new Vector2(29f, 9f));
            AddTech.Vector(科技类.钛矿冶炼, new Vector2(37f, 9f));
            AddTech.Vector(科技类.钢材冶炼, new Vector2(21f, 17f));
            AddTech.Vector(科技类.高强度钛合金, new Vector2(29f, 17f));
            AddTech.Vector(科技类.高强度轻质结构, new Vector2(45f, 17f));
            AddTech.Vector(科技类.移山填海工程, new Vector2(29f, 13f));
            AddTech.Vector(科技类.气态行星开采, new Vector2(37f, 13f));
            AddTech.Vector(科技类.大型采矿机, new Vector2(45f, 13f));
            AddTech.Vector(科技类.能量储存, new Vector2(21f, 5f));
            AddTech.Vector(科技类.星际电力运输, new Vector2(29f, 5f));
            AddTech.Vector(科技类.集装物流系统, new Vector2(13f, 21f));
            AddTech.Vector(科技类.基础物流系统, new Vector2(21f, 21f));
            AddTech.Vector(科技类.改良物流系统, new Vector2(29f, 21f));
            AddTech.Vector(科技类.高效物流系统, new Vector2(37f, 21f));
            AddTech.Vector(科技类.行星物流系统, new Vector2(45f, 21f));
            AddTech.Vector(科技类.星际物流系统, new Vector2(53f, 21f));
            AddTech.Vector(科技类.太阳能收集, new Vector2(21f, 1f));
            AddTech.Vector(科技类.太阳帆轨道系统, new Vector2(33f, 1f));
            AddTech.Vector(科技类.垂直发射井, new Vector2(45f, 1f));
            AddTech.Vector(科技类.任务完成, new Vector2(57f, 1f));
            AddTech.Vector(科技类.戴森球应力系统, new Vector2(45f, 5f));
            AddTech.Vector(科技类.射线接收站, new Vector2(45f, -3f));
            AddTech.Vector(科技类.行星电离层利用, new Vector2(53f, -3f));
            AddTech.Vector(科技类.狄拉克逆变机制, new Vector2(45f, -7f));
            AddTech.Vector(科技类.可控湮灭反应, new Vector2(49f, -7f));
            AddTech.Vector(科技类.人造恒星, new Vector2(53f, -7f));
            AddTech.Vector(科技类.微型粒子对撞机, new Vector2(45f, -11f));

            //已删除科技
            EditTech.CanLab(科技.光子变频, false);
            EditTech.CanLab(科技.重氢分馏, false);
            EditTech.CanLab(科技.粒子磁力阱, false);
            EditTech.CanLab(科技.加速剂2, false);
            EditTech.CanLab(科技.加速剂3, false);
            EditTech.CanLab(科技.高强度晶体, false);
            EditTech.Vector(科技.光子变频, new Vector2(61f, 1f));
            EditTech.Vector(科技.粒子磁力阱, new Vector2(61f, 5f));
            EditTech.Vector(科技.重氢分馏, new Vector2(61f, 9f));
            // EditTech.Vector(科技.加速剂1, new Vector2(61f, 9f));
            EditTech.Vector(科技.加速剂2, new Vector2(61f, -3f));
            EditTech.Vector(科技.加速剂3, new Vector2(61f, -7f));
            EditTech.Vector(科技.高强度晶体, new Vector2(61f, -11f));
            EditTech.Name(科技.光子变频, "前面的区域,以后再来探索吧!");
            EditTech.Name(科技.粒子磁力阱, "前面的区域,以后再来探索吧!");
            EditTech.Name(科技.加速剂2, "前面的区域,以后再来探索吧!");
            EditTech.Name(科技.加速剂3, "前面的区域,以后再来探索吧!");
            EditTech.Name(科技.高强度晶体, "前面的区域,以后再来探索吧!");
            EditTech.Name(科技.重氢分馏, "前面的区域,以后再来探索吧!");

            //原版科技编辑
            EditTech.ItemAndPoint(科技.集装物流系统, new int[] { 物品.电路板 }, new int[] { 20 });
            EditTech.ItemAndPoint(科技.电磁驱动, new int[] { 物品.电磁矩阵 }, new int[] { 4 });
            EditTech.ItemArray(科技.集装物流系统, new ItemProto[] { LDB.items.Select(物品.电路板) });
            科技类.集装物流系统.IsLabTech = false;
            EditTech.UnlockRecipe(科技.集装物流系统, new int[] { 配方.小型储物仓, 配方.大型储物仓 });
            EditTech.UnlockRecipeArray(科技.集装物流系统, new RecipeProto[] { 配方类.小型储物仓, 配方类.大型储物仓 });
            EditTech.Hash(科技.集装物流系统, 1800);
            科技类.集装物流系统.name = "物品仓储";
            科技类.集装物流系统.Name = "物品仓储";
            LDB.items.Select(物品.小型储物仓).preTech = 科技类.集装物流系统;
            LDB.items.Select(物品.大型储物仓).preTech = 科技类.集装物流系统;
            LDB.recipes.Select(配方.大型储物仓).preTech = 科技类.集装物流系统;
            LDB.recipes.Select(配方.小型储物仓).preTech = 科技类.集装物流系统;
            EditTech.ItemAndPoint(科技.高级制造工艺制台, new int[] { 6001 }, new int[] { 20 });
            EditTech.ItemArray(科技.高级制造工艺制台, new ItemProto[] { 物品类.电磁矩阵 });
            EditTech.Hash(科技.高级制造工艺制台, 18000);
            EditTech.ItemAndPoint(科技.量子打印技术制造台, new int[] { 6001, 6002, 6003 }, new int[] { 40, 40, 40 });
            EditTech.ItemArray(科技.量子打印技术制造台, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵 });
            EditTech.Hash(科技.量子打印技术制造台, 72000);

            高效硅晶圆.preTech = 科技类.高分子化工;
            高效钛晶圆.preTech = 科技类.高分子化工;
            高效钨晶圆.preTech = 科技类.高分子化工;
            剧毒液体配方.preTech = 科技类.基础化工;

            //放飞自我的前置设定
            铜线圈.preTech = 科技类.基础制造工艺制造台;
            石墨线圈.preTech = 科技类.基础制造工艺制造台;
            钛线圈.preTech = 科技类.基础制造工艺制造台;
            铜线圈配方.preTech = 科技类.基础制造工艺制造台;
            石墨线圈配方.preTech = 科技类.基础制造工艺制造台;
            钛线圈配方.preTech = 科技类.基础制造工艺制造台;
            钨矿.preTech = 科技类.钛矿冶炼;
            钨块.preTech = 科技类.钛矿冶炼;
            钨矿配方.preTech = 科技类.钛矿冶炼;
            钨块配方.preTech = 科技类.钛矿冶炼;
            钛齿轮.preTech = 先进组件;
            钨齿轮.preTech = 尖端组件;
            钛齿轮配方.preTech = 先进组件;
            钨齿轮配方.preTech = 尖端组件;
            乙烯.preTech = 科技类.基础化工;
            氨.preTech = 科技类.基础化工;
            丙酮.preTech = 科技类.基础化工;
            苯酚.preTech = 科技类.基础化工;
            二氨基联苯胺.preTech = 科技类.基础化工;
            二甲苯.preTech = 科技类.基础化工;
            二氯联苯胺.preTech = 科技类.基础化工;
            二硝基氯苯.preTech = 科技类.基础化工;
            甘油.preTech = 科技类.基础化工;
            环氧氯丙烷.preTech = 科技类.基础化工;
            间苯二甲酸二苯酯.preTech = 科技类.基础化工;
            邻苯二甲酸.preTech = 科技类.基础化工;
            氯苯.preTech = 科技类.基础化工;
            氢氯酸.preTech = 科技类.基础化工;
            三氯化铁.preTech = 科技类.基础化工;
            双酚A.preTech = 科技类.基础化工;
            硝酸.preTech = 科技类.基础化工;
            盐水.preTech = 科技类.基础化工;
            煤焦油.preTech = 科技类.基础化工;
            氯化钠.preTech = 科技类.基础化工;
            氢氧化钠.preTech = 科技类.基础化工;
            空燃料棒.preTech = 科技类.基础制造工艺制造台;
            氢燃料.preTech = 科技类.氢燃料棒;
            氘核燃料.preTech = 科技类.微型核聚变发电;
            反物质燃料.preTech = 科技类.狄拉克逆变机制;
            高效反物质.preTech = 科技类.狄拉克逆变机制;
            高效奇异物质.preTech = 科技类.奇异物质;
            极效石墨烯.preTech = 科技类.高强度材料;
            高效重氢.preTech = 科技类.基础化工;
            配方类.重氢.preTech = 科技类.基础化工;
            氨配方.preTech = 科技类.基础化工;
            二氨基联苯胺配方.preTech = 科技类.基础化工;
            二氯联苯胺配方.preTech = 科技类.基础化工;
            二硝基氯苯配方.preTech = 科技类.基础化工;
            环氧氯丙烷配方.preTech = 科技类.基础化工;
            间苯二甲酸二苯酯配方.preTech = 科技类.基础化工;
            邻苯二甲酸配方.preTech = 科技类.基础化工;
            三氯化铁配方.preTech = 科技类.基础化工;
            双酚A配方.preTech = 科技类.基础化工;
            硝酸配方.preTech = 科技类.基础化工;
            盐水配方.preTech = 科技类.基础化工;
            煤焦油配方.preTech = 科技类.基础化工;
            氯化钠配方.preTech = 科技类.基础化工;
            氢氧化钠配方.preTech = 科技类.基础化工;
            硝酸钠.preTech = 科技类.基础化工;
            硫酸钠.preTech = 科技类.基础化工;
            环氧树脂.preTech = 科技类.基础化工;
            聚苯并咪唑.preTech = 科技类.基础化工;
            环氧树脂配方.preTech = 科技类.基础化工;
            聚苯并咪唑配方.preTech = 科技类.基础化工;
            活性生物纤维.preTech = 生物神经;
            活性生物纤维配方.preTech = 生物神经;
            空燃料棒配方.preTech = 科技类.基础制造工艺制造台;
            氢燃料配方.preTech = 科技类.氢燃料棒;
            氘核燃料配方.preTech = 科技类.微型核聚变发电;
            反物质燃料配方.preTech = 科技类.狄拉克逆变机制;
            精炼氢.preTech = 科技类.基础化工;


            //叠加


            //原版补丁
            EditRecipe.Item(配方.结构矩阵, new int[] { 物品.金刚石, 物品.晶格硅 });
            EditRecipe.Item(配方.电磁矩阵, new int[] { 铜线圈.ID, 物品.电路板 });
            EditRecipe.ItemCount(配方.信息矩阵, new int[] { 1, 1 });
            EditRecipe.ItemCount(配方.能量矩阵, new int[] { 1, 1 });
            EditRecipe.NeedTime(配方.能量矩阵, 3);
            EditRecipe.NeedTime(配方.结构矩阵, 3);
            EditRecipe.NeedTime(配方.信息矩阵, 3);
            EditRecipe.NeedTime(配方.引力矩阵, 3);
            EditRecipe.NeedTime(配方.宇宙矩阵, 3);
            EditRecipe.Item(配方.信息矩阵, new int[] { 物品.卡西米尔晶体, 物品.粒子宽带 });
            EditRecipe.Item(配方.引力矩阵, new int[] { 物品.引力透镜, 物品.位面过滤器 });
            EditRecipe.ItemCount(配方.钛块, new int[] { 1 });
            EditRecipe.NeedTime(配方.钛块, 1);
            EditRecipe.NeedTime(配方.高效晶格硅, 1);

            EditRecipe.GridIndex(配方.电浆激发器, 2409);

            EditTech.ItemArray(科技.电磁学, new ItemProto[] { 铜线圈 });
            EditTech.ItemAndPoint(科技.电磁学, new int[] { 铜线圈.ID }, new int[] { 30 });
            LDB.techs.Select(2901).itemArray = new ItemProto[] { LDB.items.Select(物品.煤矿), 铜线圈 };
            LDB.techs.Select(2901).Items = new int[] { LDB.items.Select(物品.煤矿).ID, 铜线圈.ID };
            EditTech.ItemArray(4103, new ItemProto[] { 物品类.电磁矩阵 });
            EditTech.ItemAndPoint(4103, new int[] { 物品.电磁矩阵 }, new int[] { 12 });
            EditTech.ItemArray(4104, new ItemProto[] { 物品类.电磁矩阵 });
            EditTech.ItemAndPoint(4104, new int[] { 物品.电磁矩阵 }, new int[] { 6 });
            EditRecipe.ItemCount(配方.地基, new int[] { 1, 1 });
            AddRecipe.RecipeHandCraft(盐水配方, true);
            EditTech.ItemArray(科技.自动化台金, new ItemProto[] { 铜线圈, LDB.items.Select(物品.电路板) });
            EditTech.ItemAndPoint(科技.自动化台金, new int[] { 铜线圈.ID, 物品.电路板 }, new int[] { 20, 20 });
            EditTech.ItemArray(科技.电磁矩阵, new ItemProto[] { 铜线圈, LDB.items.Select(物品.电路板) });
            EditTech.ItemAndPoint(科技.电磁矩阵, new int[] { 铜线圈.ID, 物品.电路板 }, new int[] { 20, 20 });


            铜线圈.preTech = LDB.techs.Select(1);
            铜线圈配方.preTech = LDB.techs.Select(1);

            //物流线修改
            EditTech.UnlockRecipe(科技.基础物流系统, new int[] { 配方.低速传送带, 配方.低速分拣器, 配方.四向分流器 });
            EditTech.UnlockRecipeArray(科技.基础物流系统, new RecipeProto[] { 配方类.低速传送带, 配方类.低速分拣器, 配方类.四向分流器 });
            LDB.items.Select(物品.低速传送带).preTech = 科技类.基础物流系统;
            LDB.items.Select(物品.低速分拣器).preTech = 科技类.基础物流系统;
            LDB.items.Select(物品.四向分流器).preTech = 科技类.基础物流系统;
            LDB.recipes.Select(配方.低速传送带).preTech = 科技类.基础物流系统;
            LDB.recipes.Select(配方.低速分拣器).preTech = 科技类.基础物流系统;
            LDB.recipes.Select(配方.四向分流器).preTech = 科技类.基础物流系统;
            EditTech.UnlockRecipe(科技.改良物流系统, new int[] { 配方.高速传送带, 配方.高速分拣器, 配方.流速监测器 });
            EditTech.UnlockRecipeArray(科技.改良物流系统, new RecipeProto[] { 配方类.高速传送带, 配方类.高速分拣器, 配方类.流速监测器 });
            LDB.items.Select(物品.高速传送带).preTech = 科技类.改良物流系统;
            LDB.items.Select(物品.高速分拣器).preTech = 科技类.改良物流系统;
            LDB.recipes.Select(配方.高速传送带).preTech = 科技类.改良物流系统;
            LDB.recipes.Select(配方.高速分拣器).preTech = 科技类.改良物流系统;
            LDB.items.Select(物品.流速计).preTech = 科技类.改良物流系统;
            LDB.recipes.Select(配方.流速监测器).preTech = 科技类.改良物流系统;
            EditTech.UnlockRecipe(科技.高效物流系统, new int[] { 配方.极速传送带, 配方.极速分拣器 });
            EditTech.UnlockRecipeArray(科技.高效物流系统, new RecipeProto[] { 配方类.极速传送带, 配方类.极速分拣器 });
            LDB.items.Select(物品.极速传送带).preTech = 科技类.高效物流系统;
            LDB.items.Select(物品.极速分拣器).preTech = 科技类.高效物流系统;
            LDB.recipes.Select(配方.极速传送带).preTech = 科技类.高效物流系统;
            LDB.recipes.Select(配方.极速分拣器).preTech = 科技类.高效物流系统;
            EditTech.UnlockRecipe(科技.行星物流系统, new int[] { 配方.物流运输站, 配方.物流运输机 });
            EditTech.UnlockRecipeArray(科技.行星物流系统, new RecipeProto[] { 配方类.物流运输站, 配方类.物流运输机 });
            LDB.items.Select(物品.行星物流运输站).preTech = 科技类.行星物流系统;
            LDB.items.Select(物品.物流运输机).preTech = 科技类.行星物流系统;
            LDB.recipes.Select(配方.物流运输站).preTech = 科技类.行星物流系统;
            LDB.recipes.Select(配方.物流运输机).preTech = 科技类.行星物流系统;
            //科技逻辑
            EditTech.PreTech(科技.电磁矩阵, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.电磁矩阵, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.电磁矩阵, new TechProto[] { 科技类.能量矩阵 });
            EditTech.PreTech(科技.能量矩阵, new int[] { 科技.电磁矩阵 });
            EditTech.PreTechArray(科技.能量矩阵, new TechProto[] { 科技类.电磁矩阵 });
            EditTech.PostTechArray(科技.能量矩阵, new TechProto[] { 科技类.结构矩阵 });
            EditTech.PreTech(科技.结构矩阵, new int[] { 科技.能量矩阵 });
            EditTech.PreTechArray(科技.结构矩阵, new TechProto[] { 科技类.能量矩阵 });
            EditTech.PostTechArray(科技.结构矩阵, new TechProto[] { 科技类.信息矩阵 });
            EditTech.PreTech(科技.信息矩阵, new int[] { 科技.结构矩阵 });
            EditTech.PreTechArray(科技.信息矩阵, new TechProto[] { 科技类.结构矩阵 });
            EditTech.PostTechArray(科技.信息矩阵, new TechProto[] { 科技类.引力矩阵 });
            EditTech.PreTech(科技.引力矩阵, new int[] { 科技.信息矩阵 });
            EditTech.PreTechArray(科技.引力矩阵, new TechProto[] { 科技类.信息矩阵 });
            EditTech.PostTechArray(科技.引力矩阵, new TechProto[] { 科技类.宇宙矩阵 });
            EditTech.PreTech(科技.宇宙矩阵, new int[] { 科技.引力矩阵 });
            EditTech.PreTechArray(科技.宇宙矩阵, new TechProto[] { 科技类.引力矩阵 });
            EditTech.PostTechArray(科技.宇宙矩阵, new TechProto[] { });
            EditTech.PreTech(科技.电磁驱动, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.电磁驱动, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.电磁驱动, new TechProto[] { 科技类.磁悬浮技术 });
            EditTech.PreTech(科技.磁悬浮技术, new int[] { 科技.电磁驱动 });
            EditTech.PreTechArray(科技.磁悬浮技术, new TechProto[] { 科技类.电磁驱动 });
            EditTech.PostTechArray(科技.磁悬浮技术, new TechProto[] { 科技类.超级磁场发生器 });
            EditTech.PreTech(科技.超级磁场发生器, new int[] { 科技.磁悬浮技术 });
            EditTech.PreTechArray(科技.超级磁场发生器, new TechProto[] { 科技类.磁悬浮技术 });
            EditTech.PostTechArray(科技.超级磁场发生器, new TechProto[] { 科技类.卫星配电系统 });
            EditTech.PreTech(科技.卫星配电系统, new int[] { 科技.超级磁场发生器 });
            EditTech.PreTechArray(科技.卫星配电系统, new TechProto[] { 科技类.超级磁场发生器 });
            EditTech.PostTechArray(科技.卫星配电系统, new TechProto[] { });
            EditTech.PreTech(科技.高效电浆控制, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.高效电浆控制, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.高效电浆控制, new TechProto[] { 科技类.等离子萃取精炼 });
            EditTech.PreTech(科技.等离子萃取精炼, new int[] { 科技.高效电浆控制 });
            EditTech.PreTechArray(科技.等离子萃取精炼, new TechProto[] { 科技类.高效电浆控制 });
            EditTech.PostTechArray(科技.等离子萃取精炼, new TechProto[] { 精炼科技 });
            精炼科技.preTechArray = new TechProto[] { 科技类.等离子萃取精炼 };
            精炼科技.PreTechs = new int[] { 科技.等离子萃取精炼 };
            精炼科技.postTechArray = new TechProto[] { 科技类.X射线裂解 };
            EditTech.PreTech(科技.X射线裂解, new int[] { 精炼科技.ID });
            EditTech.PreTechArray(科技.X射线裂解, new TechProto[] { 精炼科技 });
            EditTech.PostTechArray(科技.X射线裂解, new TechProto[] { 化工科技 });
            EditTech.PreTech(科技.高强度玻璃, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.高强度玻璃, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.高强度玻璃, new TechProto[] { 玻璃科技 });
            玻璃科技.preTechArray = new TechProto[] { 科技类.高强度玻璃 };
            玻璃科技.PreTechs = new int[] { 科技.高强度玻璃 };
            玻璃科技.postTechArray = new TechProto[] { };
            EditTech.PreTech(科技.流体储存封装, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.流体储存封装, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.流体储存封装, new TechProto[] { 科技类.基础化工 });
            EditTech.PreTech(科技.基础化工, new int[] { 科技.流体储存封装 });
            EditTech.PreTechArray(科技.基础化工, new TechProto[] { 科技类.流体储存封装 });
            EditTech.PostTechArray(科技.基础化工, new TechProto[] { 科技类.高分子化工 });
            EditTech.PreTech(科技.高分子化工, new int[] { 科技.基础化工 });
            EditTech.PreTechArray(科技.高分子化工, new TechProto[] { 科技类.基础化工 });
            EditTech.PostTechArray(科技.高分子化工, new TechProto[] { 科技类.卡西米尔晶体, 科技类.应用型超导体 });
            EditTech.PreTech(科技.卡西米尔晶体, new int[] { 科技.高分子化工 });
            EditTech.PreTechArray(科技.卡西米尔晶体, new TechProto[] { 科技类.高分子化工 });
            EditTech.PostTechArray(科技.卡西米尔晶体, new TechProto[] { 科技类.波函数干扰 });
            EditTech.PreTech(科技.波函数干扰, new int[] { 科技.卡西米尔晶体 });
            EditTech.PreTechArray(科技.波函数干扰, new TechProto[] { 科技类.卡西米尔晶体 });
            EditTech.PostTechArray(科技.波函数干扰, new TechProto[] { 科技类.位面冶金 });
            EditTech.PreTech(科技.位面冶金, new int[] { 科技.波函数干扰 });
            EditTech.PreTechArray(科技.位面冶金, new TechProto[] { 科技类.波函数干扰 });
            EditTech.PostTechArray(科技.位面冶金, new TechProto[] { });
            EditTech.PreTech(科技.应用型超导体, new int[] { 科技.高分子化工 });
            EditTech.PreTechArray(科技.应用型超导体, new TechProto[] { 科技类.高分子化工 });
            EditTech.PostTechArray(科技.应用型超导体, new TechProto[] { 科技类.高强度材料 });
            EditTech.PreTech(科技.高强度材料, new int[] { 科技.应用型超导体 });
            EditTech.PreTechArray(科技.高强度材料, new TechProto[] { 科技类.应用型超导体 });
            EditTech.PostTechArray(科技.高强度材料, new TechProto[] { 科技类.奇异物质 });
            EditTech.PreTech(科技.奇异物质, new int[] { 科技.高强度材料 });
            EditTech.PreTechArray(科技.奇异物质, new TechProto[] { 科技类.高强度材料 });
            EditTech.PostTechArray(科技.奇异物质, new TechProto[] { 科技类.引力波折射 });
            EditTech.PreTech(科技.引力波折射, new int[] { 科技.奇异物质 });
            EditTech.PreTechArray(科技.引力波折射, new TechProto[] { 科技类.奇异物质 });
            EditTech.PostTechArray(科技.引力波折射, new TechProto[] { });
            EditTech.PreTech(科技.氢燃料棒, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.氢燃料棒, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.氢燃料棒, new TechProto[] { 科技类.推进器 });
            EditTech.PreTech(科技.推进器, new int[] { 科技.氢燃料棒 });
            EditTech.PreTechArray(科技.推进器, new TechProto[] { 科技类.氢燃料棒 });
            EditTech.PostTechArray(科技.推进器, new TechProto[] { 科技类.加力推进器 });
            EditTech.PreTech(科技.加力推进器, new int[] { 科技.推进器 });
            EditTech.PreTechArray(科技.加力推进器, new TechProto[] { 科技类.推进器 });
            EditTech.PostTechArray(科技.加力推进器, new TechProto[] { });
            EditTech.PreTech(科技.半导体材料, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.半导体材料, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.半导体材料, new TechProto[] { 科技类.处理器 });
            EditTech.PreTech(科技.处理器, new int[] { 科技.半导体材料 });
            EditTech.PreTechArray(科技.处理器, new TechProto[] { 科技类.半导体材料 });
            EditTech.PostTechArray(科技.处理器, new TechProto[] { 科技类.量子芯片, 科技类.粒子可控技术 });
            EditTech.PreTech(科技.量子芯片, new int[] { 科技.处理器 });
            EditTech.PreTechArray(科技.量子芯片, new TechProto[] { 科技类.处理器 });
            EditTech.PostTechArray(科技.量子芯片, new TechProto[] { 虚拟技术革新 });
            虚拟技术革新.preTechArray = new TechProto[] { 科技类.量子芯片 };
            虚拟技术革新.PreTechs = new int[] { 科技.量子芯片 };
            虚拟技术革新.postTechArray = new TechProto[] { 主机科技 };
            主机科技.preTechArray = new TechProto[] { 虚拟技术革新 };
            主机科技.PreTechs = new int[] { 虚拟技术革新.ID };
            主机科技.postTechArray = new TechProto[] { };
            EditTech.PreTech(科技.粒子可控技术, new int[] { 科技.处理器 });
            EditTech.PreTechArray(科技.粒子可控技术, new TechProto[] { 科技类.处理器 });
            EditTech.PostTechArray(科技.粒子可控技术, new TechProto[] { 生物神经 });
            生物神经.preTechArray = new TechProto[] { 科技类.粒子可控技术 };
            生物神经.PreTechs = new int[] { 科技.粒子可控技术 };
            生物神经.postTechArray = new TechProto[] { 主机科技2 };
            主机科技2.preTechArray = new TechProto[] { 生物神经 };
            主机科技2.PreTechs = new int[] { 生物神经.ID };
            主机科技2.postTechArray = new TechProto[] { };
            EditTech.PreTech(科技.火力发电, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.火力发电, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.火力发电, new TechProto[] { 科技类.微型核聚变发电 });
            EditTech.PreTech(科技.地热开采, new int[] { });
            EditTech.PreTechArray(科技.地热开采, new TechProto[] { });
            EditTech.PostTechArray(科技.地热开采, new TechProto[] { });
            科技类.地热开采.Position = new Vector2(33f, -11f);
            EditTech.PreTech(科技.微型核聚变发电, new int[] { 科技.火力发电 });
            EditTech.PreTechArray(科技.微型核聚变发电, new TechProto[] { 科技类.火力发电 });
            EditTech.PostTechArray(科技.微型核聚变发电, new TechProto[] { });
            EditTech.PreTech(科技.基础制造工艺制造台, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.基础制造工艺制造台, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.基础制造工艺制造台, new TechProto[] { 科技类.高级制造工艺制台 });
            EditTech.PreTech(科技.高级制造工艺制台, new int[] { 科技.基础制造工艺制造台 });
            EditTech.PreTechArray(科技.高级制造工艺制台, new TechProto[] { 科技类.基础制造工艺制造台 });
            EditTech.PostTechArray(科技.高级制造工艺制台, new TechProto[] { 科技类.量子打印技术制造台 });
            EditTech.PreTech(科技.量子打印技术制造台, new int[] { 科技.高级制造工艺制台 });
            EditTech.PreTechArray(科技.量子打印技术制造台, new TechProto[] { 科技类.高级制造工艺制台 });
            EditTech.PostTechArray(科技.量子打印技术制造台, new TechProto[] { });
            电路基板.preTechArray = new TechProto[] { 科技类.电磁学 };
            电路基板.PreTechs = new int[] { 科技.电磁学 };
            电路基板.postTechArray = new TechProto[] { 电路板 };
            电路板.preTechArray = new TechProto[] { 电路基板 };
            电路板.PreTechs = new int[] { 电路基板.ID };
            电路板.postTechArray = new TechProto[] { };
            EditTech.PreTech(科技.太阳能收集, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.太阳能收集, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.太阳能收集, new TechProto[] { 科技类.太阳帆轨道系统 });

            EditTech.PreTech(科技.太阳帆轨道系统, new int[] { 科技.太阳能收集 });
            EditTech.PreTechArray(科技.太阳帆轨道系统, new TechProto[] { 科技类.太阳能收集 });
            EditTech.PostTechArray(科技.太阳帆轨道系统,
                                   new TechProto[] { 科技类.戴森球应力系统, 科技类.垂直发射井, 科技类.射线接收站, 科技类.狄拉克逆变机制, 科技类.微型粒子对撞机 });
            EditTech.PreTech(科技.戴森球应力系统, new int[] { 科技.太阳帆轨道系统 });
            EditTech.PreTechArray(科技.戴森球应力系统, new TechProto[] { 科技类.太阳帆轨道系统 });
            EditTech.PostTechArray(科技.戴森球应力系统, new TechProto[] { });
            EditTech.PreTech(科技.垂直发射井, new int[] { 科技.太阳帆轨道系统 });
            EditTech.PreTechArray(科技.垂直发射井, new TechProto[] { 科技类.太阳帆轨道系统 });
            EditTech.PostTechArray(科技.垂直发射井, new TechProto[] { 科技类.任务完成 });
            EditTech.PreTech(科技.任务完成, new int[] { 科技.垂直发射井 });
            EditTech.PreTechArray(科技.任务完成, new TechProto[] { 科技类.垂直发射井 });
            EditTech.PostTechArray(科技.任务完成, new TechProto[] { });
            EditTech.PreTech(科技.射线接收站, new int[] { 科技.太阳帆轨道系统 });
            EditTech.PreTechArray(科技.射线接收站, new TechProto[] { 科技类.太阳帆轨道系统 });
            EditTech.PostTechArray(科技.射线接收站, new TechProto[] { 科技类.行星电离层利用 });
            EditTech.PreTech(科技.行星电离层利用, new int[] { 科技.射线接收站 });
            EditTech.PreTechArray(科技.行星电离层利用, new TechProto[] { 科技类.射线接收站 });
            EditTech.PostTechArray(科技.行星电离层利用, new TechProto[] { });
            EditTech.PreTech(科技.狄拉克逆变机制, new int[] { 科技.太阳帆轨道系统 });
            EditTech.PreTechArray(科技.狄拉克逆变机制, new TechProto[] { 科技类.太阳帆轨道系统 });
            EditTech.PostTechArray(科技.狄拉克逆变机制, new TechProto[] { 科技类.可控湮灭反应 });
            EditTech.PreTech(科技.可控湮灭反应, new int[] { 科技.狄拉克逆变机制 });
            EditTech.PreTechArray(科技.可控湮灭反应, new TechProto[] { 科技类.狄拉克逆变机制 });
            EditTech.PostTechArray(科技.可控湮灭反应, new TechProto[] { 科技类.人造恒星 });
            EditTech.PreTech(科技.人造恒星, new int[] { 科技.可控湮灭反应 });
            EditTech.PreTechArray(科技.人造恒星, new TechProto[] { 科技类.可控湮灭反应 });
            EditTech.PostTechArray(科技.人造恒星, new TechProto[] { });
            EditTech.PreTech(科技.微型粒子对撞机, new int[] { 科技.太阳帆轨道系统 });
            EditTech.PreTechArray(科技.微型粒子对撞机, new TechProto[] { 科技类.太阳帆轨道系统 });
            EditTech.PostTechArray(科技.微型粒子对撞机, new TechProto[] { });
            EditTech.PreTech(科技.自动化台金, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.自动化台金, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.自动化台金, new TechProto[] { 科技类.钢材冶炼, 科技类.冶炼提纯, 科技类.能量储存 });
            EditTech.PreTech(科技.钢材冶炼, new int[] { 科技.自动化台金 });
            EditTech.PreTechArray(科技.钢材冶炼, new TechProto[] { 科技类.自动化台金 });
            EditTech.PostTechArray(科技.钢材冶炼, new TechProto[] { 科技类.高强度钛合金, 科技类.移山填海工程 });
            EditTech.PreTech(科技.高强度钛合金, new int[] { 科技.钢材冶炼 });
            EditTech.PreTechArray(科技.高强度钛合金, new TechProto[] { 科技类.钢材冶炼 });
            EditTech.PostTechArray(科技.高强度钛合金, new TechProto[] { 钨钢 });
            钨钢.preTechArray = new TechProto[] { 科技类.高强度钛合金 };
            钨钢.PreTechs = new int[] { 科技.高强度钛合金 };
            钨钢.postTechArray = new TechProto[] { 科技类.高强度轻质结构 };
            EditTech.PreTech(科技.高强度轻质结构, new int[] { 钨钢.ID });
            EditTech.PreTechArray(科技.高强度轻质结构, new TechProto[] { 钨钢 });
            EditTech.PostTechArray(科技.高强度轻质结构, new TechProto[] { });
            EditTech.PreTech(科技.移山填海工程, new int[] { 科技.钢材冶炼 });
            EditTech.PreTechArray(科技.移山填海工程, new TechProto[] { 科技类.钢材冶炼 });
            EditTech.PostTechArray(科技.移山填海工程, new TechProto[] { 科技类.气态行星开采 });
            EditTech.PreTech(科技.气态行星开采, new int[] { 科技.移山填海工程 });
            EditTech.PreTechArray(科技.气态行星开采, new TechProto[] { 科技类.移山填海工程 });
            EditTech.PostTechArray(科技.气态行星开采, new TechProto[] { 科技类.大型采矿机 });
            EditTech.PreTech(科技.大型采矿机, new int[] { 科技.气态行星开采 });
            EditTech.PreTechArray(科技.大型采矿机, new TechProto[] { 科技类.气态行星开采 });
            EditTech.PostTechArray(科技.大型采矿机, new TechProto[] { });
            EditTech.PreTech(科技.冶炼提纯, new int[] { 科技.自动化台金 });
            EditTech.PreTechArray(科技.冶炼提纯, new TechProto[] { 科技类.自动化台金 });
            EditTech.PostTechArray(科技.冶炼提纯, new TechProto[] { 科技类.晶体冶炼 });
            EditTech.PreTech(科技.晶体冶炼, new int[] { 科技.冶炼提纯 });
            EditTech.PreTechArray(科技.晶体冶炼, new TechProto[] { 科技类.冶炼提纯 });
            EditTech.PostTechArray(科技.晶体冶炼, new TechProto[] { 科技类.钛矿冶炼 });
            EditTech.PreTech(科技.钛矿冶炼, new int[] { 科技.晶体冶炼 });
            EditTech.PreTechArray(科技.钛矿冶炼, new TechProto[] { 科技类.晶体冶炼 });
            EditTech.PostTechArray(科技.钛矿冶炼, new TechProto[] { });
            EditTech.PreTech(科技.能量储存, new int[] { 科技.自动化台金 });
            EditTech.PreTechArray(科技.能量储存, new TechProto[] { 科技类.自动化台金 });
            EditTech.PostTechArray(科技.能量储存, new TechProto[] { 科技类.星际电力运输 });
            EditTech.PreTech(科技.星际电力运输, new int[] { 科技.能量储存 });
            EditTech.PreTechArray(科技.星际电力运输, new TechProto[] { 科技类.能量储存 });
            EditTech.PostTechArray(科技.星际电力运输, new TechProto[] { });
            EditTech.PreTech(科技.集装物流系统, new int[] { 科技.电磁学 });
            EditTech.PreTechArray(科技.集装物流系统, new TechProto[] { 科技类.电磁学 });
            EditTech.PostTechArray(科技.集装物流系统, new TechProto[] { 科技类.基础物流系统 });
            EditTech.PreTech(科技.基础物流系统, new int[] { 科技.集装物流系统 });
            EditTech.PreTechArray(科技.基础物流系统, new TechProto[] { 科技类.集装物流系统 });
            EditTech.PostTechArray(科技.基础物流系统, new TechProto[] { 科技类.改良物流系统 });
            EditTech.PreTech(科技.改良物流系统, new int[] { 科技.基础物流系统 });
            EditTech.PreTechArray(科技.改良物流系统, new TechProto[] { 科技类.基础物流系统 });
            EditTech.PostTechArray(科技.改良物流系统, new TechProto[] { 科技类.高效物流系统, 科技类.加速剂1 });
            EditTech.PreTech(科技.加速剂1, new int[] { 科技.改良物流系统 });
            EditTech.PreTechArray(科技.加速剂1, new TechProto[] { 科技类.改良物流系统 });
            EditTech.PostTechArray(科技.加速剂1, new TechProto[] { });
            EditTech.PreTech(科技.高效物流系统, new int[] { 科技.改良物流系统 });
            EditTech.PreTechArray(科技.高效物流系统, new TechProto[] { 科技类.改良物流系统 });
            EditTech.PostTechArray(科技.高效物流系统, new TechProto[] { 科技类.行星物流系统 });
            EditTech.PreTech(科技.行星物流系统, new int[] { 科技.高效物流系统 });
            EditTech.PreTechArray(科技.行星物流系统, new TechProto[] { 科技类.高效物流系统 });
            EditTech.PostTechArray(科技.行星物流系统, new TechProto[] { 科技类.星际物流系统 });
            EditTech.PreTech(科技.星际物流系统, new int[] { 科技.行星物流系统 });
            EditTech.PreTechArray(科技.星际物流系统, new TechProto[] { 科技类.行星物流系统 });
            EditTech.PostTechArray(科技.星际物流系统, new TechProto[] { });
            EditTech.PreTech(科技.电磁学, new int[] { 1 });
            EditTech.PreTechArray(科技.电磁学, new TechProto[] { LDB.techs.Select(1) });
            EditTech.PostTechArray(科技.电磁学,
                                   new TechProto[]
                                   {
                                       基础组件, 科技类.集装物流系统, 科技类.自动化台金, 电路基板, 科技类.基础制造工艺制造台, 科技类.火力发电, 科技类.半导体材料, 科技类.氢燃料棒,
                                       科技类.流体储存封装, 科技类.高强度玻璃, 科技类.高效电浆控制, 科技类.电磁驱动, 科技类.电磁矩阵, 科技类.太阳能收集
                                   });

            EditTech.PreTech(1, new int[] { });
            EditTech.PreTechArray(1, new TechProto[] { });
            EditTech.PostTechArray(1, new TechProto[] { 科技类.电磁学 });
            EditTech.Name(科技.高级制造工艺制台, "电路蚀刻技术");
            EditTech.Name(科技.量子打印技术制造台, "高精度加工技术");

            LDB.items.Select(物品.水泵).prefabDesc.minerPeriod = 360000;

            LDB.recipes.Select(配方.量子芯片).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.粒子宽带).Type = (global::ERecipeType)ERecipeType.高精度加工;
            先进生物电路配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            生物微晶元件配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.小型运载火箭).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.太阳帆).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.电磁轨道弹射器).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.垂直发射井).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.位面熔炉).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.位面过滤器).Type = (global::ERecipeType)ERecipeType.高精度加工;
            LDB.recipes.Select(配方.大型采矿机).Type = (global::ERecipeType)ERecipeType.高精度加工;

            LDB.recipes.Select(配方.量子芯片).Handcraft = false;
            LDB.recipes.Select(配方.粒子宽带).Handcraft = false;
            先进生物电路配方.Handcraft = false;
            生物微晶元件配方.Handcraft = false;
            生物神经纤维配方.Handcraft = false;
            LDB.recipes.Select(配方.小型运载火箭).Handcraft = false;
            LDB.recipes.Select(配方.太阳帆).Handcraft = false;
            LDB.recipes.Select(配方.电磁轨道弹射器).Handcraft = false;
            LDB.recipes.Select(配方.垂直发射井).Handcraft = false;
            LDB.recipes.Select(配方.位面熔炉).Handcraft = false;
            LDB.recipes.Select(配方.位面过滤器).Handcraft = false;
            LDB.recipes.Select(配方.大型采矿机).Handcraft = false;

            EditTech.ItemAndPoint(科技.人造恒星, new int[] { 6001, 6002, 6003, 6004, 6005 },
                                  new int[] { 20, 20, 20, 20, 20 });
            EditTech.ItemArray(科技.人造恒星, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵, 物品类.引力矩阵 });
            EditTech.Hash(科技.人造恒星, 4320000);
            EditTech.Name(科技.人造恒星, "终极目标:人造恒星");
            EditTech.ItemAndPoint(科技.卫星配电系统, new int[] { 6001, 6002, 6003, 6004, 6005 },
                                  new int[] { 18, 18, 18, 18, 18 });
            EditTech.ItemArray(科技.卫星配电系统, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵, 物品类.引力矩阵 });
            EditTech.Hash(科技.卫星配电系统, 4800000);
            EditTech.Name(科技.卫星配电系统, "终极目标:广播式能源");
            EditItem.Name(物品.卫星配电站, "广播式能源塔");

            EditTech.Name(科技.太阳帆轨道系统, "戴森球计划:太阳帆轨道系统");
            EditTech.Name(科技.戴森球应力系统, "戴森球计划:戴森球应力系统");
            EditTech.Name(科技.垂直发射井, "戴森球计划:戴森球组件运输系统");
            EditTech.Name(科技.射线接收站, "戴森球计划:戴森球能源传输系统");
            EditTech.Name(科技.行星电离层利用, "戴森球计划:行星电离层利用");
            EditTech.Name(科技.狄拉克逆变机制, "戴森球计划:光子逆变技术");
            EditTech.Name(科技.微型粒子对撞机, "戴森球计划:粒子对撞机");

            EditTech.Hash(科技.任务完成, 36000000);

            创世之书.PreTechOverride = 1;
            虚空之书.PreTechOverride = 1;
            起源之书.PreTechOverride = 1;

            /*MilestoneProto 测试里程碑 = LDB.milestones.Select(15).Copy();
            测试里程碑.ID = 43;
            测试里程碑.Position = new Vector2(1023f, 363f);
             AddRecipe.RecipeIcon(测试里程碑,"高效奇异物质);
            LDBTool.PreAddProto(测试里程碑);*/

            科技类.电磁学.AddItems = new int[] { 2203, 2201, 2301, 创世之书.ID };
            科技类.电磁学.AddItemCounts = new int[] { 1, 1, 1, 1 };
            科技类.引力矩阵.AddItems = new int[] { 虚空之书.ID };
            科技类.引力矩阵.AddItemCounts = new int[] { 1 };
            科技类.任务完成.AddItems = new int[] { 起源之书.ID, 超级机械组件.ID };
            科技类.任务完成.AddItemCounts = new int[] { 1, 1 };

            科技类.地热开采.Published = false;
            科技类.地热开采.name = "前面的区域,以后再来探索吧!";
            科技类.地热开采.Name = "前面的区域,以后再来探索吧!";

            科技类.加速剂1.Position = new Vector2(37f, 25f);
            科技类.加速剂1.name = "物品增产";
            科技类.加速剂1.Name = "物品增产";
            EditTech.ItemAndPoint(科技.加速剂1, new int[] { 物品.电磁矩阵, 物品.能量矩阵, 物品.结构矩阵, 物品.信息矩阵 }, new int[] { 8, 8, 6, 2 });
            EditTech.ItemArray(科技.加速剂1, new ItemProto[] { 物品类.电磁矩阵, 物品类.能量矩阵, 物品类.结构矩阵, 物品类.信息矩阵 });
            EditTech.UnlockRecipe(科技.加速剂1, new int[] { 配方.喷涂机, 配方.增产剂3 });
            EditTech.UnlockRecipeArray(科技.加速剂1, new RecipeProto[] { 配方类.喷涂机, 配方类.增产剂3 });
            EditTech.Hash(科技.加速剂1, 720000);
            LDB.items.Select(物品.喷涂机).preTech = 科技类.加速剂1;
            LDB.items.Select(物品.加速剂3).preTech = 科技类.加速剂1;
            LDB.items.Select(配方.喷涂机).preTech = 科技类.加速剂1;
            LDB.items.Select(配方.增产剂3).preTech = 科技类.加速剂1;
            AddRecipe.RecipeGridIndex(配方类.增产剂3, 2205);
            AddRecipe.RecipeData(配方类.增产剂3, new int[] { 物品.碳纳米管, 三氯化铁.ID }, new int[] { 2, 1 }, new int[] { 物品.加速剂3 },
                                 new int[] { 1 });
            AddRecipe.RecipeNeedTime(配方类.增产剂3, 2);
            AddRecipe.RecipeGridIndex(配方类.喷涂机, 2108);
            AddRecipe.RecipeData(配方类.喷涂机, new int[] { 先进机械组件.ID, 物品.钛合金, 物品.钛化玻璃, 物品.量子芯片 }, new int[] { 10, 10, 6, 4 },
                                 new int[] { 物品.喷涂机 }, new int[] { 1 });
            AddRecipe.RecipeNeedTime(配方类.喷涂机, 4);

            AddRecipe.RecipeGridIndex(配方类.地热发电站, 2908);
            AddRecipe.RecipeGridIndex(配方类.自动集装机, 2901);

            超低功率IC晶片配方.Handcraft = true;
            超低功率IC晶片配方.Type = global::ERecipeType.Assemble;
            低功率IC晶片配方.Handcraft = true;
            低功率IC晶片配方.Type = global::ERecipeType.Assemble;

            LDB.items.Select(物品.人造恒星).DescFields = new int[] { 1, 4, 5, 40 };
            LDB.items.Select(物品.火力发电机).DescFields = new int[] { 1, 4, 5, 40 };


            创世之书.UnlockKey = -1;
            虚空之书.UnlockKey = -1;
            起源之书.UnlockKey = -1;
            配方类.卫星配电站.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.微型粒子对撞机.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.微型聚变发电站.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.射线接收站.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.人造恒星.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.喷涂机.Type = (global::ERecipeType)ERecipeType.高精度加工;
            配方类.卫星配电站.Handcraft = false;
            配方类.微型粒子对撞机.Handcraft = false;
            配方类.微型聚变发电站.Handcraft = false;
            配方类.射线接收站.Handcraft = false;
            配方类.人造恒星.Handcraft = false;
            配方类.喷涂机.Handcraft = false;
            尖端机械组件配方.Type = (global::ERecipeType)ERecipeType.高精度加工;
            尖端机械组件配方.Handcraft = false;
            LDB.items.Select(物品.无线输电塔).prefabDesc.powerConnectDistance = 90.5f;
            氧气.miningFrom = "气态巨星轨道";
            氧气.MiningFrom = "气态巨星轨道";
            LDB.items.Select(物品.重氢).DescFields = new int[] { 1, 2, 3, 31, 40 };
            创世之书.DescFields = new int[] { };
            虚空之书.DescFields = new int[] { };
            起源之书.DescFields = new int[] { };
            反物质燃料配方.Handcraft = false;
            反物质燃料配方.Type = global::ERecipeType.Chemical;

            LDB.items.Select(物品.一级制造台).Grade = 0;
            LDB.items.Select(物品.二级制造台).Grade = 0;
            LDB.items.Select(物品.三级制造台).Grade = 0;
            LDB.items.Select(物品.一级制造台).Upgrades = new int[] { };
            LDB.items.Select(物品.二级制造台).Upgrades = new int[] { };
            LDB.items.Select(物品.三级制造台).Upgrades = new int[] { };
            //LDB.items.Select(物品.一级制造台).canUpgrade = false;
        }

        //void ModifySpeed() { LDB.items.Select(8001).prefabDesc.assemblerSpeed = 80000; }
    }
}
