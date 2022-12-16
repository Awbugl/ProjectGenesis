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
using ProjectGenesis.Utils;
using xiaoye97;
using ERecipeType_1 = ERecipeType;
using static ProjectGenesis.Utils.PrefabFixUtils;
using static ProjectGenesis.Utils.JsonDataUtils;
using static ProjectGenesis.Utils.CopyModelUtils;

#pragma warning disable CS0618

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace ProjectGenesis
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem), nameof(TabSystem), nameof(AssemblerRecipeSystem))]

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBeInternal
    public class ProjectGenesis : BaseUnityPlugin, IModCanSave, IMultiplayerModWithSettings
    {
        public const string MODGUID = "org.LoShin.GenesisBook";
        internal const string MODNAME = "GenesisBook";
        internal const string VERSION = "2.2.8";

        // ReSharper disable once MemberCanBePrivate.Global
        internal static ManualLogSource logger;

        //无限堆叠开关(私货)
        private readonly bool StackSizeButton = false;

        private int[] TableID;

        private Harmony Harmony { get; set; }

        public string Version => VERSION;

        public void Awake()
        {
            logger = Logger;
            logger.Log(LogLevel.Info, "GenesisBook Awake");

            var executingAssembly = Assembly.GetExecutingAssembly();
            var pluginfolder = Path.GetDirectoryName(executingAssembly.Location);
            var resources = new ResourceData("org.LoShin.GenesisBook", "texpack", pluginfolder);
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

            foreach (var type in executingAssembly.GetTypes()) Harmony.PatchAll(type);

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

            LDB.themes.Select(8).WaterItemId = 7018;
            LDB.themes.Select(12).WaterItemId = 7017;
            LDB.themes.Select(12).WaterHeight = LDB.themes.Select(22).WaterHeight + 0.6f;

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var theme in LDB.themes.dataArray)
            {
                if (theme.ID == 1)
                {
                    theme.RareVeins = new[] { 8 };
                    theme.RareSettings = new float[] { 1.0f, 0.5f, 0.0f, 0.4f };
                }

                if (theme.VeinSpot.Length > 2)
                {
                    ref var silicon = ref theme.VeinSpot[2];

                    if (silicon > 0)
                    {
                        silicon = 1 + silicon / 4;
                        theme.VeinCount[2] = 0.5f;
                        theme.VeinOpacity[2] = 0.5f;
                    }
                }
            }

            AddCopiedModelProto();
            ImportJson(TableID);
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

            LDBTool.SetBuildBar(1, 10, 6261);
            LDBTool.SetBuildBar(3, 10, 2313);
            LDBTool.SetBuildBar(4, 4, 6229);
            LDBTool.SetBuildBar(5, 3, 6230);
            LDBTool.SetBuildBar(5, 4, 2303);
            LDBTool.SetBuildBar(5, 5, 2304);
            LDBTool.SetBuildBar(5, 6, 2305);
            LDBTool.SetBuildBar(5, 7, 2308);
            LDBTool.SetBuildBar(5, 8, 2309);
            LDBTool.SetBuildBar(5, 9, 2317);
            LDBTool.SetBuildBar(5, 10, 2310);
            LDBTool.SetBuildBar(7, 3, 6257);
            LDBTool.SetBuildBar(7, 4, 6258);
            LDBTool.SetBuildBar(7, 5, 6259);
            LDBTool.SetBuildBar(7, 6, 6260);
            LDBTool.SetBuildBar(7, 7, 6264);
            LDBTool.SetBuildBar(7, 8, 6265);
            LDBTool.SetBuildBar(7, 9, 6275);
            LDBTool.SetBuildBar(7, 10, 6276);
            LDBTool.SetBuildBar(2, 5, 6266);

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

        #region IModCanSave

        public void Export(BinaryWriter w) => MegaAssemblerPatches.Export(w);

        public void Import(BinaryReader r) => MegaAssemblerPatches.Import(r);

        public void IntoOtherSave() => MegaAssemblerPatches.IntoOtherSave();

        #endregion

        #region IMultiplayerMod

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);

        #endregion
    }
}
