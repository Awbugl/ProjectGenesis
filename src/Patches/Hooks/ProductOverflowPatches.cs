using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
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

            /*
                IL_034d: ldarg.0      // component
                IL_034e: ldc.i4.0
                IL_034f: stfld        int32 AssemblerComponent::extraSpeed
             */

            // get br point
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.extraSpeed))));

            object label = matcher.Labels.First();

            matcher.Start().MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AssemblerComponent_RecipeType_FieldInfo), new CodeMatch(OpCodes.Ldc_I4_3));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ProductOverflowPatches), nameof(AssemblerComponent_InsertMethod))),
                new CodeInstruction(OpCodes.Brtrue_S, label), new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ret));

            return matcher.InstructionEnumeration();
        }

        public static bool AssemblerComponent_InsertMethod(ref AssemblerComponent component, int[] productRegister)
        {
            RecipeExecuteData executeData = component.recipeExecuteData;

            int productsLength = executeData.products.Length;
            int[] products = executeData.products;
            int[] productCounts = executeData.productCounts;
            int[] produced = component.produced;

            int maxProducts;

            switch (component.recipeType)
            {
                case ERecipeType.Smelt:
                case (ERecipeType)Utils_ERecipeType.标准冶炼:
                case (ERecipeType)Utils_ERecipeType.高热冶炼:
                case (ERecipeType)Utils_ERecipeType.所有熔炉:
                {
                    maxProducts = 100;
                    break;
                }

                default:
                {
                    maxProducts = 20;
                    break;
                }
            }

            // 获取配方的溢出标志
            var recipeOverflowFlag = (EOverflowFlag)(LDB.recipes.Select(component.recipeId)?.Overflow ?? 0);

            // 判断是否启用溢出功能
            bool overflowEnabled = (ProjectGenesis.ProductOverflowEntry.Value == 0 && recipeOverflowFlag != EOverflowFlag.None)
                                || ProjectGenesis.ProductOverflowEntry.Value == 1;

            if (overflowEnabled)
            {
                int overflowCounter = 0; // 用于统计本次有多少个产物处于“已溢出”状态

                // --- 第一步：检查与计数 ---
                for (int index = 0; index < productsLength; ++index)
                {
                    bool canOverflow = IsProductAllowedToOverflow(index, recipeOverflowFlag);
                    int limit = productCounts[index] * maxProducts;

                    if (produced[index] >= limit)
                    {
                        // 不允许溢出但满了 -> 直接停止生产
                        if (!canOverflow) return false;

                        // 允许溢出且满了 -> 计入“已溢出”计数器
                        overflowCounter++;
                    }
                }

                // --- 关键安全阀：如果所有产物都溢出了，停止生产 ---
                // 这防止了当所有输出仓库都满时，机器还在空转消耗电力/原料却不产出有效物品
                if (overflowCounter == productsLength) { return false; }

                // --- 第二步：执行生产逻辑 ---
                for (int index = 0; index < productsLength; ++index)
                {
                    int productCount = productCounts[index];
                    int componentProductCount = productCount * maxProducts;
                    bool canOverflow = IsProductAllowedToOverflow(index, recipeOverflowFlag);

                    // 如果允许溢出 且 当前已经超出上限
                    if (canOverflow && produced[index] >= componentProductCount)
                    {
                        // 重置为上限值（模拟丢弃多余部分，保持本地计数饱和）
                        // 注意：这里不增加 productRegister，意味着多余的产物消失了
                        produced[index] = componentProductCount;
                    }
                    else
                    {
                        // 正常生产：增加本地计数和全局寄存器
                        produced[index] += productCount;
                        lock (productRegister) productRegister[products[index]] += productCount;
                    }
                }

                return true;
            }

            // --- 非溢出状态：保持原始逻辑 ---
            {
                for (var index = 0; index < productsLength; ++index)
                    if (produced[index] > productCounts[index] * maxProducts)
                        return false;

                for (int index = 0; index < productsLength; ++index)
                {
                    produced[index] += productCounts[index];
                    lock (productRegister) productRegister[products[index]] += productCounts[index];
                }

                return true;
            }
        }

        private static bool IsProductAllowedToOverflow(int productIndex, EOverflowFlag overflowFlag)
        {
            if ((overflowFlag & EOverflowFlag.All) == EOverflowFlag.All) return true;

            switch (productIndex)
            {
                case 0: return (overflowFlag & EOverflowFlag.FirstProduct) == EOverflowFlag.FirstProduct;
                case 1: return (overflowFlag & EOverflowFlag.SecondProduct) == EOverflowFlag.SecondProduct;
                case 2: return (overflowFlag & EOverflowFlag.ThirdProduct) == EOverflowFlag.ThirdProduct;
                case 3: return (overflowFlag & EOverflowFlag.FourthProduct) == EOverflowFlag.FourthProduct;
                case 4: return (overflowFlag & EOverflowFlag.FifthProduct) == EOverflowFlag.FifthProduct;
                default: return false;
            }
        }
    }
}
