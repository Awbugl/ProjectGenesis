using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static partial class QuantumStoragePatches
    {
        [HarmonyPatch(typeof(FactoryStorage), "NewStorageComponent")]
        [HarmonyPostfix]
        public static void FactoryStorage_NewStorageComponent(
            FactoryStorage __instance,
            int entityId,
            int size,
            ref StorageComponent __result)
        {
            if (size == QuantumStorageSize)
            {
                __instance.storagePool[__result.id] = _component;
                __result = _component;
                SyncNewQuantumStorageData.Sync(__instance.planet.id, __result.id);
            }
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
        [HarmonyPrefix]
        public static bool StorageComponent_SetEmpty(StorageComponent __instance) => __instance.size != QuantumStorageSize;

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

        [HarmonyPatch(typeof(FactoryStorage), "Export")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactoryStorage_Export_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).Advance(1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuantumStoragePatches), nameof(Export_PatchMethod))))
                   .SetOpcodeAndAdvance(OpCodes.Brtrue);

            return matcher.InstructionEnumeration();
        }

        public static bool Export_PatchMethod(StorageComponent component, FactoryStorage storage, int index)
        {
            bool b = component.size == QuantumStorageSize;
            if (b) _quantumStorageIds.TryAddOrInsert(storage.planet.id, index);
            return b;
        }
    }
}
