using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class GoalLogicPatches
    {
        [HarmonyPatch(typeof(GoalLogic), nameof(GoalLogic.CheckLogic))]
        [HarmonyPostfix]
        public static void GoalLogic_CheckLogic_Postfix(GoalLogic __instance, int protoId, ref bool __result)
        {
            GoalProto goalProto = LDB.goals.Select(protoId);

            if (goalProto == null) return;

            if (goalProto.ParentId == -1)
            {
                __result = false;
                return;
            }

            switch (protoId)
            {
                case ProtoID.G蓝图:
                    if (__instance.TechUnlockedWithDelay(ProtoID.T等离子萃取精炼, 0, (long)(goalProto.GetDisplayParam(0) * 60.0))) __result = true;
                    break;

                case ProtoID.G增产剂:
                    if (__instance.TechUnlockedWithDelay(1203, 0, (long)(goalProto.GetDisplayParam(0) * 60.0))) __result = true;
                    break;


                case ProtoID.G扩建戴森球:
                    if (__instance.TechUnlockedWithDelay(1507, 0, (long)(goalProto.GetDisplayParam(0) * 3600.0))
                     && GoalTools.GeneratorCheck(__instance.gameData, 12))
                        __result = true;
                    break;
            }
        }
    }
}
