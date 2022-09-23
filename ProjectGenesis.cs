using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using NebulaAPI;
using ProjectGenesis.Patches;
using UnityEngine;
using xiaoye97;
using ERecipeType_1 = ERecipeType;

#pragma warning disable CS0618

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace ProjectGenesis
{
    [BepInPlugin(MODGUID, "GenesisBook", "2.0.0")]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem), nameof(AssemblerRecipeSystem))]

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBeInternal
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerMod
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        private const string VERSION = "2.0.0";

        private static ProjectGenesis Instance { get; set; }
        private Harmony Harmony { get; set; }

        private int[] TableID = new int[3];

        public string Version => VERSION;

        private static ManualLogSource logger;

        //无限堆叠开关(私货)
        private readonly bool StackSizeButton = false;

        public void Awake()
        {
            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", pluginfolder);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            TableID = new int[]
                      {
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab1", new TabData("精炼", "Assets/texpack/主机科技")),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2", new TabData("化工", "Assets/texpack/化工科技")),
                      };


            Instance = this;

            LDBTool.PreAddDataAction += InitData;
            LDBTool.PostAddDataAction += PostAddDataAction;

            NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());

            Harmony = new Harmony(MODGUID);
            Harmony.PatchAll(typeof(UIPatches));
            Harmony.PatchAll(typeof(MultiProductionPatches));
            Harmony.PatchAll(typeof(MutliPlayerPatches));
            Harmony.PatchAll(typeof(PlanetGasPatches));
            Harmony.PatchAll(typeof(OceanDischargePatches));
            Harmony.PatchAll(typeof(MegaAssemblerPatches));
        }

        public void OnDestroy()
        {
            Harmony.UnpatchAll();
            Harmony = null;
        }

        private int GetTableID(int gridIndex)
        {
            if (gridIndex >= 4000) return TableID[1] * 1000 + (gridIndex - 4000);
            if (gridIndex >= 3000) return TableID[0] * 1000 + (gridIndex - 3000);
            return gridIndex;
        }

        private void InitData()
        {
            PreFix();

            #region ModelProto

            var TankModel = CopyModelProto(121, 401, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            LDBTool.PreAddProto(TankModel);
            var OreFactoryModel = CopyModelProto(194, 402, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            LDBTool.PreAddProto(OreFactoryModel);

            var TestCraftingTableModel = CopyModelProto(49, 403, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            LDBTool.PreAddProto(TestCraftingTableModel);
            var TestCraftingTableModel2 = CopyModelProto(49, 404, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(TestCraftingTableModel2);
            var TestCraftingTableModel3 = CopyModelProto(49, 405, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            LDBTool.PreAddProto(TestCraftingTableModel3);
            var TestCraftingTableModel4 = CopyModelProto(49, 406, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            LDBTool.PreAddProto(TestCraftingTableModel4);
            var TestCraftingTableModel5 = CopyModelProto(49, 410, new Color(0.3216F, 0.8157F, 0.09020F));
            LDBTool.PreAddProto(TestCraftingTableModel5);
            var AntiMatterModel = CopyModelProto(118, 407, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            LDBTool.PreAddProto(AntiMatterModel);
            var AssembleModel = CopyModelProto(67, 408, Color.HSVToRGB(0.9688f, 0.9068f, 0.9255f));
            LDBTool.PreAddProto(AssembleModel);
            var CircleModel = CopyModelProto(69, 409, Color.grey);
            LDBTool.PreAddProto(CircleModel);

            #endregion

            #region StringProto

            foreach (var stringProto in JsonHelper.StringProtos())
            {
                if (LDB.strings.Exist(stringProto.Name))
                    ProtoRegistry.EditString(stringProto.Name, stringProto.ENUS, stringProto.ZHCN);
                else
                    ProtoRegistry.RegisterString(stringProto.Name, stringProto.ENUS, stringProto.ZHCN);
            }

            #endregion

            #region TechProto

            var templateTech = LDB.techs.Select(1311);

            foreach (var techjson in JsonHelper.TechProtos())
            {
                var proto = LDB.techs.Exist(techjson.ID) ? LDB.techs.Select(techjson.ID) : templateTech.Copy();
                proto.ID = techjson.ID;
                proto.Name = techjson.Name;
                proto.Desc = techjson.Desc;
                proto.Conclusion = techjson.Conclusion;
                proto.Published = techjson.Published;
                proto.IconPath = techjson.IconPath;
                proto.IsLabTech = techjson.IsLabTech;
                proto.PreTechs = techjson.PreTechs;
                proto.PreTechsImplicit = techjson.PreTechsImplicit;
                proto.PreTechsMax = techjson.PreTechsMax;
                proto.Items = techjson.Items ?? Array.Empty<int>();
                proto.ItemPoints = techjson.ItemPoints ?? Array.Empty<int>();
                proto.AddItems = techjson.AddItems ?? Array.Empty<int>();
                proto.AddItemCounts = techjson.AddItemCounts ?? Array.Empty<int>();
                proto.Position = new Vector2(techjson.Position[0], techjson.Position[1]);
                proto.HashNeeded = techjson.HashNeeded;
                proto.UnlockRecipes = techjson.UnlockRecipes;
                proto.UnlockFunctions = techjson.UnlockFunctions;
                proto.UnlockValues = techjson.UnlockValues;
                proto.Level = techjson.Level;
                proto.MaxLevel = techjson.MaxLevel;
                proto.LevelCoef1 = techjson.LevelCoef1;
                proto.LevelCoef2 = techjson.LevelCoef2;
                proto.PropertyOverrideItems = techjson.PropertyOverrideItems;
                proto.PropertyItemCounts = techjson.PropertyItemCounts;

                if (!LDB.techs.Exist(techjson.ID)) LDBTool.PreAddProto(proto);
            }

            #endregion

            #region ItemProto

            foreach (var itemjson in JsonHelper.ItemProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);

                var proto = LDB.items.Exist(itemjson.ID)
                                ? LDB.items.Select(itemjson.ID)
                                : ProtoRegistry.RegisterItem(itemjson.ID, itemjson.Name, itemjson.Description, itemjson.IconPath, itemjson.GridIndex,
                                                             itemjson.StackSize, (EItemType)itemjson.Type);

                proto.ID = itemjson.ID;
                proto.Name = itemjson.Name;
                proto.Description = itemjson.Description;
                proto.IconPath = itemjson.IconPath;
                proto.StackSize = StackSizeButton ? 10000000 : itemjson.StackSize;
                proto.GridIndex = itemjson.GridIndex;
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
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.PreTechOverride = itemjson.PreTechOverride;
                proto.Productive = itemjson.Productive;
            }

            #endregion

            #region RecipeProto

            foreach (var recipeJson in JsonHelper.RecipeProtos())
            {
                if (!LDB.recipes.Exist(recipeJson.ID))
                {
                    recipeJson.GridIndex = GetTableID(recipeJson.GridIndex);

                    var proto = ProtoRegistry.RegisterRecipe(recipeJson.ID, (ERecipeType_1)recipeJson.Type, recipeJson.Time, recipeJson.Input,
                                                             recipeJson.InCounts, recipeJson.Output ?? Array.Empty<int>(),
                                                             recipeJson.OutCounts ?? Array.Empty<int>(), recipeJson.Description, recipeJson.PreTech,
                                                             recipeJson.GridIndex, recipeJson.Name, recipeJson.IconPath);

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
                    proto.Type = (ERecipeType_1)recipeJson.Type;
                    proto.TimeSpend = recipeJson.Time;
                    proto.Items = recipeJson.Input;
                    proto.ItemCounts = recipeJson.InCounts;
                    proto.Results = recipeJson.Output ?? Array.Empty<int>();
                    proto.ResultCounts = recipeJson.OutCounts ?? Array.Empty<int>();
                    proto.Description = recipeJson.Description;
                    proto.preTech = LDB.techs.Select(recipeJson.PreTech);
                    proto.GridIndex = recipeJson.GridIndex;
                    proto.IconPath = recipeJson.IconPath;
                    proto.NonProductive = recipeJson.NonProductive;
                }
            }

            #endregion
        }

        private void PreFix()
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

            LDB.items.OnAfterDeserialize();
        }

        private void PostAddDataAction()
        {
            LDB.items.OnAfterDeserialize();
            LDB.recipes.OnAfterDeserialize();
            LDB.models.OnAfterDeserialize();
            GameMain.gpuiManager.Init();

            //飞行舱拆除
            var @base = LDB.veges.Select(9999);
            @base.MiningItem = new[] { 1801, 1101, 1104 };
            @base.MiningCount = new[] { 3, 10, 20 };
            @base.MiningChance = new float[] { 1, 1, 1 };
            @base.Preload();

            foreach (var proto in LDB.techs.dataArray) proto.Preload();

            for (var i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                LDB.items.dataArray[i].recipes = null;
                LDB.items.dataArray[i].rawMats = null;
                LDB.items.dataArray[i].Preload(i);
            }

            for (var i = 0; i < LDB.recipes.dataArray.Length; ++i) LDB.recipes.dataArray[i].Preload(i);

            foreach (var proto in LDB.techs.dataArray)
            {
                proto.PreTechsImplicit = proto.PreTechsImplicit.Except(proto.PreTechs).ToArray();
                proto.UnlockRecipes = proto.UnlockRecipes.Distinct().ToArray();
                proto.Preload2();
            }

            ModelPostFix(LDB.models);
            ItemPostFix(LDB.items);

            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.fuelNeeds[4] = new int[] { 6533, 1803 };
            ItemProto.fuelNeeds[5] = new int[] { 6241, 6242, 6243 };
            ItemProto.fuelNeeds[6] = new int[] { 1121, 6532, 1802, 6244, 6245 };

            foreach (var proto in LDB.items.dataArray)
            {
                StorageComponent.itemIsFuel[proto.ID] = proto.HeatValue > 0L;

                if (StackSizeButton)
                    StorageComponent.itemStackCount[proto.ID] = 10000000;
                else
                    StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            // JsonHelper.ExportAsJson(@"C:\Git\ProjectGenesis");
        }

        #region PrefabDescPostFix

        private void ItemPostFix(ItemProtoSet itemProtos)
        {
            LDB.items.OnAfterDeserialize();

            itemProtos.Select(物品.二级制造台).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.电路蚀刻;
            itemProtos.Select(物品.三级制造台).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.高精度加工;

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

            itemProtos.Select(6230).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.矿物处理;
            itemProtos.Select(6230).prefabDesc.idleEnergyPerTick = 400;
            itemProtos.Select(6230).prefabDesc.workEnergyPerTick = 12000;

            itemProtos.Select(6229).prefabDesc.fluidStorageCount = 1000000;
            itemProtos.Select(6275).prefabDesc.assemblerSpeed = 500;
            itemProtos.Select(6275).prefabDesc.workEnergyPerTick = 2000000;
            itemProtos.Select(6275).prefabDesc.idleEnergyPerTick = 100000;
            itemProtos.Select(6275).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.精密组装;
            itemProtos.Select(6276).prefabDesc.assemblerSpeed = 1000;
            itemProtos.Select(6276).prefabDesc.workEnergyPerTick = 8000000;
            itemProtos.Select(6276).prefabDesc.idleEnergyPerTick = 200000;
            itemProtos.Select(6276).prefabDesc.assemblerSpeed = 1000;
            itemProtos.Select(6276).prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.聚变生产;

            LDBTool.SetBuildBar(1, 10, 6261);
            LDBTool.SetBuildBar(3, 10, 2313);
            LDBTool.SetBuildBar(4, 4, 6229);
            LDBTool.SetBuildBar(5, 3, 6230);
            LDBTool.SetBuildBar(5, 4, 2303);
            LDBTool.SetBuildBar(5, 5, 2304);
            LDBTool.SetBuildBar(5, 6, 2305);
            LDBTool.SetBuildBar(5, 7, 6275);
            LDBTool.SetBuildBar(5, 8, 2308);
            LDBTool.SetBuildBar(5, 9, 2309);
            LDBTool.SetBuildBar(5, 10, 2310);
            LDBTool.SetBuildBar(7, 9, 6276);
            LDBTool.SetBuildBar(7, 3, 6257);
            LDBTool.SetBuildBar(7, 4, 6258);
            LDBTool.SetBuildBar(7, 5, 6259);
            LDBTool.SetBuildBar(7, 6, 6260);
            LDBTool.SetBuildBar(7, 7, 6264);

            //矿场修复
            var OreFactory = LDB.items.Select(6230);
            var OreFactoryModel = LDB.items.Select(302);
            OreFactoryModel.prefabDesc.dragBuild = true;
            OreFactoryModel.prefabDesc.dragBuildDist = new Vector2(2.9f, 2.9f);
            OreFactory.prefabDesc.dragBuild = true;
            OreFactory.prefabDesc.dragBuildDist = new Vector2(2.9f, 2.9f);
        }

        private void ModelPostFix(ModelProtoSet models)
        {
            //行星装配站调试部分
            var TestCraftingTableModel = models.Select(403);
            var TestCraftingTableModel2 = models.Select(404);
            var TestCraftingTableModel3 = models.Select(405);
            var TestCraftingTableModel4 = models.Select(406);
            var TestCraftingTableModel5 = models.Select(410);
            var AntiMatterModel = models.Select(407);

            TestCraftingTableModel.prefabDesc.isAssembler = true;
            TestCraftingTableModel.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.Assemble;
            TestCraftingTableModel.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            TestCraftingTableModel.prefabDesc.isStation = false;
            TestCraftingTableModel.prefabDesc.isStellarStation = false;
            TestCraftingTableModel.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel.prefabDesc.workEnergyPerTick = 500000;

            TestCraftingTableModel2.prefabDesc.isAssembler = true;
            TestCraftingTableModel2.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.Smelt;
            TestCraftingTableModel2.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            TestCraftingTableModel2.prefabDesc.isStation = false;
            TestCraftingTableModel2.prefabDesc.isStellarStation = false;
            TestCraftingTableModel2.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel2.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel2.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel2.prefabDesc.workEnergyPerTick = 500000;

            TestCraftingTableModel3.prefabDesc.isAssembler = true;
            TestCraftingTableModel3.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.Chemical;
            TestCraftingTableModel3.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            TestCraftingTableModel3.prefabDesc.isStation = false;
            TestCraftingTableModel3.prefabDesc.isStellarStation = false;
            TestCraftingTableModel3.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel3.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel3.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel3.prefabDesc.workEnergyPerTick = 500000;

            TestCraftingTableModel4.prefabDesc.isAssembler = true;
            TestCraftingTableModel4.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.高精度加工;
            TestCraftingTableModel4.prefabDesc.assemblerSpeed = MegaAssemblerPatches.MegaAssemblerSpeed;
            TestCraftingTableModel4.prefabDesc.isStation = false;
            TestCraftingTableModel4.prefabDesc.isStellarStation = false;
            TestCraftingTableModel4.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel4.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel4.prefabDesc.idleEnergyPerTick = 100000;
            TestCraftingTableModel4.prefabDesc.workEnergyPerTick = 500000;

            TestCraftingTableModel5.prefabDesc.isAssembler = true;
            TestCraftingTableModel5.prefabDesc.assemblerRecipeType = (ERecipeType_1)ERecipeType.垃圾回收;
            TestCraftingTableModel5.prefabDesc.assemblerSpeed = MegaAssemblerPatches.TrashSpeed;
            TestCraftingTableModel5.prefabDesc.isStation = false;
            TestCraftingTableModel5.prefabDesc.isStellarStation = false;
            TestCraftingTableModel5.prefabDesc.stationMaxDroneCount = 0;
            TestCraftingTableModel5.prefabDesc.stationMaxEnergyAcc = 0;
            TestCraftingTableModel5.prefabDesc.stationMaxItemCount = 0;
            TestCraftingTableModel5.prefabDesc.stationMaxItemKinds = 0;
            TestCraftingTableModel5.prefabDesc.stationMaxShipCount = 0;
            TestCraftingTableModel5.prefabDesc.idleEnergyPerTick = 200000;
            TestCraftingTableModel5.prefabDesc.workEnergyPerTick = 1000000;

            LDB.items.Select(2211).prefabDesc.fuelMask = 5;
            LDB.items.Select(2210).prefabDesc.fuelMask = 6;

            AntiMatterModel.prefabDesc.fuelMask = 4;
            AntiMatterModel.prefabDesc.genEnergyPerTick = 1200000000;
            AntiMatterModel.prefabDesc.useFuelPerTick = 1200000000;
        }

        #endregion

        #region CopyModelProto

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

        #endregion

        #region IModCanSave

        public void Export(BinaryWriter w) => MegaAssemblerPatches.Export(w);

        public void Import(BinaryReader r) => MegaAssemblerPatches.Import(r);

        internal static byte[] Export()
        {
            if (Instance != null)
                using (var p = NebulaModAPI.GetBinaryWriter())
                {
                    Instance.Export(p.BinaryWriter);
                    return p.CloseAndGetBytes();
                }

            return Array.Empty<byte>();
        }

        internal static void Import(byte[] bytes)
        {
            if (Instance != null)
                using (var p = NebulaModAPI.GetBinaryReader(bytes))
                    Instance.Import(p.BinaryReader);
        }

        public void IntoOtherSave() => MegaAssemblerPatches.IntoOtherSave();

        #endregion

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);
    }
}
