using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Compatibility
{
    internal static class GigaStationsUpdated
    {
        internal const string GUID = "org.kremnev8.plugin.GigaStationsUpdated";

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo)) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;

            var type = assembly.GetType("GigaStations.GigaStationsPlugin");

            HarmonyPatch.Patch(AccessTools.Method(type,
                    "AddGigaCollector"), null, null,
                new HarmonyMethod(typeof(GigaStationsUpdated), nameof(AddGigaCollector_Transpiler)));


            ref List<List<Proto>> preToAdd =
                ref AccessTools.StaticFieldRefAccess<List<List<Proto>>>(typeof(LDBTool), "PreToAdd");

            MoveConflictRecipes(ref preToAdd);
            MoveBuildIndex(ref preToAdd);
        }

        private static void MoveConflictRecipes(ref List<List<Proto>> preToAdd)
        {
            int index = ProtoIndex.GetIndex(typeof(RecipeProto));

            foreach (var proto in preToAdd[index].Cast<RecipeProto>().Where(proto => proto != null))
            {
                switch (proto.ID)
                {
                    case 410:
                        proto.ID = 430;
                        proto.Type = (ERecipeType)Utils.ERecipeType.标准制造;
                        break;

                    case 411:
                        proto.ID = 431;
                        proto.Type = (ERecipeType)Utils.ERecipeType.标准制造;
                        break;

                    case 412:
                        proto.ID = 432;
                        proto.Type = (ERecipeType)Utils.ERecipeType.标准制造;
                        break;
                }
            }
        }

        private static void MoveBuildIndex(ref List<List<Proto>> preToAdd)
        {
            int index = ProtoIndex.GetIndex(typeof(ItemProto));

            foreach (var proto in preToAdd[index].Cast<ItemProto>().Where(proto => proto != null))
            {
                switch (proto.ID)
                {
                    case 2110:
                        proto.BuildIndex = 1207;
                        break;

                    case 2111:
                        proto.BuildIndex = 1208;
                        break;

                    case 2112:
                        proto.BuildIndex = 1209;
                        proto.DescFields = new[] { 18, 32, 1, 40 };
                        break;
                }
            }
        }

        public static IEnumerable<CodeInstruction> AddGigaCollector_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld,
                    AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.workEnergyPerTick))));

            matcher.SetAndAdvance(OpCodes.Call,
                AccessTools.Method(typeof(GigaStationsUpdated), nameof(SetWorkEnergyPerTick)));

            return matcher.InstructionEnumeration();
        }

        public static void SetWorkEnergyPerTick(PrefabDesc desc, long workEnergyPerTick)
        {
            desc.workEnergyPerTick = 0;
        }
    }
}