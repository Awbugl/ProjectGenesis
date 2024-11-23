using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class ProductOverflowPatches
    {
        private static readonly FieldInfo AssemblerComponent_RecipeType_FieldInfo =
            AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType));

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.InternalUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            // chemical
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo), new CodeMatch(OpCodes.Ldc_I4_2));

            object label = matcher.Advance(-1).Operand;
            matcher.Advance(1);

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ProductOverflowPatches), nameof(AssemblerComponent_InsertMethod_Chemical))),
                new CodeInstruction(OpCodes.Brtrue_S, label));

            // refine
            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo), new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ProductOverflowPatches), nameof(AssemblerComponent_InsertMethod_Refine))),
                new CodeInstruction(OpCodes.Brtrue_S, label));

            // assemble
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo), new CodeMatch(OpCodes.Ldc_I4_4));
            matcher.Advance(4);

            // other recipe
            matcher.Advance(6).MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Stloc_S));
            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ProductOverflowPatches), nameof(AssemblerComponent_InsertMethod_Other))),
                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod_Refine(ref AssemblerComponent component, int[] productRegister)
        {
            if (component.products.Length < 2) return false;

            switch (ProjectGenesis.ProductOverflowEntry.Value)
            {
                case 0:
                    var b = false;

                    switch (component.recipeId)
                    {
                        case ProtoID.R等离子精炼:
                            b = true;

                            break;
                    }

                    return b && CalcMaxProduct(ref component, productRegister, 19);

                case 1: return CalcMaxProduct(ref component, productRegister, 19);

                default: return false;
            }
        }

        public static bool AssemblerComponent_InsertMethod_Chemical(ref AssemblerComponent component, int[] productRegister)
        {
            if (component.products.Length < 2) return false;

            switch (ProjectGenesis.ProductOverflowEntry.Value)
            {
                case 0:
                    var b = false;

                    switch (component.recipeId)
                    {
                        case ProtoID.R盐水电解:
                        case ProtoID.R海水淡化:
                        case ProtoID.R高效石墨烯:
                        case ProtoID.R水电解:
                        case ProtoID.R二氧化硫还原:
                            b = true;

                            break;
                    }

                    return b && CalcMaxProduct(ref component, productRegister, 19);

                case 1: return CalcMaxProduct(ref component, productRegister, 19);

                default: return false;
            }
        }

        public static bool AssemblerComponent_InsertMethod_Other(ref AssemblerComponent component, int[] productRegister)
        {
            if (component.products.Length < 2) return false;

            switch (ProjectGenesis.ProductOverflowEntry.Value)
            {
                case 0:
                    var b = false;

                    switch (component.recipeId)
                    {
                        case ProtoID.R放射性矿物处理:
                            b = true;

                            break;
                    }

                    return b && CalcMaxProduct(ref component, productRegister, 19);

                case 1: return CalcMaxProduct(ref component, productRegister, 19);

                default: return false;
            }
        }

        private static bool CalcMaxProduct(ref AssemblerComponent component, int[] productRegister, int maxproduct)
        {
            var counter = 0;

            int productsLength = component.products.Length;

            for (var index = 0; index < productsLength; ++index)
            {
                if (component.produced[index] > component.productCounts[index] * maxproduct) ++counter;
            }

            if (counter == productsLength) return false;

            for (var index = 0; index < productsLength; ++index)
            {
                int productCount = component.productCounts[index];
                int componentProductCount = productCount * maxproduct;

                ref int intPtr = ref component.produced[index];

                if (intPtr > componentProductCount)
                {
                    intPtr = componentProductCount;

                    lock (productRegister) productRegister[component.products[index]] -= productCount;
                }
            }

            return true;
        }
    }
}
