using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.GoalDeterminator;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class GoalDeterminatorPatches
    {
        private static readonly Assembly Assembly = Assembly.GetAssembly(typeof(GDGB_GroupPlasmaControl));

        [HarmonyPatch(typeof(GoalLogic), nameof(GoalLogic.CreateDeterminator))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GoalLogic_CreateDeterminator_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                IL_001e: ldsfld       class [netstandard]System.Reflection.Assembly GoalDeterminator::asm
                IL_0023: ldloc.0      // goalProto
                IL_0024: ldfld        string GoalProto::DeterminatorName
                IL_0029: callvirt     instance object [netstandard]System.Reflection.Assembly::CreateInstance(string)
                IL_002e: isinst       GoalDeterminator
                IL_0033: stloc.1      // 'instance'

                IL_0034: ldloc.1      // 'instance'
                IL_0035: brfalse.s    IL_0052
            */

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_1), new CodeMatch(OpCodes.Ldloc_1), CodeMatchUtils.BrFalse);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0), // goalProto
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GoalDeterminatorPatches), nameof(CheckGoalDeterminator))));

            return matcher.InstructionEnumeration();
        }

        public static global::GoalDeterminator CheckGoalDeterminator(global::GoalDeterminator goalDeterminator, GoalProto goalProto)
        {
            if (goalDeterminator != null) return goalDeterminator;

            if (!goalProto.DeterminatorName.StartsWith("GDGB_")) return null;

            return Assembly.CreateInstance("ProjectGenesis.GoalDeterminator." + goalProto.DeterminatorName) as global::GoalDeterminator;
        }

        [HarmonyPatch(typeof(GD_Group), nameof(GD_Group.IsSubclass))]
        [HarmonyPatch(typeof(GD_Group), nameof(GD_Group.IsSameClassOrSubclass))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GD_Group_IsSubclass_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                IL_000d: ldsfld       class [netstandard]System.Reflection.Assembly GoalDeterminator::asm
                IL_0012: ldarg.0      // className
                IL_0013: callvirt     instance class [netstandard]System.Type [netstandard]System.Reflection.Assembly::GetType(string)
                IL_0018: stloc.0      // 'type'
            */

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_0));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), // className
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GoalDeterminatorPatches), nameof(CheckType))));

            return matcher.InstructionEnumeration();
        }

        public static Type CheckType(Type type, string className)
        {
            if (type != null) return type;

            return className.StartsWith("GDGB_") ? Assembly.GetType("ProjectGenesis.GoalDeterminator." + className) : null;
        }
    }
}
