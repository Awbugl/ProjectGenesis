using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class MechaReplicatorSpeedPatches
    {
        [HarmonyPatch(typeof(MechaForge), nameof(MechaForge.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MechaForge_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // ReplicatorSpeedBoost check
            
            /*
                if (this.mecha.reactorItemId == 5206)

                IL_0030: ldarg.0      // this
                IL_0031: call         instance class Mecha MechaForge::get_mecha()
                IL_0036: ldfld        int32 Mecha::reactorItemId
                IL_003b: ldc.i4       5206 // 0x00001456
                IL_0040: bne.un.s     IL_0083
             */

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Mecha), nameof(Mecha.reactorItemId))),
                new CodeMatch(OpCodes.Ldc_I4, 5206));

            var label = matcher.Operand;

            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(MechaReplicatorSpeedPatches), nameof(CheckReactorItemId)))
               .SetAndAdvance(OpCodes.Brfalse_S, label);

            return matcher.InstructionEnumeration();
        }

        public static bool CheckReactorItemId(int reactorItemId)
        {
            // frags or books or logs
            return reactorItemId == 5206 || (reactorItemId > 6506 && reactorItemId < 6508)
                                         || (reactorItemId > 6511 && reactorItemId < 6521);
        }
    }
}
