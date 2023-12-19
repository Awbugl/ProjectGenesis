using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.UI
{
    public static class GeneralMissilePatches
    {
        [HarmonyPatch(typeof(GeneralMissile), "TickSkillLogic")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TickSkillLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GeneralMissile), nameof(GeneralMissile.modelIndex))),
                                 new CodeMatch(OpCodes.Ldc_I4, 431), new CodeMatch(OpCodes.Sub));

            matcher.Advance(-1).SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(GeneralMissilePatches), nameof(TickSkillLogic_Patch)))
                   .SetOpcodeAndAdvance(OpCodes.Nop);

            return matcher.InstructionEnumeration();
        }

        public static int TickSkillLogic_Patch(int modelIndex)
        {
            switch (modelIndex)
            {
                case ProtoIDUsedByPatches.M洲际导弹组:
                    return 5;

                case ProtoIDUsedByPatches.M反物质导弹组:
                    return 6;
            }

            return modelIndex - 431;
        }
    }
}
