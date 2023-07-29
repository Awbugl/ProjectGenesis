using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class AtmosphericEmissionPatches
    {
        private static readonly FieldInfo AssemblerComponent_RecipeType_FieldInfo
            = AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType));

        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_InternalUpdate_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            // assemble
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                 new CodeMatch(OpCodes.Ldc_I4_4));

            var label = matcher.Advance(-1).Operand;

            // refine
            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                         new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(AtmosphericEmissionPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Refine))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            // chemical
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                 new CodeMatch(OpCodes.Ldc_I4_2));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(AtmosphericEmissionPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Chemical))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod_Refine(ref AssemblerComponent component, int[] productRegister)
        {
            var componentRecipeId = component.recipeId;

            var b = componentRecipeId == ProtoIDUsedByPatches.R有机液体离心 && ProjectGenesis.AtmosphericEmissionValue;

            return b && CalcMaxProduct(ref component, productRegister, 19);
        }

        public static bool AssemblerComponent_InsertMethod_Chemical(ref AssemblerComponent component, int[] productRegister)
        {
            var b = false;

            switch (component.recipeId)
            {
                case ProtoIDUsedByPatches.R氢氯酸:
                case ProtoIDUsedByPatches.R硫酸:
                case ProtoIDUsedByPatches.R海水淡化:
                case ProtoIDUsedByPatches.R水电解:
                    b = true;
                    break;
            }

            return b && ProjectGenesis.AtmosphericEmissionValue && CalcMaxProduct(ref component, productRegister, 19);
        }

        private static bool CalcMaxProduct(ref AssemblerComponent component, int[] productRegister, int maxproduct)
        {
            var counter = 0;

            var productsLength = component.products.Length;

            for (var index = 0; index < productsLength; ++index)
            {
                if (component.produced[index] > component.productCounts[index] * maxproduct) ++counter;
            }

            if (counter == productsLength) return false;

            for (var index = 0; index < productsLength; ++index)
            {
                var productCount = component.productCounts[index];
                var componentProductCount = productCount * maxproduct;

                ref var intPtr = ref component.produced[index];

                if (intPtr > componentProductCount)
                {
                    intPtr = componentProductCount;

                    lock (productRegister)
                    {
                        productRegister[intPtr] -= productCount;
                    }
                }
            }

            return true;
        }
    }
}
