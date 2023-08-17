using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        internal const int TrashSpeed = 60000;
        internal const int MegaAssemblerSpeed = 400000;

        private static int TmpSandCount;

        private static readonly FieldInfo EntityData_StationId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.stationId)),
                                          EntityData_AssemblerId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.assemblerId)),
                                          PlanetFactory_EntityPool_Field = AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityPool)),
                                          FactorySystem_AssemblerPool_Field
                                              = AccessTools.Field(typeof(FactorySystem), nameof(FactorySystem.assemblerPool)),
                                          AssemblerComponent_Speed_Field
                                              = AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.speed));

        private static readonly MethodInfo AssemblerComponent_InternalUpdate_Method
                                               = AccessTools.Method(typeof(AssemblerComponent), nameof(AssemblerComponent.InternalUpdate)),
                                           MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches),
                                                                    nameof(GameTick_AssemblerComponent_InternalUpdate_Patch)),
                                           MegaAssembler_AssemblerComponent_UpdateNeeds_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches), nameof(AssemblerComponent_UpdateNeeds_Patch));

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                                                                new CodeMatch(OpCodes.Ldloc_1), new CodeMatch(OpCodes.Ldloc_2),
                                                                                new CodeMatch(OpCodes.Call,
                                                                                              AssemblerComponent_InternalUpdate_Method));

            var local1 = matcher.Operand;
            var power1 = matcher.Advance(1).Operand;

            matcher.CreateLabelAt(matcher.Pos + 4, out var label1);

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldloc_S, local1),
                                                 new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power1),
                                                 new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                 new CodeInstruction(OpCodes.Brfalse_S, label1), new CodeInstruction(OpCodes.Pop));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                 new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            if (matcher.IsValid)
            {
                var local2 = matcher.Operand;
                var power2 = matcher.Advance(1).Operand;

                matcher.CreateLabelAt(matcher.Pos + 4, out var label2);

                matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldloc_S, local2),
                                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power2),
                                                     new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                     new CodeInstruction(OpCodes.Brfalse_S, label2), new CodeInstruction(OpCodes.Pop));
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler_2(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld),
                                                                                new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelema),
                                                                                new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                                                                new CodeMatch(OpCodes.Ldloc_2),
                                                                                new CodeMatch(OpCodes.Call,
                                                                                              AssemblerComponent_InternalUpdate_Method));

            var index = matcher.Advance(2).Operand;
            var power = matcher.Advance(2).Operand;

            matcher.CreateLabelAt(matcher.Pos + 4, out var label);

            matcher.Advance(-4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldarg_0),
                                                 new CodeInstruction(OpCodes.Ldfld, FactorySystem_AssemblerPool_Field),
                                                 new CodeInstruction(OpCodes.Ldloc_S, index),
                                                 new CodeInstruction(OpCodes.Ldelema, typeof(AssemblerComponent)),
                                                 new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power),
                                                 new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                 new CodeInstruction(OpCodes.Brfalse_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }

        public static bool GameTick_AssemblerComponent_InternalUpdate_Patch(
            ref AssemblerComponent __instance,
            FactorySystem factorySystem,
            float power)
        {
            // 巨型建筑效果
            if (__instance.speed >= TrashSpeed)
            {
                var factory = factorySystem.factory;
                SlotData[] slotdata = GetSlots(factory.planetId, __instance.entityId);

                var cargoTraffic = factory.cargoTraffic;
                SignData[] entitySignPool = factory.entitySignPool;

                var stationPilerLevel = GameMain.history.stationPilerLevel;

                UpdateOutputSlots(ref __instance, cargoTraffic, slotdata, entitySignPool, stationPilerLevel);
                UpdateInputSlots(ref __instance, power, factory, cargoTraffic, slotdata, entitySignPool);
            }

            return power < 0.1f;
        }

        private static void UpdateOutputSlots(
            ref AssemblerComponent __instance,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool,
            int maxPilerCount)
        {
            for (var index1 = 0; index1 < slotdata.Length; ++index1)
            {
                if (slotdata[index1].dir == IODir.Output)
                {
                    if (slotdata[index1].counter > 0)
                    {
                        --slotdata[index1].counter;
                    }
                    else
                    {
                        var beltId = slotdata[index1].beltId;
                        if (beltId <= 0) continue;
                        var beltComponent = traffic.beltPool[beltId];
                        var cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
                        if (cargoPath == null) continue;

                        var index2 = slotdata[index1].storageIdx - 1;
                        var itemId = 0;

                        if (index2 >= 0)
                        {
                            if (index2 < __instance.products.Length)
                            {
                                itemId = __instance.products[index2];
                                var produced = __instance.produced[index2];
                                if (itemId > 0 && produced > 0)
                                {
                                    var num2 = produced < maxPilerCount ? produced : maxPilerCount;
                                    if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, 0)) __instance.produced[index2] -= num2;
                                }
                            }
                            else
                            {
                                var index3 = index2 - __instance.products.Length;
                                if (index3 < __instance.requires.Length)
                                {
                                    itemId = __instance.requires[index3];
                                    var served = __instance.served[index3];
                                    if (itemId > 0 && served > 0)
                                    {
                                        var num2 = served < maxPilerCount ? served : maxPilerCount;
                                        var inc = (int)((double)__instance.incServed[index3] * num2 / __instance.served[index3]);
                                        if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, (byte)inc))
                                        {
                                            __instance.incServed[index3] -= inc;
                                            __instance.served[index3] -= num2;
                                        }
                                    }
                                }
                            }
                        }

                        if (itemId > 0)
                        {
                            var entityId = beltComponent.entityId;
                            signPool[entityId].iconType = 1U;
                            signPool[entityId].iconId0 = (uint)itemId;
                        }
                    }
                }
                else if (slotdata[index1].dir != IODir.Input)
                {
                    slotdata[index1].beltId = 0;
                    slotdata[index1].counter = 0;
                }
            }
        }

        private static void UpdateInputSlots(
            ref AssemblerComponent __instance,
            float power,
            PlanetFactory factory,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool)
        {
            for (var index = 0; index < slotdata.Length; ++index)
            {
                if (slotdata[index].dir == IODir.Input)
                {
                    if (slotdata[index].counter > 0)
                    {
                        --slotdata[index].counter;
                    }
                    else
                    {
                        var beltId = slotdata[index].beltId;
                        if (beltId <= 0) continue;
                        var beltComponent = traffic.beltPool[beltId];
                        var cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
                        if (cargoPath == null) continue;

                        if (__instance.recipeId == ProtoIDUsedByPatches.R物质分解)
                        {
                            if (power < 0.1f) return;

                            var itemId = traffic.TryPickItemAtRear(beltId, 0, null, out var stack, out _);

                            if (itemId <= 0) continue;

                            var consumeRegister = GameMain.statistics.production.factoryStatPool[factory.index].consumeRegister;

                            lock (consumeRegister)
                            {
                                consumeRegister[itemId] += stack;
                            }

                            TmpSandCount += stack;

                            if (TmpSandCount < 1000 || GameMain.mainPlayer == null) continue;

                            // This method will be called in a worker thread (not main UI thread).
                            // Thus, calling `GameMain.mainPlayer.SetSandCount` which brings up sand tooltip UI
                            // will crash the program.
                            // Instead, we should increase the sand count directly.
                            AccessTools.PropertySetter(typeof(Player), "sandCount").Invoke(GameMain.mainPlayer,
                                                                                           new object[]
                                                                                           {
                                                                                               Math.Min(1000000000,
                                                                                                        GameMain.mainPlayer.sandCount +
                                                                                                        TmpSandCount * 20)
                                                                                           });
                            TmpSandCount = 0;
                        }
                        else
                        {
                            var itemId = cargoPath.TryPickItemAtRear(__instance.needs, out var needIdx, out var stack, out var inc);

                            if (needIdx >= 0 && itemId > 0 && __instance.needs[needIdx] == itemId)
                            {
                                __instance.served[needIdx] += stack;
                                __instance.incServed[needIdx] += inc;
                                slotdata[index].storageIdx = __instance.products.Length + needIdx + 1;
                            }

                            for (var i = 0; i < __instance.products.Length; i++)
                            {
                                if (__instance.produced[i] >= 50) continue;

                                itemId = traffic.TryPickItemAtRear(beltId, __instance.products[i], null, out stack, out _);

                                if (__instance.products[i] == itemId)
                                {
                                    __instance.produced[i] += stack;
                                    slotdata[index].storageIdx = i + 1;
                                    break;
                                }
                            }

                            if (itemId > 0)
                            {
                                var entityId = beltComponent.entityId;
                                signPool[entityId].iconType = 1U;
                                signPool[entityId].iconId0 = (uint)itemId;
                            }
                        }
                    }
                }
                else if (slotdata[index].dir != IODir.Output)
                {
                    slotdata[index].beltId = 0;
                    slotdata[index].counter = 0;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        public static void PlanetFactory_ApplyInsertTarget(
            ref PlanetFactory __instance,
            int entityId,
            int insertTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            var assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId <= 0) return;

            var assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

            var beltId = __instance.entityPool[insertTarget].beltId;
            if (beltId <= 0) return;
            SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
            slotdata[slotId].dir = IODir.Output;
            slotdata[slotId].beltId = beltId;
            slotdata[slotId].counter = 0;
            SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyPickTarget")]
        public static void PlanetFactory_ApplyPickTarget(
            ref PlanetFactory __instance,
            int entityId,
            int pickTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            var assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId <= 0) return;

            var assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

            var beltId = __instance.entityPool[pickTarget].beltId;
            if (beltId <= 0) return;
            SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
            slotdata[slotId].dir = IODir.Input;
            slotdata[slotId].beltId = beltId;
            slotdata[slotId].storageIdx = 0;
            slotdata[slotId].counter = 0;
            SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        public static void PlanetFactory_ApplyEntityDisconnection(
            ref PlanetFactory __instance,
            int otherEntityId,
            int removingEntityId,
            int otherSlotId,
            int removingSlotId)
        {
            if (otherEntityId == 0) return;
            var assemblerId = __instance.entityPool[otherEntityId].assemblerId;
            if (assemblerId <= 0) return;

            var assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

            var beltId = __instance.entityPool[removingEntityId].beltId;
            if (beltId <= 0) return;

            SlotData[] slotdata = GetSlots(__instance.planetId, otherEntityId);

            slotdata[otherSlotId].dir = IODir.None;
            slotdata[otherSlotId].beltId = 0;
            slotdata[otherSlotId].counter = 0;

            SyncSlotData.Sync(__instance.planetId, otherSlotId, otherEntityId, slotdata[otherSlotId]);
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(PlanetFactory), "RemoveEntityWithComponents")]
        public static void PlanetFactory_RemoveEntityWithComponents(ref PlanetFactory __instance, int id)
        {
            if (id != 0)
            {
                var entityData = __instance.entityPool[id];

                if (entityData.id != 0 && entityData.assemblerId != 0) SetEmpty(__instance.planetId, id);
            }
        }

        [HarmonyPatch(typeof(AssemblerComponent), "UpdateNeeds")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_UpdateNeeds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_3), new CodeMatch(OpCodes.Mul));

                if (matcher.IsInvalid) break;

                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AssemblerComponent_Speed_Field));
                matcher.SetInstruction(new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_UpdateNeeds_Patch_Method));
            }

            return matcher.InstructionEnumeration();
        }

        public static sbyte AssemblerComponent_UpdateNeeds_Patch(int speed) => speed > TrashSpeed ? (sbyte)10 : (sbyte)3;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "NewAssemblerComponent")]
        public static void FactorySystem_NewAssemblerComponent(ref FactorySystem __instance, int entityId, int speed)
        {
            if (speed >= TrashSpeed) __instance.factory.entityPool[entityId].stationId = 0;
        }
    }
}
