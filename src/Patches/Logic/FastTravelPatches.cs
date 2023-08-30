using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class FastTravelPatches
    {
        [HarmonyPatch(typeof(UIStarmap), "UpdateCursorView")]
        [HarmonyPatch(typeof(UIStarmap), "StartFastTravelToUPosition")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarmap_UpdateCursorView_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameMain), "sandboxToolsEnabled")));
            matcher.Advance(1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(FastTravelPatches), nameof(IsFastTravelTechUnlocked))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarmap), "OnFastTravelButtonClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarmap_FastTravel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameMain), "sandboxToolsEnabled")));

            matcher.Advance(1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FastTravelPatches), nameof(IsFastTravelEnabled))));

            return matcher.InstructionEnumeration();
        }

        public static bool IsFastTravelTechUnlocked(bool sandboxToolsEnabled)
            => sandboxToolsEnabled || GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T虫洞航行);

        public static bool IsFastTravelEnabled(bool sandboxToolsEnabled)
        {
            if (sandboxToolsEnabled) return true;

            if (!GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T虫洞航行))
            {
                UIRealtimeTip.Popup("未解锁虫洞航行".TranslateFromJson());
                return false;
            }

            Mecha mecha = GameMain.mainPlayer.mecha;

            long energyWant = 12000000000;

            mecha.QueryEnergy(energyWant, out double energyGet, out _);

            if (energyGet < energyWant)
            {
                UIRealtimeTip.Popup("虫洞能量不足".TranslateFromJson());
                return false;
            }

            if (!mecha.UseWarper())
            {
                UIRealtimeTip.PopupAhead("空间翘曲器不足".Translate());
                return false;
            }

            mecha.coreEnergy -= energyGet;
            mecha.MarkEnergyChange(9, -energyWant);

            return true;
        }
    }
}
