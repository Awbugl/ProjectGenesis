using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Compatibility
{
    internal static class GigaStationsUpdated
    {
        internal const string GUID = "org.kremnev8.plugin.GigaStationsUpdated";

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        private static bool _finished;

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out _)) return;

            MoveConflictRecipes();

            HarmonyPatch.Patch(AccessTools.Method(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded)),
                null,
                new HarmonyMethod(typeof(MoreMegaStructure), nameof(LDBToolOnPostAddDataAction))
                    { before = new[] { ProjectGenesis.MODGUID, }, });
        }

        private static void MoveConflictRecipes()
        {
            ref List<List<Proto>> preToAdd =
                ref AccessTools.StaticFieldRefAccess<List<List<Proto>>>(typeof(LDBTool), "PreToAdd");

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

        public static void LDBToolOnPostAddDataAction()
        {
            if (_finished) return;

            LDB.items.Select(2110).BuildIndex = 607;
            LDB.items.Select(2111).BuildIndex = 608;
            LDB.items.Select(2112).BuildIndex = 609;
            
            _finished = true;
        }
    }
}