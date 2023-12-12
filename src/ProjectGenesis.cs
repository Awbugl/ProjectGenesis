using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using NebulaAPI;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Patches.Logic.AddVein;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.Logic.PlanetFocus;
using ProjectGenesis.Patches.UI.PlanetFocus;
using ProjectGenesis.Patches.UI.QTools;
using ProjectGenesis.Utils;
using UnityEngine;
using xiaoye97;
using static ProjectGenesis.Patches.Logic.AddVein.AddVeinPatches;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;
using static ProjectGenesis.Patches.Logic.AddVein.AdjustPlanetTheme;

#pragma warning disable CS0618

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
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(TabSystem))]
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerMod
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        public const string MODNAME = "GenesisBook";
        public const string VERSION = "2.8.1";

        internal static ManualLogSource logger;
        internal static ConfigFile configFile;
        internal static UIPlanetFocusWindow PlanetFocusWindow;
        internal static UIQToolsWindow QToolsWindow;

        public static bool LoadCompleted;

        internal static int[] TableID;

        internal static string ModPath;

        internal static ConfigEntry<bool> EnableLDBToolCacheEntry, EnableHideTechModeEntry, DisableMessageBoxEntry;

        internal static ConfigEntry<KeyboardShortcut> QToolsHotkey;

        private Harmony Harmony;

        public void Awake()
        {
            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            configFile = Config;

            if (DSPBattleCompatibilityPlugin.DSPBattleInstalled)
            {
                logger.Log(LogLevel.Error, "They Come From Void is installed, which is incompatible with GenesisBook. Load Cancelled.");
                return;
            }

            #region Settings

            EnableLDBToolCacheEntry = Config.Bind("config", "UseLDBToolCache", false,
                                                  "Enable LDBTool Cache, which allows you use config to fix some compatibility issues.\n启用LDBTool缓存，允许使用配置文件修复部分兼容性问题");

            EnableHideTechModeEntry = Config.Bind("config", "HideTechMode", false,
                                                  "Enable Tech Exploration Mode, which will hide locked techs in tech tree.\n启用科技探索模式，启用后将隐藏未解锁的科技");

            DisableMessageBoxEntry = Config.Bind("config", "DiableMessageBox", false, "Don't show message when GenesisBook is loaded.\n禁用首次加载时的提示信息");

            QToolsHotkey = Config.Bind("config", "QToolsHotkey", KeyboardShortcut.Deserialize("BackQuote"), "Shortcut to open QTools window");

            Config.Save();

            #endregion Settings

            var executingAssembly = Assembly.GetExecutingAssembly();

            ModPath = Path.GetDirectoryName(executingAssembly.Location);

            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", ModPath);
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            var resources_models = new ResourceData("org.LoShin.GenesisBook", "genesis-models", ModPath);
            resources_models.LoadAssetBundle("genesis-models");
            ProtoRegistry.AddResource(resources_models);

            var stoneVeinShader = resources_models.bundle.LoadAsset<Shader>("Assets/genesis-models/shaders/PBR Standard Vein Stone COLOR.shader");
            SwapShaderPatches.AddSwapShaderMapping("VF Shaders/Forward/PBR Standard Vein Stone", stoneVeinShader);

            var metalVeinShader = resources_models.bundle.LoadAsset<Shader>("Assets/genesis-models/shaders/PBR Standard Vein Metal COLOR.shader");
            SwapShaderPatches.AddSwapShaderMapping("VF Shaders/Forward/PBR Standard Vein Metal", metalVeinShader);

            TableID = new int[]
                      {
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab1",
                                                new TabData("精炼页面".TranslateFromJson(), "Icons/Tech/1101")),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                                new TabData("化工页面".TranslateFromJson(), "Assets/texpack/化工科技"))
                      };

            NebulaModAPI.RegisterPackets(executingAssembly);

            NebulaModAPI.OnPlanetLoadRequest += planetId =>
            {
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new GenesisBookPlanetLoadRequest(planetId));
            };

            NebulaModAPI.OnPlanetLoadFinished += GenesisBookPlanetDataProcessor.ProcessBytesLater;

            Harmony = new Harmony(MODGUID);

            IEnumerable<Type> types = from t in executingAssembly.GetTypes()
                                      where t.IsClass && !string.IsNullOrWhiteSpace(t.Namespace) && t.Namespace.StartsWith("ProjectGenesis.Patches")
                                      select t;

            foreach (Type type in types)
            {
                Harmony.PatchAll(type);

                GC.Collect();
            }

            ModifyVeinData();

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;

            LoadCompleted = true;
        }

        private void Update()
        {
            if (VFInput.inputing) return;
            if (QToolsWindow) QToolsWindow._OnUpdate();
        }

        public void Export(BinaryWriter w)
        {
            MegaAssemblerPatches.Export(w);
            PlanetFocusPatches.Export(w);
        }

        public void Import(BinaryReader r)
        {
            MegaAssemblerPatches.Import(r);
            PlanetFocusPatches.Import(r);
        }

        public void IntoOtherSave()
        {
            MegaAssemblerPatches.IntoOtherSave();
            PlanetFocusPatches.IntoOtherSave();
        }

        public string Version => VERSION;

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);

        private void PreAddDataAction()
        {
            LDB.items.OnAfterDeserialize();
            AdjustPlanetThemeDataVanilla();
            AddCopiedModelProto();
            AddEffectEmitterProto();
            ImportJson(TableID);
        }

        private void PostAddDataAction()
        {
            LDB.strings.OnAfterDeserialize();
            Localization._strings = LDB.strings;

            //飞行舱拆除
            VegeProto vegeProto = LDB.veges.Select(9999);
            vegeProto.MiningItem = new[] { 6216, 1101, 1104 };
            vegeProto.MiningCount = new[] { 3, 60, 60 };
            vegeProto.MiningChance = new float[] { 1, 1, 1 };
            vegeProto.Preload();

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
                GameMain.instance.CreateMultithreadSystem();
            }

            PrefabDescPostFix();

            foreach (MilestoneProto milestone in LDB.milestones.dataArray) milestone.Preload();
            foreach (JournalPatternProto journalPattern in LDB.journalPatterns.dataArray) journalPattern.Preload();

            foreach (VeinProto proto in LDB.veins.dataArray)
            {
                proto.Preload();
                proto.name = proto.Name.Translate();
            }

            foreach (TechProto proto in LDB.techs.dataArray) proto.Preload();

            for (int i = 0; i < LDB.items.dataArray.Length; ++i)
            {
                LDB.items.dataArray[i].recipes = null;
                LDB.items.dataArray[i].rawMats = null;
                LDB.items.dataArray[i].Preload(i);
            }

            for (int i = 0; i < LDB.recipes.dataArray.Length; ++i)
            {
                LDB.recipes.dataArray[i].Preload(i);
            }

            foreach (TechProto proto in LDB.techs.dataArray)
            {
                proto.PreTechsImplicit = proto.PreTechsImplicit.Except(proto.PreTechs).ToArray();
                proto.UnlockRecipes = proto.UnlockRecipes.Distinct().ToArray();
                proto.Preload2();
            }

            SetMinerMk2Color();
            SetBuildBar();

            SetEffectEmitterProto();
            VFEffectEmitter.Init();

            ItemProto.InitFluids();
            ItemProto.InitItemIds();
            ItemProto.InitFuelNeeds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.stationCollectorId = 2105;

            LDB.items.Select(1000).recipes = new List<RecipeProto>() { LDB.recipes.Select(801) };
            LDB.items.Select(1120).isRaw = true;

            StorageComponent.staticLoaded = false;
            StorageComponent.LoadStatic();

            // JsonHelper.ExportAsJson(@"D:\Git\ProjectGenesis\data");
        }

        internal static void SetConfig(bool currentLDBToolCache, bool currentHideTechMode, bool currentDisableMessageBox)
        {
            EnableLDBToolCacheEntry.Value = currentLDBToolCache;
            EnableHideTechModeEntry.Value = currentHideTechMode;
            DisableMessageBoxEntry.Value = currentDisableMessageBox;
            logger.LogInfo("SettingChanged");
            configFile.Save();
        }
    }
}
