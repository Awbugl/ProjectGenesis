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

        private static ManualLogSource logger;

        //无限堆叠开关(私货)
        private readonly bool StackSizeButton = false;

        private int[] TableID;

        private static ProjectGenesis Instance { get; set; }
        
        private Harmony Harmony { get; set; }

        public string Version => VERSION;

        public void Awake()
        {
            Instance = this;
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
                                                new TabData("精炼页面".TranslateFromJson(), "Assets/texpack/主机科技")
                                               ),
                          TabSystem.RegisterTab("org.LoShin.GenesisBook:org.LoShin.GenesisBookTab2", 
                                                new TabData("化工页面".TranslateFromJson(), "Assets/texpack/化工科技")
                                               )
                      };

            NebulaModAPI.RegisterPackets(executingAssembly);

            Harmony = new Harmony(MODGUID);
            
            foreach (var type in executingAssembly.GetTypes()) Harmony.PatchAll(type);

            LDBTool.PreAddDataAction += PreAddDataAction;
            LDBTool.PostAddDataAction += PostAddDataAction;
        }

        private void PreAddDataAction()
        {
            LDB.strings.Select(2314).Name = "剧毒液体海洋";
            LDB.veins.Select(14).Name = "钨矿";
            LDB.milestones.Select(9).Name = "钨";
            LDB.milestones.Select(9).DefaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.";
            LDB.milestones.Select(9).defaultDesc = "你采集了钨矿,宇宙珍奇之一.它是一种用途广泛的新材料.".TranslateFromJson();
            LDB.items.OnAfterDeserialize();

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

            // JsonHelper.ExportAsJson(@"C:\Git\ProjectGenesis");
        }

        #region IModCanSave

        public void Export(BinaryWriter w) => MegaAssemblerPatches.Export(w);

        public void Import(BinaryReader r) => MegaAssemblerPatches.Import(r);

        public void IntoOtherSave() => MegaAssemblerPatches.IntoOtherSave();

        #endregion

        #region IMultiplayerMod

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

        public bool CheckVersion(string hostVersion, string clientVersion) => hostVersion.Equals(clientVersion);

        #endregion
    }
}
