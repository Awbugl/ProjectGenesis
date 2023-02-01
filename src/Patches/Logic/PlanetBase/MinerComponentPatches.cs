using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.PlanetBase
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        [HarmonyPrefix]
        public static void MinerComponent_InternalUpdate_PreFix(PlanetFactory factory, ref float miningSpeed)
        {
            if (ContainsFocus(factory.planetId, 6528)) miningSpeed *= 1.25f;
        }

        [HarmonyPatch(typeof(UIMinerWindow), "_OnUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIMinerWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(false,
                                     new CodeMatch(OpCodes.Ldfld,
                                                   AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.miningSpeedScale))));

                if (matcher.IsInvalid) break;

                matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                    new CodeInstruction(OpCodes.Ldfld,
                                                                        AccessTools.Field(typeof(UIMinerWindow), nameof(UIMinerWindow.factory))));

                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(MiningSpeedScale))));
            }

            return matcher.InstructionEnumeration();
        }

        public static float MiningSpeedScale(float scale, PlanetFactory factory)
        {
            var exist = ContainsFocus(factory.planetId, 6528);
            return exist ? scale * 1.25f : scale;
        }
    }
}
