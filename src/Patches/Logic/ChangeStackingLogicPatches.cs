using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class ChangeStackingLogicPatches
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

            // chemical
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                 new CodeMatch(OpCodes.Ldc_I4_2));

            object label = matcher.Advance(-1).Operand;
            matcher.Advance(1);

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ChangeStackingLogicPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Chemical))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            // refine
            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                         new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ChangeStackingLogicPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod_Refine))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod_Refine(ref AssemblerComponent component, int[] productRegister)
        {
            if (component.products.Length < 2) return false;

            bool b = false;

            switch (component.recipeId)
            {
                case ProtoIDUsedByPatches.R等离子精炼:
                    b = true;
                    break;
            }

            return b && CalcMaxProduct(ref component, productRegister, 19);
        }

        public static bool AssemblerComponent_InsertMethod_Chemical(ref AssemblerComponent component, int[] productRegister)
        {
            if (component.products.Length < 2) return false;

            bool b = false;

            switch (component.recipeId)
            {
                case ProtoIDUsedByPatches.R氢氯酸:
                case ProtoIDUsedByPatches.R海水淡化:
                case ProtoIDUsedByPatches.R羰基合成:
                case ProtoIDUsedByPatches.R氨氧化:
                case ProtoIDUsedByPatches.R三氯化铁:
                case ProtoIDUsedByPatches.R四氢双环戊二烯:
                case ProtoIDUsedByPatches.R高效石墨烯:
                case ProtoIDUsedByPatches.R水电解:
                    b = true;
                    break;
            }

            return b && CalcMaxProduct(ref component, productRegister, 19);
        }

        private static bool CalcMaxProduct(ref AssemblerComponent component, int[] productRegister, int maxproduct)
        {
            int counter = 0;

            int productsLength = component.products.Length;

            for (int index = 0; index < productsLength; ++index)
            {
                if (component.produced[index] > component.productCounts[index] * maxproduct) ++counter;
            }

            if (counter == productsLength) return false;

            for (int index = 0; index < productsLength; ++index)
            {
                int productCount = component.productCounts[index];
                int componentProductCount = productCount * maxproduct;

                ref int intPtr = ref component.produced[index];

                if (intPtr > componentProductCount)
                {
                    intPtr = componentProductCount;

                    lock (productRegister)
                    {
                        productRegister[component.products[index]] -= productCount;
                    }
                }
            }

            return true;
        }
    }
}
