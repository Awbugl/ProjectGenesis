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

            // assemble
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType))),
                                 new CodeMatch(OpCodes.Ldc_I4_4));

            var label = matcher.Advance(-1).Operand;

            matcher.Advance(5).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ProductionPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Assemble))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            // refine
            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                                         new CodeMatch(OpCodes.Ldfld,
                                                       AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType))),
                                         new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ProductionPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Refine))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod_Refine(ref AssemblerComponent component, int[] productRegister)
        {
            var b = GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T大气排污) &&
                    (component.recipeId == ProtoIDUsedByPatches.R焦油分馏 ||
                     component.recipeId == ProtoIDUsedByPatches.R原油裂化 ||
                     component.recipeId == ProtoIDUsedByPatches.R有机液体离心);

            if (b)
            {
                var counter = 0;

                var productsLength = component.products.Length;

                for (var index = 0; index < productsLength; ++index)
                {
                    if (component.produced[index] > component.productCounts[index] * 19) ++counter;
                }

                if (counter == productsLength) return false;

                for (var index = 0; index < productsLength; ++index)
                {
                    if (component.produced[index] > component.productCounts[index] * 19)
                    {
                        component.produced[index] = component.productCounts[index] * 19;

                        lock (productRegister)
                        {
                            productRegister[component.products[index]] -= component.productCounts[index];
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public static bool AssemblerComponent_InsertMethod_Assemble(ref AssemblerComponent component, int[] productRegister)
        {
            var b = GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T大气排污) && component.recipeId == ProtoIDUsedByPatches.R金属盐分解;

            if (b)
            {
                var counter = 0;

                var productsLength = component.products.Length;

                for (var index = 0; index < productsLength; ++index)
                {
                    if (component.produced[index] > component.productCounts[index] * 9) ++counter;
                }

                if (counter == productsLength) return false;

                for (var index = 0; index < productsLength; ++index)
                {
                    if (component.produced[index] > component.productCounts[index] * 9)
                    {
                        component.produced[index] = component.productCounts[index] * 9;

                        lock (productRegister)
                        {
                            productRegister[component.products[index]] -= component.productCounts[index];
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
