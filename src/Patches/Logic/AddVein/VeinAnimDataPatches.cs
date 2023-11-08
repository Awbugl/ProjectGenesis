using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        [HarmonyPatch(typeof(PlanetFactory), "AddVeinData")]
        [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_AddVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.type))),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddVeinPatches), nameof(EVeinTypeToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.productId))),
                                 new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(VeinProtoSet), nameof(VeinProtoSet.GetVeinTypeByItemId))),
                                 new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddVeinPatches), nameof(EVeinTypeToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        // need update with preloader
        public static uint EVeinTypeToAnimDataState(byte type)
        {
            if (type >= 15)
                switch (type)
                {
                    case 15:
                        return 5;

                    case 16:
                        return 11;

                    case 17:
                        return 6; 
                    
                    case 18:
                        return 14;
                }

            return type;
        }
    }
}
