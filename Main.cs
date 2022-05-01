using xiaoye97;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;
using CommonAPI;
using CommonAPI.Systems;
using ProjectGenesis.Patches;
using ERecipeType_1 = ERecipeType;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace ProjectGenesis
{
    [BepInPlugin("org.LoShin.GenesisBook", "Genesis", "1.0.0")]
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool")]
    [BepInDependency("dsp.common-api.CommonAPI")]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem),
                                  nameof(AssemblerRecipeSystem))]
    public class Main : BaseUnityPlugin
    {
        private int TableID, TableID2;

        public void Awake()
        {
            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", pluginfolder);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            TableID = TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab",
                                            new TabData("3", "Assets/texpack/主机科技"));
            TableID2 = TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                             new TabData("4", "Assets/texpack/化工科技"));

            LDBTool.PreAddDataAction += InitData;
            LDBTool.PostAddDataAction += PostAddDataAction;

            Harmony.CreateAndPatchAll(typeof(UIPatches));
            Harmony.CreateAndPatchAll(typeof(MultiProductionPatches));
            Harmony.CreateAndPatchAll(typeof(PlanetGasPatches));
            Harmony.CreateAndPatchAll(typeof(OceanDischargePatches));
        }

        public void InitData()
        {
            PreFix();

            #region ModelProto

            var TankModel = CopyModelProto(121, 301, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            LDBTool.PreAddProto(TankModel);

            var OreFactoryModel = CopyModelProto(194, 302, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            LDBTool.PreAddProto(OreFactoryModel);

            var TestCraftingTableModel = CopyModelProto(49, 303, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            LDBTool.PreAddProto(TestCraftingTableModel);
            var TestCraftingTableModel2 = CopyModelProto(49, 304, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(TestCraftingTableModel2);
            var TestCraftingTableModel3 = CopyModelProto(49, 305, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            LDBTool.PreAddProto(TestCraftingTableModel3);
            var TestCraftingTableModel4 = CopyModelProto(49, 306, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            LDBTool.PreAddProto(TestCraftingTableModel4);
            var AntiMatterModel = CopyModelProto(118, 307, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            LDBTool.PreAddProto(AntiMatterModel);

            #endregion

            #region TechProto

            var templateTech = LDB.techs.Select(1311);

            foreach (var techjson in JsonHelper.TechProtos())
            {
                var proto = LDB.techs.Exist(techjson.ID)
                    ? LDB.techs.Select(techjson.ID)
                    : templateTech.Copy();

                proto.ID = techjson.ID;
                proto.Name = techjson.Name;
                proto.Desc = techjson.Desc;
                proto.Conclusion = techjson.Conclusion;
                proto.Published = techjson.Published;
                proto.IconPath = techjson.IconPath;
                proto.IsLabTech = techjson.IsLabTech;
                proto.PreTechs = techjson.PreTechs;
                proto.PreTechsImplicit = techjson.PreTechs;
                proto.Items = techjson.Items ?? Array.Empty<int>();
                proto.ItemPoints = techjson.ItemPoints ?? Array.Empty<int>();
                proto.HashNeeded = techjson.HashNeeded;
                proto.UnlockRecipes = techjson.UnlockRecipes;
                proto.AddItems = techjson.AddItems ?? Array.Empty<int>();
                proto.AddItemCounts = techjson.AddItemCounts ?? Array.Empty<int>();
                proto.Position = new Vector2(techjson.Position[0], techjson.Position[1]);

                if (!LDB.techs.Exist(techjson.ID))
                {
                    LDBTool.PreAddProto(proto);
                }
            }

            #endregion

            #region ItemProto

            foreach (var itemjson in JsonHelper.ItemProtos())
            {
                if (itemjson.GridIndex >= 4000 && itemjson.GridIndex < 5000)
                    itemjson.GridIndex = TableID2 * 1000 + (itemjson.GridIndex - 4000);

                else if (itemjson.GridIndex >= 3000) itemjson.GridIndex = TableID * 1000 + (itemjson.GridIndex - 3000);

                var proto = LDB.items.Exist(itemjson.ID)
                    ? LDB.items.Select(itemjson.ID)
                    : ProtoRegistry.RegisterItem(itemjson.ID, itemjson.Name, itemjson.Description, itemjson.IconPath,
                                                 itemjson.GridIndex, itemjson.StackSize, (EItemType)itemjson.Type);

                proto.ID = itemjson.ID;
                proto.Name = itemjson.Name;
                proto.Description = itemjson.Description;
                proto.IconPath = itemjson.IconPath;
                proto.GridIndex = itemjson.GridIndex;
                proto.StackSize = itemjson.StackSize;
                proto.FuelType = itemjson.FuelType;
                proto.HeatValue = itemjson.HeatValue;
                proto.ReactorInc = itemjson.ReactorInc;
                proto.DescFields = itemjson.DescFields ?? Array.Empty<int>();
                proto.IsFluid = itemjson.IsFluid;
                proto.Type = (EItemType)itemjson.Type;
                proto.SubID = itemjson.SubID;
                proto.MiningFrom = itemjson.MiningFrom;
                proto.ProduceFrom = itemjson.ProduceFrom;
                proto.Grade = itemjson.Grade;
                proto.Upgrades = itemjson.Upgrades ?? Array.Empty<int>();
                proto.IsEntity = itemjson.IsEntity;
                proto.CanBuild = itemjson.CanBuild;
                proto.BuildInGas = itemjson.BuildInGas;
                proto.ModelIndex = itemjson.ModelIndex;
                proto.ModelCount = itemjson.ModelCount;
                proto.HpMax = itemjson.HpMax;
                proto.Ability = itemjson.Ability;
                proto.Potential = itemjson.Potential;
                proto.BuildIndex = itemjson.BuildIndex;
                proto.BuildMode = itemjson.BuildMode;
                proto.UnlockKey = itemjson.UnlockKey;
                proto.PreTechOverride = itemjson.PreTechOverride;
                proto.Productive = itemjson.Productive;
            }

            #endregion

            #region RecipeProto

            foreach (var recipeJson in JsonHelper.RecipeProtos())
            {
                if (!LDB.recipes.Exist(recipeJson.ID))
                {
                    if (recipeJson.GridIndex >= 4000 && recipeJson.GridIndex < 5000)
                        recipeJson.GridIndex = TableID2 * 1000 + (recipeJson.GridIndex - 4000);
                    else if (recipeJson.GridIndex >= 3000)
                        recipeJson.GridIndex = TableID * 1000 + (recipeJson.GridIndex - 3000);

                    var proto = ProtoRegistry.RegisterRecipe(recipeJson.ID, (ERecipeType_1)recipeJson.Type,
                                                             recipeJson.Time, recipeJson.Input, recipeJson.InCounts,
                                                             recipeJson.Output ?? Array.Empty<int>(),
                                                             recipeJson.OutCounts ?? Array.Empty<int>(),
                                                             recipeJson.Description, recipeJson.PreTech,
                                                             recipeJson.GridIndex, recipeJson.Name,
                                                             recipeJson.IconPath);

                    proto.Explicit = recipeJson.Explicit;
                    proto.Name = recipeJson.Name;
                    proto.Handcraft = recipeJson.Handcraft;
                    proto.NonProductive = recipeJson.NonProductive;
                }
                else
                {
                    var proto = LDB.recipes.Select(recipeJson.ID);

                    proto.Explicit = recipeJson.Explicit;
                    proto.Name = recipeJson.Name;
                    proto.Handcraft = recipeJson.Handcraft;
                    proto.NonProductive = recipeJson.NonProductive;
                    proto.Type = (global::ERecipeType)recipeJson.Type;
                    proto.TimeSpend = recipeJson.Time;
                    proto.Items = recipeJson.Input;
                    proto.ItemCounts = recipeJson.InCounts;
                    proto.Results = recipeJson.Output ?? Array.Empty<int>();
                    proto.ResultCounts = recipeJson.OutCounts ?? Array.Empty<int>();
                    proto.Description = recipeJson.Description;
                    proto.preTech = LDB.techs.Select(recipeJson.PreTech);
                    proto.GridIndex = recipeJson.GridIndex;
                    proto.IconPath = recipeJson.IconPath;
                    proto.Explicit = recipeJson.Explicit;
                    proto.Name = recipeJson.Name;
                    proto.Handcraft = recipeJson.Handcraft;
                    proto.NonProductive = recipeJson.NonProductive;
                }
            }

            #endregion
        }


        public void PreFix()
        {
            LDB.strings.Select(2314).name = "剧毒液体海洋";
            LDB.strings.Select(2314).Name = "剧毒液体海洋";

            LDB.veins.Select(14).Name = "钨矿";
            LDB.veins.Select(14).name = "钨矿";

            LDB.milestones.Select(9).name = "钨";
            LDB.milestones.Select(9).Name = "钨";
            LDB.milestones.Select(9).defaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";
            LDB.milestones.Select(9).DefaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";

            LDB.items.Select(物品.聚变发电机).name = "裂变能源发电站";
            LDB.items.Select(物品.聚变发电机).Name = "裂变能源发电站";

            LDB.items.Select(物品.单极磁石).ID = 6980;
            LDB.items.Select(物品.硫酸).ID = 6998;

            LDB.items.OnAfterDeserialize();
        }

        public void PostFix(ItemProtoSet itemProtos)
        {
            LDB.items.OnAfterDeserialize();


            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.电路蚀刻;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.高精度加工;

            itemProtos.Select(物品.一级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.一级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.一级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.一级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.一级制造台).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.二级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.二级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.二级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.二级制造台).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.三级制造台).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.三级制造台).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.三级制造台).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.三级制造台).prefabDesc.workEnergyPerTick * 2;

            itemProtos.Select(物品.位面熔炉).prefabDesc.assemblerSpeed = 40000;
            itemProtos.Select(物品.化工厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.原油精炼厂).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.粒子对撞机).prefabDesc.assemblerSpeed = 40000;

            itemProtos.Select(物品.采矿机).prefabDesc.minerPeriod = 600000;
            itemProtos.Select(物品.大型采矿机).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.原油采集站).prefabDesc.minerPeriod = 300000;
            itemProtos.Select(物品.水泵).prefabDesc.minerPeriod = 360000;

            itemProtos.Select(物品.电磁轨道弹射器).prefabDesc.ejectorChargeFrame = 20;
            itemProtos.Select(物品.电磁轨道弹射器).prefabDesc.ejectorColdFrame = 10;

            itemProtos.Select(物品.垂直发射井).prefabDesc.siloChargeFrame = 24;
            itemProtos.Select(物品.垂直发射井).prefabDesc.siloColdFrame = 6;

            itemProtos.Select(物品.卫星配电站).prefabDesc.powerConnectDistance = 5300.5f;
            itemProtos.Select(物品.卫星配电站).prefabDesc.powerCoverRadius = 2600.5f;
            itemProtos.Select(物品.电力感应塔).prefabDesc.powerConnectDistance = 44.5f;
            itemProtos.Select(物品.电力感应塔).prefabDesc.powerCoverRadius = 20.5f;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.powerConnectDistance = 32.5f;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.powerCoverRadius = 14.9f;

            itemProtos.Select(物品.火力发电机).prefabDesc.genEnergyPerTick = 200000;
            itemProtos.Select(物品.太阳能板).prefabDesc.genEnergyPerTick = 30000;
            itemProtos.Select(物品.地热发电机).prefabDesc.genEnergyPerTick = 400000;
            itemProtos.Select(物品.聚变发电机).prefabDesc.genEnergyPerTick = 2500000;
            itemProtos.Select(物品.聚变发电机).prefabDesc.useFuelPerTick = 2500000;
            itemProtos.Select(物品.风力涡轮机).prefabDesc.genEnergyPerTick = 25000;
            itemProtos.Select(物品.人造恒星).prefabDesc.genEnergyPerTick = 10000000;
            itemProtos.Select(物品.人造恒星).prefabDesc.useFuelPerTick = 10000000;

            itemProtos.Select(物品.火力发电机).prefabDesc.useFuelPerTick = 150000;

            itemProtos.Select(物品.低速传送带).prefabDesc.beltSpeed = 3;
            itemProtos.Select(物品.高速传送带).prefabDesc.beltSpeed = 5;
            itemProtos.Select(物品.极速传送带).prefabDesc.beltSpeed = 10;

            itemProtos.Select(物品.低速分拣器).prefabDesc.inserterSTT = 100000;
            itemProtos.Select(物品.高速分拣器).prefabDesc.inserterSTT = 50000;
            itemProtos.Select(物品.极速分拣器).prefabDesc.inserterSTT = 25000;

            itemProtos.Select(物品.卫星配电站).prefabDesc.idleEnergyPerTick = 12000000;
            itemProtos.Select(物品.卫星配电站).prefabDesc.workEnergyPerTick = 48000000;

            itemProtos.Select(物品.电弧熔炉).prefabDesc.assemblerSpeed = 20000;
            itemProtos.Select(物品.电弧熔炉).prefabDesc.idleEnergyPerTick = itemProtos.Select(物品.电弧熔炉).prefabDesc.idleEnergyPerTick * 2;
            itemProtos.Select(物品.电弧熔炉).prefabDesc.workEnergyPerTick = itemProtos.Select(物品.电弧熔炉).prefabDesc.workEnergyPerTick * 2;
            itemProtos.Select(物品.无线输电塔).prefabDesc.powerConnectDistance = 90.5f;

            itemProtos.Select(6230).prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.矿物处理;
            itemProtos.Select(6230).prefabDesc.idleEnergyPerTick = 400;
            itemProtos.Select(6230).prefabDesc.workEnergyPerTick = 12000;

            itemProtos.Select(6229).prefabDesc.fluidStorageCount = 1000000;
            LDBTool.SetBuildBar(4, 4, 6229);
        }

        private void PostAddDataAction()
        {
            LDB.items.OnAfterDeserialize();
            LDB.recipes.OnAfterDeserialize();
            LDB.models.OnAfterDeserialize();
            GameMain.gpuiManager.Init();

            //行星装配站调试部分
            var TestCraftingTableModel = LDB.models.Select(303);
            var TestCraftingTableModel2 = LDB.models.Select(304);
            var TestCraftingTableModel3 = LDB.models.Select(305);
            var TestCraftingTableModel4 = LDB.models.Select(306);
            var AntiMatterModel = LDB.models.Select(307);
            //TestCraftingTableModel.Preload();
            TestCraftingTableModel.prefabDesc.isAssembler = true;
            TestCraftingTableModel.prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.Assemble;
            TestCraftingTableModel.prefabDesc.assemblerSpeed = 400000;
            TestCraftingTableModel.prefabDesc.isStation = false;
            TestCraftingTableModel.prefabDesc.isStellarStation = false;
            TestCraftingTableModel.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel.prefabDesc.workEnergyPerTick = 500000;
            TestCraftingTableModel.prefabDesc.slotPoses = TestCraftingTableModel.prefabDesc.portPoses;
            TestCraftingTableModel.prefabDesc.portPoses = Array.Empty<Pose>();
            TestCraftingTableModel.prefabDesc.slotPoses = new Pose[] {
                new Pose(new Vector3(1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(0f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-3.3f, 0.0f, 1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, 0.0f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, -1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(0.0f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(3.3f, 0.0f, -1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 0.0f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f))
            }; 
            TestCraftingTableModel2.prefabDesc.isAssembler = true;
            TestCraftingTableModel2.prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.Smelt;
            TestCraftingTableModel2.prefabDesc.assemblerSpeed = 400000;
            TestCraftingTableModel2.prefabDesc.isStation = false;
            TestCraftingTableModel2.prefabDesc.isStellarStation = false;
            TestCraftingTableModel2.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel2.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel2.prefabDesc.workEnergyPerTick = 500000;
            TestCraftingTableModel2.prefabDesc.slotPoses = TestCraftingTableModel2.prefabDesc.portPoses;
            TestCraftingTableModel2.prefabDesc.portPoses = Array.Empty<Pose>();
            TestCraftingTableModel2.prefabDesc.slotPoses = new Pose[] {
                new Pose(new Vector3(1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(0f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-3.3f, 0.0f, 1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, 0.0f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, -1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(0.0f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(3.3f, 0.0f, -1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 0.0f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f))
            };
            TestCraftingTableModel3.prefabDesc.isAssembler = true;
            TestCraftingTableModel3.prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.Chemical;
            TestCraftingTableModel3.prefabDesc.assemblerSpeed = 400000;
            TestCraftingTableModel3.prefabDesc.isStation = false;
            TestCraftingTableModel3.prefabDesc.isStellarStation = false;
            TestCraftingTableModel3.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel3.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel3.prefabDesc.workEnergyPerTick = 500000;
            TestCraftingTableModel3.prefabDesc.slotPoses = TestCraftingTableModel3.prefabDesc.portPoses;
            TestCraftingTableModel3.prefabDesc.portPoses = Array.Empty<Pose>();
            TestCraftingTableModel3.prefabDesc.slotPoses = new Pose[] {
                new Pose(new Vector3(1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(0f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-3.3f, 0.0f, 1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, 0.0f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, -1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(0.0f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(3.3f, 0.0f, -1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 0.0f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f))
            }; 
            TestCraftingTableModel4.prefabDesc.isAssembler = true;
            TestCraftingTableModel4.prefabDesc.assemblerRecipeType = (global::ERecipeType)ERecipeType.高精度加工;
            TestCraftingTableModel4.prefabDesc.assemblerSpeed = 400000;
            TestCraftingTableModel4.prefabDesc.isStation = false;
            TestCraftingTableModel4.prefabDesc.isStellarStation = false;
            TestCraftingTableModel4.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel4.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel4.prefabDesc.workEnergyPerTick = 500000;
            TestCraftingTableModel4.prefabDesc.slotPoses = TestCraftingTableModel4.prefabDesc.portPoses;
            TestCraftingTableModel4.prefabDesc.portPoses = Array.Empty<Pose>();
            //奇怪的测试
            //LDB.items.Select(2302).prefabDesc.portPoses = LDB.items.Select(2302).prefabDesc.slotPoses;
            //LDB.items.Select(2302).prefabDesc.slotPoses = Array.Empty<Pose>();
            //果然不行x_x
            TestCraftingTableModel4.prefabDesc.slotPoses = new Pose[] {
                new Pose(new Vector3(1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(0f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-1.3f, 0.0f, 3.3f), new Quaternion(0f, 0f, 0f, 1f)),
                new Pose(new Vector3(-3.3f, 0.0f, 1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, 0.0f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-3.3f, 0.0f, -1.3f), new Quaternion(0f, -0.7f, 0f, 0.7f)),
                new Pose(new Vector3(-1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(0.0f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(1.3f, 0.0f, -3.3f), new Quaternion(0f, -1f, 0f, 0f)),
                new Pose(new Vector3(3.3f, 0.0f, -1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 0.0f), new Quaternion(0f, 0.7f, 0f, 0.7f)),
                new Pose(new Vector3(3.3f, 0.0f, 1.3f), new Quaternion(0f, 0.7f, 0f, 0.7f))
            };

            LDB.items.Select(2211).prefabDesc.fuelMask = 5;
            LDB.items.Select(2210).prefabDesc.fuelMask = 6;
            AntiMatterModel.prefabDesc.fuelMask = 4;
            AntiMatterModel.prefabDesc.genEnergyPerTick = 1200000000;
            AntiMatterModel.prefabDesc.useFuelPerTick = 12000000;


            foreach (var proto in LDB.techs.dataArray)
            {
                proto.Preload();
            }

            for (var i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                LDB.items.dataArray[i].Preload(i);
            }

            for (var i = 0; i < LDB.recipes.dataArray.Length; ++i)
            {
                LDB.recipes.dataArray[i].Preload(i);
            }

            foreach (var proto in LDB.techs.dataArray)
            {
                proto.Preload2();
            }

            PostFix(LDB.items);

            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.fuelNeeds[5] = new int[] { 6241, 6242, 6243 };
            ItemProto.fuelNeeds[6] = new int[] { 1121, 6532, 1802, 6244, 6245 };
            foreach (var proto in LDB.items.dataArray)
            {
                StorageComponent.itemIsFuel[proto.ID] = proto.HeatValue > 0L;
                StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            var logo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            var image = logo.GetComponent<RawImage>();
            var rectTransform = logo.GetComponent<RectTransform>();

            var iconstr = Localization.language == Language.zhCN
                ? "Assets/texpack/中文图标"
                : "Assets/texpack/英文图标";

            image.texture = Resources.Load<Sprite>(iconstr).texture;

            rectTransform.sizeDelta = new Vector2(800f, 500f);
            rectTransform.anchoredPosition
                = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 50f);
        }

        private static ModelProto CopyModelProto(int oriId, int id, Color color)
        {
            var oriModel = LDB.models.Select(oriId);
            var model = oriModel.Copy();
            model.name = id.ToString();
            model.Name = id.ToString(); //这俩至少有一个必须加，否则LDBTool报冲突导致后面null
            model.ID = id;
            var desc = oriModel.prefabDesc;
            model.prefabDesc = new PrefabDesc(id, desc.prefab, desc.colliderPrefab);
            for (var i = 0; i < model.prefabDesc.lodMaterials.Length; i++)
            {
                if (model.prefabDesc.lodMaterials[i] == null) continue;
                for (var j = 0; j < model.prefabDesc.lodMaterials[i].Length; j++)
                {
                    if (model.prefabDesc.lodMaterials[i][j] == null) continue;
                    model.prefabDesc.lodMaterials[i][j] = new Material(desc.lodMaterials[i][j]);
                }
            }

            try
            {
                model.prefabDesc.lodMaterials[0][0].color = color;
                model.prefabDesc.lodMaterials[1][0].color = color;
                model.prefabDesc.lodMaterials[2][0].color = color;
            }
            catch
            {
                // ignored
            }

            model.prefabDesc.modelIndex = id;
            model.prefabDesc.hasBuildCollider = true;
            model.prefabDesc.colliders = desc.colliders;
            model.prefabDesc.buildCollider = desc.buildCollider;
            model.prefabDesc.buildColliders = desc.buildColliders;
            model.prefabDesc.colliderPrefab = desc.colliderPrefab;

            model.sid = "";
            model.SID = "";
            return model;
        }
    }
}
