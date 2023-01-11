using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class ProductionPatches
    {
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_InternalUpdate_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType))),
                                 new CodeMatch(OpCodes.Ldc_I4_4));

            var label = matcher.Advance(15).Operand;
            var index = matcher.Advance(-4).Operand;

            matcher.Advance(5).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(OpCodes.Ldloc_S, index),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ProductionPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod))),
                                                new CodeInstruction(OpCodes.Brtrue, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod(ref AssemblerComponent component, int[] productRegister, int index)
        {
            var b = component.recipeId == 801;
            if (b) component.produced[index] = component.productCounts[index] * 9;
            
            lock (productRegister)
            {
                productRegister[component.products[index]] -= component.productCounts[index];
            }

            return b;
        }
    }
}
