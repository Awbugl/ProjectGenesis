using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using xiaoye97;
using crecheng.DSPModSave;
using NebulaAPI;
using NebulaAPI.Interfaces;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Patches;
using ProjectGenesis.Utils;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;
using static ProjectGenesis.Utils.TranslateUtils;
using static ProjectGenesis.Utils.ModifyUpgradeTech;
using static ProjectGenesis.Patches.AddVeinPatches;
using static ProjectGenesis.Patches.ModifyPlanetTheme;
using static ProjectGenesis.Patches.ChemicalRecipeFcolPatches;
using static ProjectGenesis.Compatibility.InstallationCheckPlugin;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [BepInDependency(InstallationCheckPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(TabSystem), nameof(LocalizationModule))]
    [ModSaveSettings(LoadOrder = LoadOrder.Preload)]
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerModWithSettings
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        public const string MODNAME = "GenesisBook";
        public const string VERSION = "3.2.0";
        public const string DEBUGVERSION = "alpha1.2";

        public static bool LoadCompleted;

        internal static ManualLogSource logger;
        internal static ConfigFile configFile;
        internal static UIPlanetFocusWindow PlanetFocusWindow;

        internal static int[] TableID;

        internal static string ModPath;

        internal static ConfigEntry<bool> LDBToolCacheEntry, HideTechModeEntry, ShowMessageBoxEntry;

        internal static ConfigEntry<int> ProductOverflowEntry;

        private Harmony Harmony;

        public void Awake()
        {
        #region Logger

            logger = Logger;

            if (!BepinExVersionMatch)
            {
                logger.Log(LogLevel.Error, "GenesisBook BepinEx Version Check Failed");
                return;
            }

            logger.Log(LogLevel.Info, "GenesisBook Awake");

        #endregion Logger

        #region Configs

            configFile = Config;

            LDBToolCacheEntry = Config.Bind("config", "UseLDBToolCache", false,
                "Enable LDBTool Cache, which allows you use config to fix some compatibility issues.\n启用LDBTool缓存，允许使用配置文件修复部分兼容性问题");

            HideTechModeEntry = Config.Bind("config", "HideTechMode", true,
                "Enable Tech Exploration Mode, which will hide locked techs in tech tree.\n启用科技探索模式，启用后将隐藏未解锁的科技");

            ShowMessageBoxEntry = Config.Bind("config", "ShowMessageBox", true, "Show message when GenesisBook is loaded.\n首次加载时的提示信息");

            ProductOverflowEntry = Config.Bind("config", "ProductOverflow", 0,
                "Changing the condition for stopping production of some recipes from single product pile up to all product pile up.\n将部分配方停止生产的条件由单产物堆积改为所有产物均堆积");

            Config.Save();

        #endregion Configs

        #region ResourceData

            var executingAssembly = Assembly.GetExecutingAssembly();

            ModPath = Path.GetDirectoryName(executingAssembly.Location);

            var resources_models = new ResourceData("org.LoShin.GenesisBook", "genesis-models", ModPath);
            resources_models.LoadAssetBundle("genesis-models");
            ProtoRegistry.AddResource(resources_models);

            var resources_lab = new ResourceData("org.LoShin.GenesisBook", "genesis-models-lab", ModPath);
            resources_lab.LoadAssetBundle("genesis-models-lab");
            ProtoRegistry.AddResource(resources_lab);

            Shader stoneVeinShader =
                resources_models.bundle.LoadAsset<Shader>("Assets/genesis-models/shaders/PBR Standard Vein Stone COLOR.shader");
            SwapShaderPatches.AddSwapShaderMapping("VF Shaders/Forward/PBR Standard Vein Stone", stoneVeinShader);

            Shader metalVeinShader =
                resources_models.bundle.LoadAsset<Shader>("Assets/genesis-models/shaders/PBR Standard Vein Metal COLOR.shader");
            SwapShaderPatches.AddSwapShaderMapping("VF Shaders/Forward/PBR Standard Vein Metal", metalVeinShader);

            Shader labToggleShader =
                resources_lab.bundle.LoadAsset<Shader>("Assets/genesis-models/shaders/PBR Standard Vertex Toggle Lab REPLACE.shader");
            SwapShaderPatches.AddSwapShaderMapping("VF Shaders/Forward/PBR Standard Vertex Toggle Lab", labToggleShader);

        #endregion ResourceData

        #region NebulaModAPI

            NebulaModAPI.RegisterPackets(executingAssembly);

            NebulaModAPI.OnPlanetLoadRequest += planetId =>
            {
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new GenesisBookPlanetLoadRequest(planetId));
            };

            NebulaModAPI.OnPlanetLoadFinished += GenesisBookPlanetDataProcessor.ProcessBytesLater;

        #endregion NebulaModAPI

            Harmony = new Harmony(MODGUID);

            foreach (Type type in executingAssembly.GetTypes())
                if (type.Namespace?.StartsWith("ProjectGenesis.Patches", StringComparison.Ordinal) == true)
                    Harmony.PatchAll(type);

            TableID = new int[]
            {
                TabSystem.RegisterTab($"{MODGUID}:{MODGUID}Tab2",
                    new TabData("化工页面".TranslateFromJsonSpecial(), "Assets/texpack/T基础化工")),
                TabSystem.RegisterTab($"{MODGUID}:{MODGUID}Tab3", new TabData("防御页面".TranslateFromJsonSpecial(), "Assets/texpack/O防御")),
            };

            RegisterStrings();
            ModifyVeinData();

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;

            LoadCompleted = true;
        }

        public void Export(BinaryWriter w)
        {
            w.Write(VersionNumber());
            MegaAssemblerPatches.Export(w);
            PlanetFocusPatches.Export(w);
            QuantumStoragePatches.Export(w);
            AdvancedLaserPatches.Export(w);
            GlobalPowerSupplyPatches.Export(w);
        }

        public void Import(BinaryReader r)
        {
            int version = r.ReadInt32();
            MegaAssemblerPatches.Import(r);
            PlanetFocusPatches.Import(r);
            QuantumStoragePatches.Import(r);
            AdvancedLaserPatches.Import(r);
            GlobalPowerSupplyPatches.Import(r);
        }

        public void IntoOtherSave()
        {
            MegaAssemblerPatches.IntoOtherSave();
            PlanetFocusPatches.IntoOtherSave();
            QuantumStoragePatches.IntoOtherSave();
            AdvancedLaserPatches.IntoOtherSave();
            GlobalPowerSupplyPatches.IntoOtherSave();
        }

        public string Version => VERSION;

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);

        private void PreAddDataAction()
        {
            LDB.items.OnAfterDeserialize();
            ModifyPlanetThemeDataVanilla();
            AddCopiedModelProto();
            AddEffectEmitterProto();
            ImportJson(TableID);
            ModifyUpgradeTeches();
        }

        private void PostAddDataAction()
        {
            //飞行舱拆除
            VegeProto vegeProto = LDB.veges.Select(9999);
            vegeProto.MiningItem = new[]
            {
                ProtoID.I四氢双环戊二烯燃料棒, ProtoID.I铁块, ProtoID.I铜块, ProtoID.I铝块,
            };
            vegeProto.MiningCount = new[]
            {
                10, 100, 100, 100,
            };
            vegeProto.MiningChance = new float[]
            {
                1, 1, 1, 1,
            };
            vegeProto.Preload();

            LabComponent.matrixPoints = new int[9];

            LabComponent.matrixIds = new[]
            {
                ProtoID.I电磁矩阵, ProtoID.I能量矩阵, ProtoID.I结构矩阵, ProtoID.I信息矩阵,
                ProtoID.I引力矩阵, ProtoID.I宇宙矩阵, ProtoID.I玻色矩阵, ProtoID.I耗散矩阵,
                ProtoID.I奇点矩阵,
            };

            LabComponent.matrixShaderStates = new[]
            {
                0.0f, 11111.2f, 22222.2f, 33333.2f,
                44444.2f, 55555.2f, 66666.2f, 77777.2f,
                88888.2f, 99999.2f,
            };

            LDB.items.OnAfterDeserialize();
            LDB.recipes.OnAfterDeserialize();
            LDB.techs.OnAfterDeserialize();
            LDB.models.OnAfterDeserialize();
            LDB.milestones.OnAfterDeserialize();
            LDB.journalPatterns.OnAfterDeserialize();
            LDB.themes.OnAfterDeserialize();
            LDB.veins.OnAfterDeserialize();

            if (GameMain.instance != null)
            {
                GameMain.instance.CreateGPUInstancing();
                GameMain.instance.CreateBPGPUInstancing();
                GameMain.instance.CreateStarmapGPUInstancing();
            }

            PrefabDescPostFix();
            ModelPostFix();

            ProtoPreload();

            ModifyEnemyHpUpgrade();
            SetMinerMk2Color();
            SetChemicalRecipeFcol();
            SetEffectEmitterProto();

            VFEffectEmitter.Init();

            ItemProto.InitFuelNeeds();
            ItemProto.InitTurretNeeds();
            ItemProto.InitFluids();
            ItemProto.InitTurrets();
            ItemProto.InitEnemyDropTables();
            ItemProto.InitConstructableItems();
            ItemProto.InitItemIds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.InitFighterIndices();
            ItemProto.InitPowerFacilityIndices();
            ItemProto.InitProductionMask();
            ModelProto.InitMaxModelIndex();
            ModelProto.InitModelIndices();
            ModelProto.InitModelOrders();
            RecipeProto.InitRecipeItems();
            RecipeProto.InitFractionatorNeeds();
            SignalProtoSet.InitSignalKeyIdPairs();
            RaycastLogic.LoadStatic();
            
            ref int[] turretNeed = ref ItemProto.turretNeeds[(int)EAmmoType.Bullet];
            turretNeed[1] = ProtoID.I钢芯弹箱;
            turretNeed[2] = ProtoID.I超合金弹箱;

            ItemProto.stationCollectorId = ProtoID.I轨道采集器;
            ItemProto.kFuelAutoReplenishIds = FuelRodPatches.FuelRods;

            ItemPostFix();

            StorageComponent.staticLoaded = false;
            StorageComponent.LoadStatic();

            UIBuildMenu.staticLoaded = false;
            UIBuildMenu.StaticLoad();

            SpaceSector.PrefabDescByModelIndex = null;
            SpaceSector.InitPrefabDescArray();

            PlanetFactory.PrefabDescByModelIndex = null;
            PlanetFactory.InitPrefabDescArray();

            ref MechaMaterialSetting material = ref Configs.builtin.mechaArmorMaterials[21];
            material.itemId = ProtoID.I钨块;
            material.density = 19.35f;
            material.durability = 4.35f;

            DevFunction();
        }

        private static void DevFunction()
        {
            // JsonHelper.ExportAsJson(@"D:\Git\ProjectGenesis\data");

            // GenerateRecipeTexture(@"D:\Git\ProjectGenesis\assets\texture\化工厂渲染索引-new.png");
        }

        private static void ProtoPreload()
        {
            foreach (MilestoneProto milestone in LDB.milestones.dataArray) milestone.Preload();

            foreach (JournalPatternProto journalPattern in LDB.journalPatterns.dataArray) journalPattern.Preload();

            foreach (VeinProto proto in LDB.veins.dataArray) proto.Preload();

            foreach (TechProto proto in LDB.techs.dataArray) proto.Preload();

            for (var i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                ItemProto itemProto = LDB.items.dataArray[i];
                itemProto.recipes = null;
                itemProto.rawMats = null;
                itemProto.Preload(i);
            }

            for (var i = 0; i < LDB.recipes.dataArray.Length; ++i) LDB.recipes.dataArray[i].Preload(i);

            foreach (TechProto proto in LDB.techs.dataArray)
            {
                proto.PreTechsImplicit = proto.PreTechsImplicit.Except(proto.PreTechs).ToArray();
                proto.UnlockRecipes = proto.UnlockRecipes.Distinct().ToArray();
                proto.Preload2();
            }
        }

        internal static void SetConfig(bool currentLDBToolCache, bool currentHideTechMode, bool currentShowMessageBox,
            int currentProductOverflow)
        {
            LDBToolCacheEntry.Value = currentLDBToolCache;
            HideTechModeEntry.Value = currentHideTechMode;
            ShowMessageBoxEntry.Value = currentShowMessageBox;
            ProductOverflowEntry.Value = currentProductOverflow;
            logger.LogInfo("SettingChanged");
            configFile.Save();
        }

        internal static void LogInfo(object data) => logger.LogInfo(data);

        internal static int VersionNumber()
        {
            var version = new Version();
            version.FromFullString(VERSION);
            return version.sig;
        }
    }
}
