using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class ModelLoadingPatches
    {
        private static readonly Func<int, int> Action = modelIndex =>
        {
            if (modelIndex >= 401 && modelIndex <= 412) modelIndex += 50;
            return modelIndex;
        };

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
        public static IEnumerable<CodeInstruction> LoadingPlanetFactoryMain_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // prebuild part
            var codeMatcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldelema, typeof(PrebuildData)),
                                                                         new CodeMatch(OpCodes.Ldfld,
                                                                                       AccessTools.Field(typeof(PrebuildData), "modelIndex")));

            codeMatcher.Advance(1).InsertAndAdvance(Transpilers.EmitDelegate(Action));

            // entity part
            codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                     new CodeMatch(OpCodes.Ldelema, typeof(EntityData)), new CodeMatch(OpCodes.Ldc_I4_0),
                                     new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(EntityData), "audioId")), new CodeMatch(OpCodes.Ldloc_0),
                                     new CodeMatch(OpCodes.Ldloc_S),
                                     new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlanetFactory), "CreateEntityDisplayComponents")));

            var entityPool = codeMatcher.Instruction;
            var entityId = codeMatcher.Advance(1).Instruction;
            var ldelema = codeMatcher.Advance(1).Instruction;

            var modelIndexFieldInfo = AccessTools.Field(typeof(EntityData), "modelIndex");
            codeMatcher.Advance(3).InsertAndAdvance(new CodeInstruction(entityPool), new CodeInstruction(entityId), new CodeInstruction(ldelema),
                                                    new CodeInstruction(entityPool), new CodeInstruction(entityId), new CodeInstruction(ldelema),
                                                    new CodeInstruction(OpCodes.Ldfld, modelIndexFieldInfo), Transpilers.EmitDelegate(Action),
                                                    new CodeInstruction(OpCodes.Stfld, modelIndexFieldInfo));

            return codeMatcher.InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BlueprintBuilding), "Import")]
        public static IEnumerable<CodeInstruction> BlueprintBuilding_Import_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt),
                                                                         new CodeMatch(OpCodes.Stfld,
                                                                                       AccessTools.Field(typeof(BlueprintBuilding), "modelIndex")));

            codeMatcher.Advance(1).InsertAndAdvance(Transpilers.EmitDelegate(Action));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
