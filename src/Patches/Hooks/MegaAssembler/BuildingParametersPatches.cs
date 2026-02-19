using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static partial class MegaAssemblerPatches
    {
        private static readonly FieldInfo PrefabDesc_assemblerRecipeType_Field =
            AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.assemblerRecipeType));

        private static readonly MethodInfo MegaAssemblerPatches_ContainsRecipeTypeRevert_Method =
            AccessTools.Method(typeof(MegaAssemblerPatches), nameof(ContainsRecipeTypeRevert));

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildingParameters_ApplyPrebuildParametersToEntity_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.forceAccMode))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Ldarg, 5),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(MegaAssemblerPatches),
                        nameof(BuildingParameters_ApplyPrebuildParametersToEntity_Patch_Method))));

            return matcher.InstructionEnumeration();
        }

        public static void BuildingParameters_ApplyPrebuildParametersToEntity_Patch_Method(int entityId, int[] parameters,
            PlanetFactory factory)
        {
            if (parameters == null || parameters.Length < 2048) return;

            SlotData[] slots = GetSlots(factory.planetId, entityId);
            const int num4 = 192;

            for (var index = 0; index < slots.Length; ++index)
            {
                slots[index].dir = (IODir)parameters[num4 + index * 4];
                slots[index].storageIdx = parameters[num4 + index * 4 + 1];
            }

            SyncSlotsData.Sync(factory.planetId, entityId, slots);
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.FromParamsArray))]
        [HarmonyPostfix]
        public static void BuildingParameters_FromParamsArray(ref BuildingParameters __instance, int[] _parameters)
        {
            if (__instance.type != BuildingType.Assembler) return;

            if (_parameters == null || _parameters.Length < 2048) return;

            if (__instance.parameters.Length < 2048) Array.Resize(ref __instance.parameters, 2048);

            Array.Copy(_parameters, __instance.parameters, 2048);
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ToParamsArray))]
        [HarmonyPostfix]
        public static void BuildingParameters_ToParamsArray(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if (__instance.type != BuildingType.Assembler) return;

            if (__instance.parameters.Length >= 2048)
            {
                if (_parameters == null || _parameters.Length < 2048) Array.Resize(ref _parameters, 2048);

                Array.Copy(__instance.parameters, _parameters, 2048);
                _paramCount = _parameters.Length;
            }
            else
            {
                _paramCount = __instance.parameters.Length;

                if (_parameters == null || _parameters.Length < _paramCount) Array.Resize(ref _parameters, _paramCount);
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CopyFromFactoryObject))]
        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CopyFromBuildPreview))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildingParameters_CopyFromFactoryObject_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));


            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brtrue);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CopyFromFactoryObject))]
        [HarmonyPostfix]
        public static void BuildingParameters_CopyFromFactoryObject(ref BuildingParameters __instance, int objectId, PlanetFactory factory,
            bool copyInserters)
        {
            if (__instance.type != BuildingType.Assembler) return;

            if (objectId > 0)
            {
                if (factory.entityPool.Length <= objectId || factory.entityPool[objectId].id != objectId) return;

                int assemblerId = factory.entityPool[objectId].assemblerId;

                if (assemblerId <= 0 || factory.factorySystem.assemblerPool.Length <= assemblerId) return;

                AssemblerComponent assembler = factory.factorySystem.assemblerPool[assemblerId];

                if (assembler.id != assemblerId || assembler.speed < MegaAssemblerSpeed) return;

                if (__instance.parameters == null || __instance.parameters.Length < 2048) Array.Resize(ref __instance.parameters, 2048);

                __instance.parameters[0] = assembler.forceAccMode ? 1 : 0;
                __instance.recipeId = assembler.recipeId;
                __instance.recipeType = assembler.recipeType;

                const int num2 = 192;

                SlotData[] slots = GetSlots(factory.planetId, objectId);

                for (var index = 0; index < slots.Length; ++index)
                {
                    __instance.parameters[num2 + index * 4] = (int)slots[index].dir;
                    __instance.parameters[num2 + index * 4 + 1] = slots[index].storageIdx;
                }

                SyncSlotsData.Sync(factory.planetId, objectId, slots);
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.PasteToFactoryObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildingParameters_PasteToFactoryObject_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(3).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CanPasteToFactoryObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildingParameters_CanPasteToFactoryObject_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_assemblerRecipeType_Field));

            matcher.Advance(3).InsertAndAdvance(new CodeInstruction(OpCodes.Call, MegaAssemblerPatches_ContainsRecipeTypeRevert_Method));

            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }
    }
}
