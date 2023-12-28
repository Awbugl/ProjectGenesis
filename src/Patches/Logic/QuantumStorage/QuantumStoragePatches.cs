using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static partial class QuantumStoragePatches
    {
        private static readonly Mutex QuantumStorageMutex = new Mutex(-1);

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
                      new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool) },
                      new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Out, ArgumentType.Normal })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
                      new Type[] { typeof(int), typeof(int), typeof(int[]), typeof(int), typeof(bool) },
                      new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItemsFiltered))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void StorageComponent_TakeTailItems_Prefix(StorageComponent __instance)
        {
            if (__instance.size == QuantumStorageSize) Monitor.Enter(QuantumStorageMutex);
        }

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
                      new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool) },
                      new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Out, ArgumentType.Normal })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
                      new Type[] { typeof(int), typeof(int), typeof(int[]), typeof(int), typeof(bool) },
                      new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItemsFiltered))]
        [HarmonyPostfix]
        public static void StorageComponent_TakeTailItems_Postfix(StorageComponent __instance)
        {
            if (__instance.size == QuantumStorageSize) Monitor.Exit(QuantumStorageMutex);
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.GameTick))]
        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.GetStorageComponent))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactoryStorage_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))));

            CodeInstruction ins = matcher.InstructionAt(-1);

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(ins),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(QuantumStoragePatches),
                                                                                       nameof(FactoryStorage_GameTick_PatchMethod))));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        public static bool FactoryStorage_GameTick_PatchMethod(int id, int index, StorageComponent component)
            => id == index || component.size == QuantumStorageSize;

        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.UpdateRaycast))]
        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.DeterminePreviews))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Addon_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStorage))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(QuantumStoragePatches),
                                                                                    nameof(BuildTool_Addon_PatchMethod))));

            return matcher.InstructionEnumeration();
        }

        public static bool BuildTool_Addon_PatchMethod(PrefabDesc desc) => desc.isStorage && desc.storageRow != 9;

        [HarmonyPatch(typeof(StorageComponent), "SetEmpty")]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool StorageComponent_SetEmpty(StorageComponent __instance) => __instance.size != QuantumStorageSize;

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.NewStorageComponent))]
        [HarmonyPostfix]
        public static void FactoryStorage_NewStorageComponent(
            FactoryStorage __instance,
            int entityId,
            int size,
            ref StorageComponent __result)
        {
            if (size != QuantumStorageSize) return;

            __instance.storagePool[__result.id] = _component;
            __result = _component;

            _quantumStorageIds.TryAddOrInsert(__instance.planet.id, __result.id);
            SyncNewQuantumStorageData.Sync(__instance.planet.id, __result.id);
        }

        [HarmonyPatch(typeof(FactoryStorage), nameof(FactoryStorage.RemoveStorageComponent))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool FactoryStorage_RemoveStorageComponent(FactoryStorage __instance, int id)
        {
            StorageComponent storageComponent = __instance.storagePool[id];

            if (storageComponent == null) return false;

            if (storageComponent.size != QuantumStorageSize) return true;

            _quantumStorageIds.TryRemove(__instance.planet.id, id);

            __instance.storagePool[id] = new StorageComponent(30);
            __instance.storagePool[id].SetEmpty();
            __instance.storageRecycle[__instance.storageRecycleCursor++] = id;

            return false;
        }

        [HarmonyPatch(typeof(FactoryStorage), "TryTakeBackItems_Storage")]
        [HarmonyPostfix]
        public static void FactoryStorage_TryTakeBackItems_Storage(FactoryStorage __instance, int storageId, ref bool __result)
        {
            StorageComponent package = __instance.GetStorageComponent(storageId);
            if (package == null || package.size == QuantumStorageSize) __result = true;
        }

        [HarmonyPatch(typeof(UIStorageWindow), "_OnUpdate")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnSortClick")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnBansSliderValueChange")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterItemPickerReturn")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterButton2Click")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterButton3Click")]
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
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool PatchMethod(StorageComponent component) => component.size == QuantumStorageSize;

        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageWindow_OnStorageIdChange_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);


            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EntityData), nameof(EntityData.protoId))));

            matcher.CreateLabelAt(matcher.Pos + 1, out Label label);

            matcher.Advance(-6).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, ProtoID.I量子储物仓), new CodeInstruction(OpCodes.Ldloc_0),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                                                 new CodeInstruction(OpCodes.Brtrue_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }
    }
}
