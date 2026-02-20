using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 将ModelArray长度扩展到1024
    /// </summary>
    public static class ModelArrayExpandPatches
    {
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
        public static void InitMaxModelIndex() =>
            ModelProto.maxModelIndex = LDB.models.dataArray.Max(model => model?.ID).GetValueOrDefault();
    }
}
