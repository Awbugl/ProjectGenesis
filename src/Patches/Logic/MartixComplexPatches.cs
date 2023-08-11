using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class MartixComplexPatches
    {
        private static readonly FieldInfo LabComponent_MatrixIncServed_FieldInfo
            = AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixIncServed));

        [HarmonyPatch(typeof(PlanetFactory), "EntityFastFillIn")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_EntityFastFillIn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelema),
                                 new CodeMatch(OpCodes.Ldfld, LabComponent_MatrixIncServed_FieldInfo), new CodeMatch(OpCodes.Stloc_S));

            List<CodeInstruction> ins = matcher.InstructionsWithOffsets(-4, -2);

            matcher.Advance(1).InsertAndAdvance(ins).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_0),
                                                                      new CodeInstruction(OpCodes.Ldfld,
                                                                                          AccessTools.Field(typeof(PlanetFactory), "_tmp_items")),
                                                                      new CodeInstruction(OpCodes.Ldarg_2),
                                                                      new CodeInstruction(OpCodes.Call,
                                                                                          AccessTools.Method(typeof(MartixComplexPatches),
                                                                                                             nameof(
                                                                                                                 MartixComplex_EntityFastFillIn))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetFactory), "InsertInto")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_InsertInto_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_S), new CodeMatch(OpCodes.Ldarg_S), new CodeMatch(OpCodes.Stind_I1));

            var remainInc = matcher.Operand;
            var itemInc = matcher.Advance(1).Operand;

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldarg_2), new CodeMatch(OpCodes.Ldarg_3),
                                 new CodeMatch(OpCodes.Ldarg_S), new CodeMatch(OpCodes.Ldarg_S, itemInc));

            var itemCount = matcher.Advance(-1).Operand;

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), "factorySystem")),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), "labPool")), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldelema), new CodeMatch(OpCodes.Ldfld, LabComponent_MatrixIncServed_FieldInfo),
                                 new CodeMatch(OpCodes.Stloc_S));

            var matcher2 = matcher.Clone();
            matcher2.MatchForward(true, new CodeMatch(OpCodes.Ldarg_S), new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Leave));
            var leavelabel = matcher2.Operand;
            var stitemCount = matcher2.Advance(-1).Operand;

            List<CodeInstruction> ins = matcher.InstructionsWithOffsets(-6, -2);
            matcher.Advance(1).CreateLabel(out var orilabel).Insert(new CodeInstruction(OpCodes.Pop)).CreateLabel(out var label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, itemCount), new CodeInstruction(OpCodes.Ldc_I4_0),
                                     new CodeInstruction(OpCodes.Beq_S, orilabel));

            matcher.InsertAndAdvance(ins).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_3),
                                                           new CodeInstruction(OpCodes.Ldarg_S, itemCount),
                                                           new CodeInstruction(OpCodes.Ldarg_S, itemInc),
                                                           new CodeInstruction(OpCodes.Ldarg_S, remainInc),
                                                           new CodeInstruction(OpCodes.Call,
                                                                               AccessTools.Method(typeof(MartixComplexPatches),
                                                                                                  nameof(MartixComplex_Insert))),
                                                           new CodeInstruction(OpCodes.Dup), new CodeInstruction(OpCodes.Brfalse, label),
                                                           new CodeInstruction(OpCodes.Stloc_S, stitemCount),
                                                           new CodeInstruction(OpCodes.Leave, leavelabel));

            return matcher.InstructionEnumeration();
        }

        public static void MartixComplex_EntityFastFillIn(
            ref LabComponent component,
            PlanetFactory factory,
            ItemBundle tmpItems,
            bool fromPackage)
        {
            var factoryStatPool = GameMain.statistics.production.factoryStatPool[factory.index];

            var itemId = ProtoIDUsedByPatches.I稳定矩阵;
            var itemCount = 100 - Math.Max(component.matrixServed[0], component.matrixServed[1]) / 3600;
            var itemInc = 0;

            if (itemCount > 0) factory.gameData.mainPlayer.TakeItemFromPlayer(ref itemId, ref itemCount, out itemInc, fromPackage, tmpItems);

            if (itemCount > 0)
            {
                factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I稳定矩阵] += itemCount;
                factoryStatPool.productRegister[ProtoIDUsedByPatches.I电磁矩阵] += itemCount;
                factoryStatPool.productRegister[ProtoIDUsedByPatches.I能量矩阵] += itemCount;

                component.matrixServed[0] += itemCount * 3600;
                component.matrixIncServed[0] += itemInc * 1800;
                component.matrixServed[1] += itemCount * 3600;
                component.matrixIncServed[1] += itemInc * 1800;
            }

            itemId = ProtoIDUsedByPatches.I虚空矩阵;
            itemCount = 100 - Math.Max(component.matrixServed[2], component.matrixServed[3]) / 3600;
            itemInc = 0;
            if (itemCount > 0) factory.gameData.mainPlayer.TakeItemFromPlayer(ref itemId, ref itemCount, out itemInc, fromPackage, tmpItems);

            if (itemCount > 0)
            {
                factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I虚空矩阵] += itemCount;
                factoryStatPool.productRegister[ProtoIDUsedByPatches.I结构矩阵] += itemCount;
                factoryStatPool.productRegister[ProtoIDUsedByPatches.I信息矩阵] += itemCount;

                component.matrixServed[2] += itemCount * 3600;
                component.matrixIncServed[2] += itemInc * 1800;
                component.matrixServed[3] += itemCount * 3600;
                component.matrixIncServed[3] += itemInc * 1800;
            }

            var max = 0;

            for (var index = 0; index < 5; ++index) max = Math.Max(max, component.matrixServed[index]);

            itemId = ProtoIDUsedByPatches.I宇宙矩阵粗坯;
            itemCount = 100 - max / 3600;
            itemInc = 0;
            if (itemCount > 0) factory.gameData.mainPlayer.TakeItemFromPlayer(ref itemId, ref itemCount, out itemInc, fromPackage, tmpItems);

            if (itemCount > 0)
            {
                factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I宇宙矩阵粗坯] += itemCount;

                for (var index = 0; index < 5; ++index)
                {
                    factoryStatPool.productRegister[ProtoIDUsedByPatches.I电磁矩阵 + index] += itemCount;

                    component.matrixServed[index] += itemCount * 3600;
                    component.matrixIncServed[index] += itemInc * 720;
                }
            }
        }

        public static int MartixComplex_Insert(
            ref LabComponent component,
            PlanetFactory factory,
            int itemId,
            byte itemCount,
            byte itemInc,
            ref byte remainInc)
        {
            if (component.matrixServed != null)
            {
                var factoryStatPool = GameMain.statistics.production.factoryStatPool[factory.index];

                switch (itemId)
                {
                    case ProtoIDUsedByPatches.I稳定矩阵:
                        factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I稳定矩阵] += itemCount;
                        factoryStatPool.productRegister[ProtoIDUsedByPatches.I电磁矩阵] += itemCount;
                        factoryStatPool.productRegister[ProtoIDUsedByPatches.I能量矩阵] += itemCount;

                        component.matrixServed[0] += 3600 * itemCount;
                        component.matrixIncServed[0] += 3600 * itemInc;
                        component.matrixServed[1] += 3600 * itemCount;
                        component.matrixIncServed[1] += 3600 * itemInc;
                        remainInc = 0;
                        return itemCount;

                    case ProtoIDUsedByPatches.I虚空矩阵:
                        factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I虚空矩阵] += itemCount;
                        factoryStatPool.productRegister[ProtoIDUsedByPatches.I结构矩阵] += itemCount;
                        factoryStatPool.productRegister[ProtoIDUsedByPatches.I信息矩阵] += itemCount;
                        
                        component.matrixServed[2] += 3600 * itemCount;
                        component.matrixIncServed[2] += 3600 * itemInc;
                        component.matrixServed[3] += 3600 * itemCount;
                        component.matrixIncServed[3] += 3600 * itemInc;
                        remainInc = 0;
                        return itemCount;

                    case ProtoIDUsedByPatches.I宇宙矩阵粗坯:
                    {
                        factoryStatPool.consumeRegister[ProtoIDUsedByPatches.I宇宙矩阵粗坯] += itemCount;

                        for (var index = 0; index < 5; ++index)
                        {
                            factoryStatPool.productRegister[ProtoIDUsedByPatches.I电磁矩阵 + index] += itemCount;

                            component.matrixServed[index] += itemCount * 3600;
                            component.matrixIncServed[index] += itemInc * 3600;
                        }
                        remainInc = 0;
                        return itemCount;
                    }
                }
            }

            return 0;
        }

        private static readonly int[] matrixComplexIds
            = new int[] { ProtoIDUsedByPatches.I稳定矩阵, ProtoIDUsedByPatches.I虚空矩阵, ProtoIDUsedByPatches.I宇宙矩阵粗坯 };

        [HarmonyPatch(typeof(LabComponent), "UpdateNeedsResearch")]
        [HarmonyPostfix]
        public static void LabComponent_UpdateNeedsResearch_PostFix(ref LabComponent __instance)
        {
            var matrixPoints = __instance.matrixPoints;
            var matrixServed = __instance.matrixServed;

            bool CheckMatrixNeed(int i) => matrixPoints[i] != 0 && matrixServed[i] < 36000;

            if (CheckMatrixNeed(1) && CheckMatrixNeed(0))
            {
                var index = 0;

                if (CheckMatrixNeed(3) && CheckMatrixNeed(2)) index = 1;

                if (CheckMatrixNeed(4)) index = 2;

                for (var i = 5; i >= 0; --i)
                {
                    if (matrixPoints[i] == 0)
                    {
                        __instance.needs[i] = matrixComplexIds[index];
                        if (--index < 0) return;
                    }
                }
            }
        }
    }
}
