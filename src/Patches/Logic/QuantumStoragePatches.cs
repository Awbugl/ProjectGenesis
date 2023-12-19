using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class QuantumStoragePatches
    {
        private const int QuantumStorageSize = 90;

        private static readonly StorageComponent _component = new StorageComponent(QuantumStorageSize);

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
            }
        }

        [HarmonyPatch(typeof(StorageComponent), "SetEmpty")]
        [HarmonyPrefix]
        public static bool StorageComponent_SetEmpty(StorageComponent __instance) => __instance.size != QuantumStorageSize;

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

            matcher.Advance(-6).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, ProtoIDUsedByPatches.I量子储物仓),
                                                 new CodeInstruction(OpCodes.Ldloc_0),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                                                 new CodeInstruction(OpCodes.Brtrue_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }
    }
}
