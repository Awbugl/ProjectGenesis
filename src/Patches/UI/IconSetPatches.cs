using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class IconSetPatches
    {
        [HarmonyPatch(typeof(IconSet), nameof(IconSet.Create))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IconSet_Create_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(TechProto), nameof(TechProto.iconSprite))));

            object label = matcher.InstructionAt(5).operand;

            object index_V_23 = matcher.Advance(-2).Operand;
            object dataArray3 = matcher.Advance(-1).Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, dataArray3), new CodeInstruction(OpCodes.Ldloc_S, index_V_23),
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IconSetPatches), nameof(IconSet_Create_Patch))),
                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool IconSet_Create_Patch(TechProto proto) => proto.ID < 2000;
    }
}
