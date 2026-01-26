using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static partial class MegaAssemblerPatches
    {
        private static readonly FieldInfo UIBeltBuildTip_FilterItems_Field =
                                              AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.filterItems)),
                                          UISlotPicker_FilterItems_Field =
                                              AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.filterItems)),
                                          UIBeltBuildTip_SelectedIndex_Field =
                                              AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.selectedIndex)),
                                          UISlotPicker_SelectedIndex_Field =
                                              AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.selectedIndex)),
                                          UIBeltBuildTip_OutputEntityId_Field =
                                              AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputEntityId)),
                                          UISlotPicker_OutputEntityId_Field =
                                              AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.outputEntityId)),
                                          UIBeltBuildTip_OutputSlotId_Field =
                                              AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputSlotId)),
                                          UISlotPicker_OutputSlotId_Field =
                                              AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.outputSlotId));

        private static readonly MethodInfo MegaAssembler_SetOutputEntity_Patch_Method =
                                               AccessTools.Method(typeof(MegaAssemblerPatches), nameof(SetOutputEntity_Patch)),
                                           MegaAssembler_SetFilterToEntity_Patch_Method = AccessTools.Method(typeof(MegaAssemblerPatches),
                                               nameof(SetFilterToEntity_Patch));

        // prevent a packet flood when the filter on a belt connecting.
        // special thanks for https://github.com/hubastard/nebula/tree/master/NebulaPatcher/Patches/Transpilers/UIBeltBuildTip_Transpiler.cs

        private static (int, int) LastSetId;
        private static int LastSlotId;
        private static int LastSelectedIndex;

        [HarmonyPatch(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.SetOutputEntity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBeltBuildTip_SetOutputEntity_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_3));

            matcher.Advance(1).CreateLabel(out Label label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_3), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, UIBeltBuildTip_FilterItems_Field), new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(OpCodes.Call, MegaAssembler_SetOutputEntity_Patch_Method),
                new CodeInstruction(OpCodes.Stfld, UIBeltBuildTip_SelectedIndex_Field));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), nameof(UISlotPicker.SetOutputEntity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_SetOutputEntity_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabel(out Label label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, UISlotPicker_FilterItems_Field), new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(OpCodes.Call, MegaAssembler_SetOutputEntity_Patch_Method),
                new CodeInstruction(OpCodes.Stfld, UISlotPicker_SelectedIndex_Field));

            return matcher.InstructionEnumeration();
        }

        public static int SetOutputEntity_Patch(PlanetFactory factory, EntityData entityData, ref List<int> filterItems, int entityId,
            int slot)
        {
            AssemblerComponent assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

            if (assemblerComponent.speed < MegaAssemblerSpeed) return -1;

            if (assemblerComponent.recipeId > 0)
            {
                filterItems.AddRange(assemblerComponent.recipeExecuteData.products);
                filterItems.AddRange(assemblerComponent.recipeExecuteData.requires);
            }
            else { filterItems.AddRange(Enumerable.Repeat(0, 6)); }

            entityData.stationId = 0;

            if (slot >= 0 && slot < 12) return GetSlots(factory.planetId, entityId)[slot].storageIdx;

            Assert.CannotBeReached();

            return -1;
        }

        [HarmonyPatch(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.SetFilterToEntity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBeltBuildTip_SetFilterToEntity_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, UIBeltBuildTip_OutputEntityId_Field),
                new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabel(out Label label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_OutputEntityId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_OutputSlotId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_SelectedIndex_Field),
                new CodeInstruction(OpCodes.Call, MegaAssembler_SetFilterToEntity_Patch_Method));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), nameof(UISlotPicker.SetFilterToEntity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_SetFilterToEntity_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, UISlotPicker_OutputEntityId_Field),
                new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabel(out Label label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UISlotPicker_OutputEntityId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UISlotPicker_OutputSlotId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, UISlotPicker_SelectedIndex_Field),
                new CodeInstruction(OpCodes.Call, MegaAssembler_SetFilterToEntity_Patch_Method));

            return matcher.InstructionEnumeration();
        }

        public static void SetFilterToEntity_Patch(PlanetFactory factory, EntityData entityData, int outputEntityId, int outputSlotId,
            int selectedIndex)
        {
            AssemblerComponent assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

            if (assemblerComponent.speed < MegaAssemblerSpeed) return;

            SlotData[] slotDatas = GetSlots(factory.planetId, outputEntityId);
            slotDatas[outputSlotId].storageIdx = selectedIndex;

            entityData.stationId = 0;

            if (IsChangeCached((factory.planetId, outputEntityId), outputSlotId, selectedIndex)) return;

            SyncSlotData.Sync(factory.planetId, outputSlotId, outputEntityId, slotDatas[outputSlotId]);
            CacheChange((factory.planetId, outputEntityId), outputSlotId, selectedIndex);
        }

        private static bool IsChangeCached((int, int) id, int slotId, int selectedIndex) =>
            LastSetId == id && LastSlotId == slotId && LastSelectedIndex == selectedIndex;

        private static void CacheChange((int, int) id, int slotId, int selectedIndex)
        {
            LastSetId = id;
            LastSlotId = slotId;
            LastSelectedIndex = selectedIndex;
        }
    }
}
