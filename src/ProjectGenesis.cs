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
using ProjectGenesis.Compatibility.MoreMegaStructure;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.UI;
using ProjectGenesis.Utils;
using xiaoye97;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.PrefabFixUtils;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;
using static ProjectGenesis.Utils.PlanetThemeUtils;

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
    [BepInDependency(IncompatibleCheckPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem), nameof(AssemblerRecipeSystem))]
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerModWithSettings
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        public const string MODNAME = "GenesisBook";
        public const string VERSION = "2.4.7";

        public string Version => VERSION;

        internal static ManualLogSource logger;

        //无限堆叠开关(私货)
        private readonly bool StackSizeButton = false;

        private int[] TableID;
        private Harmony Harmony;

        internal static ConfigEntry<bool> EnableAtmosphericEmission;

        public void Awake()
        {
            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            if (IncompatibleCheckPlugin.GalacticScaleInstalled)
            {
                logger.Log(LogLevel.Error, "Galactic Scale is installed, which is incompatible with GenesisBook. Load Cancelled.");
                return;
            }

            if (IncompatibleCheckPlugin.DSPBattleInstalled)
            {
                logger.Log(LogLevel.Error, "They Come From Void is installed, which is incompatible with GenesisBook. Load Cancelled.");
                return;
            }

            var configdesc = "Add Atmospheric Emission tech in game\n是否添加大气排污科技，将其修改为 false 则游戏内不会出现此科技";

            EnableAtmosphericEmission = Config.Bind("config", "EnableAtmosphericEmission", true, configdesc);
            Config.Save();

            var executingAssembly = Assembly.GetExecutingAssembly();

            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", Path.GetDirectoryName(executingAssembly.Location));
            resources.LoadAssetBundle("texpack");
            ProtoRegistry.AddResource(resources);

            TableID = new int[]
                      {
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab1",
                                                new TabData("精炼页面".TranslateFromJson(), "Assets/texpack/主机科技")),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2",
                                                new TabData("化工页面".TranslateFromJson(), "Assets/texpack/化工科技"))
                      };

            NebulaModAPI.RegisterPackets(executingAssembly);

            NebulaModAPI.OnPlanetLoadRequest += planetId =>
            {
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new MegaBuildingLoadRequest(planetId));
            };

            NebulaModAPI.OnPlanetLoadFinished += MegaBuildingDataProcessor.ProcessBytesLater;

            Harmony = new Harmony(MODGUID);

            foreach (var type in executingAssembly.GetTypes())
            {
                if (type == typeof(MoreMegaStructureEditDataPatches) || type == typeof(UIMainMenuPatches)) continue;
                Harmony.PatchAll(type);
            }

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;
        }

        private void PreAddDataAction()
        {
            LDB.strings.Select(2314).name = "剧毒液体海洋";
            LDB.strings.Select(2314).Name = "剧毒液体海洋".TranslateFromJson();
            LDB.veins.Select(14).name = "钨矿";
            LDB.veins.Select(14).Name = "钨矿".TranslateFromJson();

            LDB.items.OnAfterDeserialize();

            AdjustPlanetThemeDataVanilla();
            AddCopiedModelProto();
            ImportJson(TableID, EnableAtmosphericEmission.Value);
        }

        private void PostAddDataAction()
        {
            LDB.strings.OnAfterDeserialize();
            LDB.items.OnAfterDeserialize();
            LDB.recipes.OnAfterDeserialize();
            LDB.techs.OnAfterDeserialize();
            LDB.models.OnAfterDeserialize();
            LDB.veins.OnAfterDeserialize();

            GameMain.gpuiManager.Init();

            LDB.milestones.Select(9).Preload();

            foreach (var theme in LDB.themes.dataArray) theme.Preload();

            LDB.themes.Select(12).oceanMat = LDB.themes.Select(22).oceanMat;

            //飞行舱拆除
            var @base = LDB.veges.Select(9999);
            @base.MiningItem = new[] { 1801, 1101, 1104 };
            @base.MiningCount = new[] { 3, 10, 20 };
            @base.MiningChance = new float[] { 1, 1, 1 };
            @base.Preload();

            ref var locstrs = ref AccessTools.StaticFieldRefAccess<StringProtoSet>(AccessTools.Field(typeof(Localization), "_strings"))();
            locstrs = LDB.strings;

            foreach (var proto in LDB.veins.dataArray)
            {
                proto.Preload();
                proto.name = proto.Name.TranslateFromJson();
            }

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
            SetBuildBar();

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
                if (StackSizeButton) proto.StackSize = 10000000;
                StorageComponent.itemStackCount[proto.ID] = proto.StackSize;
            }

            // JsonHelper.ExportAsJson(@"D:\Git\ProjectGenesis\dependencies");
        }

        public void Export(BinaryWriter w) => MegaAssemblerPatches.Export(w);

        public void Import(BinaryReader r) => MegaAssemblerPatches.Import(r);

        public void IntoOtherSave() => MegaAssemblerPatches.IntoOtherSave();

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);
    }
}
