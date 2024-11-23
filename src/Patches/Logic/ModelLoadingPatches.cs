using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class ModelLoadingPatches
    {
        private static readonly Func<short, short> ModelIdMigrationAction = modelIndex =>
        {
            if (modelIndex > 500 && modelIndex < 520) modelIndex += 300;

            return modelIndex;
        };

        [HarmonyPatch(typeof(PrebuildData), nameof(PrebuildData.Import))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PrebuildData_Import_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PrebuildData), nameof(PrebuildData.modelIndex))));

            matcher.InsertAndAdvance(Transpilers.EmitDelegate(ModelIdMigrationAction));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(EntityData), nameof(EntityData.Import))]
        [HarmonyPostfix]
        public static void EntityData_Import(ref EntityData __instance)
        {
            __instance.modelIndex = ModelIdMigrationAction(__instance.modelIndex);
        }

        [HarmonyPatch(typeof(BlueprintBuilding), nameof(BlueprintBuilding.Import))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BlueprintBuilding_Import_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BlueprintBuilding), nameof(BlueprintBuilding.modelIndex))));

            matcher.InsertAndAdvance(Transpilers.EmitDelegate(ModelIdMigrationAction));

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

        [HarmonyPatch(typeof(ModelProto), nameof(ModelProto.InitMaxModelIndex))]
        [HarmonyPostfix]
        public static void InitMaxModelIndex()
        {
            ModelProto.maxModelIndex = LDB.models.dataArray.Max(model => model?.ID).GetValueOrDefault();
        }
    }
}
