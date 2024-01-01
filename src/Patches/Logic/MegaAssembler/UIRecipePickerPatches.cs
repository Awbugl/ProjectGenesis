using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker.RefreshIcons))]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIRecipePicker_RefreshIcons(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(
                true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeProto), nameof(RecipeProto.Type))), new CodeMatch(OpCodes.Bne_Un));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MegaAssemblerPatches), nameof(ContainsRecipeType))));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }
    }
}
