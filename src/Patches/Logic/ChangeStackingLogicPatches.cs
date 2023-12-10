using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class ChangeStackingLogicPatches
    {
        private static readonly FieldInfo AssemblerComponent_RecipeType_FieldInfo
            = AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType));

        [HarmonyPatch(typeof(AssemblerComponent), "Import")]
        [HarmonyPostfix]
        public static void AssemblerComponent_Import_Postfix(ref AssemblerComponent __instance)
        {
            int recipeId = __instance.recipeId;
            RecipeProto proto = LDB.recipes.Select(recipeId);
            __instance.recipeSpecialStackingLogic = proto.SpecialStackingLogic;
        }

        [HarmonyPatch(typeof(AssemblerComponent), "SetRecipe")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_SetRecipe_Postfix(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AssemblerComponent_RecipeType_FieldInfo));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Ldfld,
                                                                    AccessTools.Field(typeof(RecipeProto), nameof(RecipeProto.SpecialStackingLogic))),
                                                new CodeInstruction(OpCodes.Stfld,
                                                                    AccessTools.Field(typeof(AssemblerComponent),
                                                                                      nameof(AssemblerComponent.recipeSpecialStackingLogic))));

            return matcher.InstructionEnumeration();
        }

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
                                                                                       nameof(AssemblerComponent_InsertMethod))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            // refine
            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo),
                                         new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ChangeStackingLogicPatches),
                                                                                       nameof(AssemblerComponent_InsertMethod))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod(ref AssemblerComponent component, int[] productRegister)
            => component.products.Length > 1 && component.recipeSpecialStackingLogic && CalcMaxProduct(ref component, productRegister, 19);

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
