using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class ModelLoadingPatches
    {
        // model id migration
        private static readonly Func<int, int> Action = modelIndex =>
        {
            if (modelIndex > 500 && modelIndex < 520) modelIndex += 300;
            return modelIndex;
        };

        [HarmonyPatch(typeof(PlanetModelingManager), nameof(PlanetModelingManager.LoadingPlanetFactoryMain))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoadingPlanetFactoryMain_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // prebuild part
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldelema, typeof(PrebuildData)),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrebuildData), nameof(PrebuildData.modelIndex))));

            matcher.Advance(1).InsertAndAdvance(Transpilers.EmitDelegate(Action));

            // entity part
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldelema, typeof(EntityData)), new CodeMatch(OpCodes.Ldc_I4_0),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(EntityData), nameof(EntityData.audioId))),
                                 new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Callvirt,
                                               AccessTools.Method(typeof(PlanetFactory), nameof(PlanetFactory.CreateEntityDisplayComponents))));

            CodeInstruction entityPool = matcher.Instruction;
            CodeInstruction entityId = matcher.Advance(1).Instruction;
            CodeInstruction ldelema = matcher.Advance(1).Instruction;

            FieldInfo modelIndexFieldInfo = AccessTools.Field(typeof(EntityData), nameof(EntityData.modelIndex));
            matcher.Advance(3).InsertAndAdvance(new CodeInstruction(entityPool), new CodeInstruction(entityId), new CodeInstruction(ldelema),
                                                new CodeInstruction(entityPool), new CodeInstruction(entityId), new CodeInstruction(ldelema),
                                                new CodeInstruction(OpCodes.Ldfld, modelIndexFieldInfo), Transpilers.EmitDelegate(Action),
                                                new CodeInstruction(OpCodes.Stfld, modelIndexFieldInfo));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BlueprintBuilding), nameof(BlueprintBuilding.Import))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BlueprintBuilding_Import_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Callvirt),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.modelIndex))));

            matcher.Advance(1).InsertAndAdvance(Transpilers.EmitDelegate(Action));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.InitPrefabDescArray))]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InitPrefabDescArray))]
        [HarmonyPatch(typeof(ModelProtoSet), nameof(ModelProtoSet.OnAfterDeserialize))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ModelProtoSet_OnAfterDeserialize_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Newarr));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldc_I4, 1024));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(SkillSystem), MethodType.Constructor, typeof(SpaceSector))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SkillSystem_Constructor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Newarr));

            do
            {
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldc_I4, 1024));
                matcher.Advance(1).MatchForward(false, new CodeMatch(OpCodes.Newarr));
            }
            while (matcher.IsValid);

            return matcher.InstructionEnumeration();
        }
    }
}
