using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetModelingManager_LoadingPlanetFactoryMain_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.type))),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(AddVeinPatches), nameof(RefVeinDataToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetFactory), "AddVeinData")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_AddVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.type))),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(AddVeinPatches), nameof(VeinDataToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        public static uint RefVeinDataToAnimDataState(ref VeinData data)
        {
            if (data.modelIndex > 2) return (uint)data.type;
            Color32 color = VeinColors[(int)data.type];
            return (uint)(color.a << 24) | (uint)(color.b << 16) | (uint)(color.g << 8) | color.r;
        }

        public static uint VeinDataToAnimDataState(VeinData data)
        {
            if (data.modelIndex > 2) return (uint)data.type;
            Color32 color = VeinColors[(int)data.type];
            return (uint)(color.a << 24) | (uint)(color.b << 16) | (uint)(color.g << 8) | color.r;
        }
    }
}
