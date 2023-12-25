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
using CommonAPI.Systems.ModLocalization;
using crecheng.DSPModSave;
using HarmonyLib;
using NebulaAPI;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.Logic.AddVein;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.Logic.PlanetFocus;
using ProjectGenesis.Patches.UI.PlanetFocus;
using ProjectGenesis.Patches.UI.QTools;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using xiaoye97;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;
using static ProjectGenesis.Utils.TranslateUtils;
using static ProjectGenesis.Patches.Logic.AddVein.AddVeinPatches;
using static ProjectGenesis.Patches.Logic.AddVein.ModifyPlanetTheme;

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
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerMod
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        public const string MODNAME = "GenesisBook";
        public const string VERSION = "2.9.0";
        public const string DEBUGVERSION = "";

        public static bool LoadCompleted;

        internal static ManualLogSource logger;
        internal static ConfigFile configFile;
        internal static UIPlanetFocusWindow PlanetFocusWindow;
        internal static UIQToolsWindow QToolsWindow;

        internal static int[] TableID;

        internal static string ModPath;

        internal static ConfigEntry<bool> EnableLDBToolCacheEntry, EnableHideTechModeEntry, DisableMessageBoxEntry;

        internal static ConfigEntry<KeyboardShortcut> QToolsHotkey;

        private Harmony Harmony;

        public void Awake()
        {
#region Logger

            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            if (DSPBattleCompatibilityPlugin.DSPBattleInstalled)
            {
                logger.Log(LogLevel.Error, "They Come From Void is installed, which is incompatible with GenesisBook. Load Cancelled.");
                return;
            }

#endregion Logger

#region Configs

            configFile = Config;

            EnableLDBToolCacheEntry = Config.Bind("config", "UseLDBToolCache", false,
                                                  "Enable LDBTool Cache, which allows you use config to fix some compatibility issues.\n启用LDBTool缓存，允许使用配置文件修复部分兼容性问题");

            EnableHideTechModeEntry = Config.Bind("config", "HideTechMode", true,
                                                  "Enable Tech Exploration Mode, which will hide locked techs in tech tree.\n启用科技探索模式，启用后将隐藏未解锁的科技");

            DisableMessageBoxEntry = Config.Bind("config", "DiableMessageBox", false, "Don't show message when GenesisBook is loaded.\n禁用首次加载时的提示信息");

            QToolsHotkey = Config.Bind("config", "QToolsHotkey", KeyboardShortcut.Deserialize("BackQuote"), "Shortcut to open QTools window");

            Config.Save();

#endregion Configs

#region ResourceData

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
            {
                if (type.Namespace == null) continue;

                if (!type.Namespace.StartsWith("ProjectGenesis.Patches", StringComparison.Ordinal)) continue;

                Harmony.PatchAll(type);
            }

            TableID = new int[]
            {
                TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab1",
                                      new TabData("精炼页面".TranslateFromJsonSpecial(), "Assets/texpack/矿物处理")),
                TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                      new TabData("化工页面".TranslateFromJsonSpecial(), "Assets/texpack/化工科技")),
                TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab3",
                                      new TabData("防御页面".TranslateFromJsonSpecial(), "Assets/texpack/防御"))
            };

            RegisterStrings();
            ModifyVeinData();

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;

            LoadCompleted = true;
        }

        private Color GetItemColor(int itemId)
        {
            switch (itemId)
            {
                case 1117:
                    return new Color(153 / 255f, 163 / 255f, 87 / 255f, 1F);

                case 7101:
                case 1114:
                    return new Color(244 / 255f, 244 / 255f, 244 / 255f, 1F);

                case 7020:
                    return new Color(0.2275f, 0.3804f, 0.6431f, 1F);

                case 1014:
                    return new Color(0.8941f, 0.6000f, 1.0000f, 1F);

                case 1124:
                    return new Color(121 / 255f, 110 / 255f, 114 / 255f, 1F);

                case 1123:
                    return new Color(150 / 255f, 161 / 255f, 174 / 255f, 1F);

                case 6201:
                    return new Color(0.2118f, 0.2118f, 0.2118f, 1F);

                case 7708:
                    return new Color(0.8235f, 0.7451f, 0.5647f, 1F);

                case 7709:
                    return new Color(0.0824f, 0.0824f, 0.0824f, 1F);

                case 1000:
                    return new Color(165 / 255f, 198 / 255f, 208 / 255f, 1F);

                case 1003:
                    return new Color(0.2157f, 0.7020f, 0.4471f, 1F);

                case 1004:
                    return new Color(0.7647f, 0.8588f, 0.9765f, 1F);

                case 1005:
                    return new Color(0.5373f, 0.5686f, 0.6471f, 1F);

                case 1143:
                    return new Color(0.0824f, 0.6863f, 0.9804f, 1F);
            }

            if (IconDescUtils.IconDescs.TryGetValue(itemId, out var t))
                return t.Color;
            else
                return IconDescUtils.ExportIconDesc(itemId).faceColor;
        }

        private void MakeTexture()
        {
            Texture2D texture = new Texture2D(1024, 16);

            Graphics.CopyTexture(Resources.Load<Sprite>("Assets/texpack/chemical-plant-recipe-fcol").texture, 0, 0, 0, 0, 512, 16, texture, 0, 0, 0,
                                 0);

            Color[] colors = Enumerable.Repeat(new Color(165 / 255f, 198 / 255f, 208 / 255f, 255 / 255f), 16).ToArray();

            for (int i = 1; i < 1024; i++)
            {
                if (i == 32 || i == 35) continue;

                var proto = LDB.recipes.Select(i);

                if (proto == null)
                {
                    texture.SetPixels(i, 0, 1, 16, colors);
                    continue;
                }
                
                texture.SetPixels(i, 0, 1, 16, colors);

                if (proto.Type == ERecipeType.Chemical || proto.Type == (ERecipeType)16)
                {
                    Color output = GetItemColor(proto.Results[0]);

                    texture.SetPixel(i, 15-6, output);
                    texture.SetPixel(i, 15-9, output);
                    texture.SetPixel(i, 15-13, output);
                    texture.SetPixel(i, 15-14, output);
                    texture.SetPixel(i, 15-15, output);

                    switch (proto.Items.Length)
                    {
                        case 1:
                            output = GetItemColor(proto.Items[0]);
                            texture.SetPixel(i, 15-7, output);
                            texture.SetPixel(i, 15-8, output);
                            texture.SetPixel(i, 15-10, output);
                            texture.SetPixel(i, 15-11, output);
                            texture.SetPixel(i, 15-12, output);
                            continue;

                        case 2:
                            output = GetItemColor(proto.Items[0]);
                            texture.SetPixel(i, 15-7, output);
                            texture.SetPixel(i, 15-10, output);
                            texture.SetPixel(i, 15-11, output);

                            output = GetItemColor(proto.Items[1]);
                            texture.SetPixel(i, 15-8, output);
                            texture.SetPixel(i, 15-12, output);
                            continue;

                        case 3:
                            output = GetItemColor(proto.Items[0]);
                            texture.SetPixel(i, 15-7, output);
                            texture.SetPixel(i, 15-10, output);

                            output = GetItemColor(proto.Items[1]);
                            texture.SetPixel(i, 15-8, output);
                            texture.SetPixel(i, 15-11, output);

                            output = GetItemColor(proto.Items[2]);
                            texture.SetPixel(i, 15-12, output);
                            continue;
                    }
                }
                else
                {
                    texture.SetPixels(i, 0, 1, 16, colors);
                }
            }

            texture.Apply();
            
            ref var prefabDesc = ref LDB.models.Select(64).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture("_FluidTex", texture);
            prefabDesc.lodMaterials[1][1].SetTexture("_FluidTex", texture);

            prefabDesc = ref LDB.models.Select(376).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture("_FluidTex", texture);
            prefabDesc.lodMaterials[1][1].SetTexture("_FluidTex", texture);
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
            QuantumStoragePatches.Export(w);
        }

        public void Import(BinaryReader r)
        {
            MegaAssemblerPatches.Import(r);
            PlanetFocusPatches.Import(r);
            QuantumStoragePatches.Import(r);
        }

        public void IntoOtherSave()
        {
            MegaAssemblerPatches.IntoOtherSave();
            PlanetFocusPatches.IntoOtherSave();
            QuantumStoragePatches.IntoOtherSave();
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
            ModifyUpgradeTech();
        }

        private void PostAddDataAction()
        {
            //飞行舱拆除
            VegeProto vegeProto = LDB.veges.Select(9999);
            vegeProto.MiningItem = new[] { ProtoID.I四氢双环戊二烯燃料棒, ProtoID.I铁块, ProtoID.I铜块 };
            vegeProto.MiningCount = new[] { 3, 70, 40 };
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
            ModelPostFix();

            ProtoPreload();

            ModifyEnemyHpUpgrade();
            SetSkillSystem();
            SetMinerMk2Color();
            SetEffectEmitterProto();

            VFEffectEmitter.Init();

            ItemProto.InitFuelNeeds();
            ItemProto.InitTurretNeeds();
            ItemProto.InitFluids();
            ItemProto.InitTurrets();
            ItemProto.InitEnemyDropTables();
            ItemProto.InitItemIds();
            ItemProto.InitItemIndices();
            ItemProto.InitMechaMaterials();
            ItemProto.InitFighterIndices();
            ModelProto.InitMaxModelIndex();
            RecipeProto.InitFractionatorNeeds();
            RaycastLogic.LoadStatic();

            ItemProto.stationCollectorId = ProtoID.I轨道采集器;

            ItemPostFix();

            StorageComponent.staticLoaded = false;
            StorageComponent.LoadStatic();

            UIBuildMenu.staticLoaded = false;
            UIBuildMenu.StaticLoad();

            ref MechaMaterialSetting material = ref Configs.builtin.mechaArmorMaterials[21];
            material.itemId = ProtoID.I钨块;
            material.density = 19.35f;
            material.durability = 4.35f;

            // JsonHelper.ExportAsJson(@"D:\Git\ProjectGenesis\data");

            MakeTexture();
        }

        private static void ProtoPreload()
        {
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
        }

        internal static void SetConfig(bool currentLDBToolCache, bool currentHideTechMode, bool currentDisableMessageBox)
        {
            EnableLDBToolCacheEntry.Value = currentLDBToolCache;
            EnableHideTechModeEntry.Value = currentHideTechMode;
            DisableMessageBoxEntry.Value = currentDisableMessageBox;
            logger.LogInfo("SettingChanged");
            configFile.Save();
        }

        internal static void LogInfo(object data) => logger.LogInfo(data);
    }
}
