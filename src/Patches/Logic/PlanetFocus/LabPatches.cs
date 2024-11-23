using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EnergyCap_Wind_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.techSpeed))),
                new CodeMatch(OpCodes.Conv_R4));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(PlanetFocusPatches), nameof(LabResearchMode))));

            return matcher.InstructionEnumeration();
        }

        public static float LabResearchMode(float techSpeed, FactorySystem factorySystem)
        {
            bool exist = ContainsFocus(factorySystem.factory.planetId, 6523);

            return exist ? techSpeed * 1.1f : techSpeed;
        }

        [HarmonyPatch(typeof(UILabWindow), nameof(UILabWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILabWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);


            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.techSpeed))),
                new CodeMatch(OpCodes.Conv_R4));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UILabWindow), nameof(UILabWindow.factory))));

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(LabTechSpeed))));


            return matcher.InstructionEnumeration();
        }

        public static double LabTechSpeed(double techSpeed, PlanetFactory factory)
        {
            bool exist = ContainsFocus(factory.planetId, 6523);

            return exist ? techSpeed * 1.1 : techSpeed;
        }
    }
}
