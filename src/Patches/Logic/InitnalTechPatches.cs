using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class InitnalTechPatches
    {
        private static readonly List<int> InitnalTechs = new List<int>
                                                         {
                                                             1,
                                                             1001,
                                                             1901,
                                                             1902,
                                                             1903,
                                                             1415
                                                         },
                                          BonusTechs = new List<int> { 1801 },
                                          SandBoxNotUnlockTechs = new List<int> { 1835, 1513 };

        [HarmonyPatch(typeof(GameData), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            foreach (var tech in InitnalTechs) __instance.history.UnlockTech(tech);
            foreach (var tech in BonusTechs) __instance.history.UnlockTech(tech);
        }

        [HarmonyPatch(typeof(GameData), "Import")]
        [HarmonyPostfix]
        public static void Import(GameData __instance)
        {
            // ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

            foreach (var tech in InitnalTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (var tech in BonusTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (var (key, value) in __instance.history.techStates)
            {
                if (value.unlocked)
                {
                    var techProto = LDB.techs.Select(key);
                    if (techProto != null)
                        foreach (var t in techProto.UnlockRecipes)
                            __instance.history.UnlockRecipe(t);
                }
            }
        }

        [HarmonyPatch(typeof(UITechNode), "DoBuyoutTech")]
        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerEnter")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerExit")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerDown")]
        [HarmonyPatch(typeof(UITechNode), "OnOtherIconClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechNode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UITechNode), nameof(UITechNode.techProto))),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))), new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(id => InitnalTechs.Contains(id)));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse_S);
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UITechTree), "Do1KeyUnlock")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechTree_Do1KeyUnlock_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 1508));

            List<CodeInstruction> insts = matcher.InstructionsInRange(matcher.Pos - 2, matcher.Pos + 1);
            matcher.Advance(2);

            foreach (var tech in SandBoxNotUnlockTechs)
            {
                CodeInstruction[] codeInstructions = insts.ToArray();
                codeInstructions[2] = new CodeInstruction(codeInstructions[2].opcode, tech);
                matcher.InsertAndAdvance(codeInstructions);
            }

            return matcher.InstructionEnumeration();
        }
    }
}
