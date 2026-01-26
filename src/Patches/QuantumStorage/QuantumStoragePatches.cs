using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static partial class QuantumStoragePatches
    {
        private static readonly Mutex QuantumStorageMutex = new Mutex(-1);

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
            new[] {
                typeof(int), typeof(int), typeof(int), typeof(bool),
            }, new[] {
                ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Out, ArgumentType.Normal,
            })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
            new[] {
                typeof(int), typeof(int), typeof(int[]), typeof(int), typeof(bool),
            }, new[] {
                ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal,
            })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItemsFiltered))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void StorageComponent_TakeTailItems_Prefix(StorageComponent __instance)
        {
            if (__instance.size == QuantumStorageSize) Monitor.Enter(QuantumStorageMutex);
        }

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
            new[] {
                typeof(int), typeof(int), typeof(int), typeof(bool),
            }, new[] {
                ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Out, ArgumentType.Normal,
            })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
            new[] {
                typeof(int), typeof(int), typeof(int[]), typeof(int), typeof(bool),
            }, new[] {
                ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal,
            })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItemsFiltered))]
        [HarmonyPostfix]
        public static void StorageComponent_TakeTailItems_Postfix(StorageComponent __instance)
        {
            if (__instance.size == QuantumStorageSize) Monitor.Exit(QuantumStorageMutex);
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.GameTickStorage))]
        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.GetStorageComponent))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactoryStorage_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))));

            CodeInstruction ins = matcher.InstructionAt(-1);

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(ins),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(QuantumStoragePatches), nameof(FactoryStorage_GameTick_PatchMethod))));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        public static bool FactoryStorage_GameTick_PatchMethod(int id, int index, StorageComponent component) =>
            id == index || component.size == QuantumStorageSize;

        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.UpdateRaycast))]
        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.DeterminePreviews))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Addon_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStorage))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(QuantumStoragePatches), nameof(BuildTool_Addon_PatchMethod))));

            return matcher.InstructionEnumeration();
        }

        public static bool BuildTool_Addon_PatchMethod(PrefabDesc desc) => desc.isStorage && desc.storageRow != 9;

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.SetEmpty))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool StorageComponent_SetEmpty(StorageComponent __instance) => __instance.size != QuantumStorageSize;

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.NewStorageComponent))]
        [HarmonyPostfix]
        public static void FactoryStorage_NewStorageComponent(FactoryStorage __instance, int entityId, int size,
            ref StorageComponent __result)
        {
            if (size != QuantumStorageSize) return;

            int storageId = __result.id;

            var orbitId = 1;

            StorageComponent _component = _components[orbitId - 1];

            __instance.storagePool[storageId] = _component;
            __result = _component;

            QuantumStorageIds.TryAddOrInsert(__instance.planet.id, new QuantumStorageData(storageId, orbitId));
            SyncNewQuantumStorageData.Sync(__instance.planet.id, storageId, orbitId);
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.RemoveStorageComponent))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool FactoryStorage_RemoveStorageComponent(FactoryStorage __instance, int id)
        {
            StorageComponent storageComponent = __instance.storagePool[id];

            if (storageComponent == null) return false;

            if (storageComponent.size != QuantumStorageSize) return true;

            int orbitId = QueryOrbitId(__instance.planet.id, id);

            QuantumStorageIds.TryRemove(__instance.planet.id, new QuantumStorageData(id, orbitId));

            __instance.storagePool[id] = new StorageComponent(30);
            __instance.storagePool[id].SetEmpty();
            __instance.storageRecycle[__instance.storageRecycleCursor++] = id;

            SyncRemoveQuantumStorageData.Sync(__instance.planet.id, id, orbitId);

            return false;
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.TryTakeBackItems_Storage))]
        [HarmonyPostfix]
        public static void FactoryStorage_TryTakeBackItems_Storage(FactoryStorage __instance, int storageId, ref bool __result)
        {
            StorageComponent package = __instance.GetStorageComponent(storageId);

            if (package == null || package.size == QuantumStorageSize) __result = true;
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertInto),
            new[] {
                typeof(int), typeof(int), typeof(int), typeof(byte), typeof(byte), typeof(byte),
            }, new[] {
                ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                ArgumentType.Out,
            })]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertIntoStorage))]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertCargoIntoStorage))]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.PickFrom),
            new[] {
                typeof(int), typeof(int), typeof(int), typeof(int[]), typeof(byte), typeof(byte),
            }, new[] {
                ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out,
                ArgumentType.Out,
            })]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.PickFromStorage))]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.PickFromStorageFiltered))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_Mutexs_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityMutexs))),
                new CodeMatch(inst => inst.IsLdloc()),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.entityId))),
                new CodeMatch(OpCodes.Ldelem_Ref));

            CodeInstruction ins = matcher.InstructionAt(-2);

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(ins),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(QuantumStoragePatches), nameof(PlanetFactory_Mutexs_PatchMethod))));

            return matcher.InstructionEnumeration();
        }

        public static Mutex PlanetFactory_Mutexs_PatchMethod(Mutex mutex, StorageComponent component) =>
            component.size != QuantumStorageSize ? mutex : QuantumStorageMutex;

        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow._OnUpdate))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnStorageIdChange))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnSortClick))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnBansSliderValueChange))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnFilterItemPickerReturn))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnFilterButton2Click))]
        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnFilterButton3Click))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageWindow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(UIStorageWindow), nameof(UIStorageWindow.storageId))));

            object label = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool PatchMethod(StorageComponent component) => component.size == QuantumStorageSize;

        [HarmonyPatch(typeof(UIStorageWindow), nameof(UIStorageWindow.OnStorageIdChange))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageWindow_OnStorageIdChange_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EntityData), nameof(EntityData.protoId))));

            matcher.CreateLabelAt(matcher.Pos + 1, out Label label);

            matcher.Advance(-6).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, ProtoID.I量子储物仓), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                new CodeInstruction(OpCodes.Brtrue_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.Import))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactoryStorage_Import_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_2), new CodeMatch(OpCodes.Ldloc_2));

            object label = matcher.InstructionAt(2).operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuantumStoragePatches), nameof(Import_PatchMethod))),
                new CodeInstruction(OpCodes.Brtrue, label));

            return matcher.InstructionEnumeration();
        }
    }
}
