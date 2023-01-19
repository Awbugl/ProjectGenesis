using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable once CommentTypo
// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.BeltColorFix
{
    /// <summary>
    ///     special thanks for https://github.com/kremnev8/DSP-Mods/blob/master/Mods/BetterMachines/src/BeltFixes.cs
    /// </summary>

    //Original author: xiaoye97, modified heavily.
    public static class BeltFixPatches
    {
        private static readonly FieldInfo BeltComponent_Speed_Field = AccessTools.Field(typeof(BeltComponent), nameof(BeltComponent.speed));

        [HarmonyPatch(typeof(CargoTraffic), "AlterBeltRenderer")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddColors(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_1),
                                                                     new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field),
                                                                     new CodeMatch(OpCodes.Ldc_I4_1));

            var matcher2 = matcher.Clone();
            matcher2.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Stloc_S));

            var arg = matcher2.Operand;
            matcher2.Advance(1);
            var label = matcher2.Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, arg))
                   .SetInstruction(Transpilers.EmitDelegate<Func<int, int, int>>((speed, other) =>
                    {
                        if (speed == 10) other += 8;
                        if (speed == 5) other += 4;
                        return other;
                    })).Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, arg)).SetInstruction(new CodeInstruction(OpCodes.Br, label));


            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CargoTraffic), "SetBeltSelected")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CargoTraffic_SetBeltSelected_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldelema),
                                                                     new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field),
                                                                     new CodeMatch(OpCodes.Call));

            matcher.Advance(2).InsertAndAdvance(Transpilers.EmitDelegate<Func<int, int>>((speed) =>
            {
                switch (speed)
                {
                    case 10:
                        return 5;

                    case 5:
                        return 2;

                    case 3:
                        return 1;

                    default:
                        return speed;
                }
            }));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltMajorPoint")]
        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltPoint")]
        [HarmonyPatch(typeof(ConnGizmoRenderer), "AddBlueprintBeltConn")]
        [HarmonyPrefix]
        public static void GizmoColor(ref ConnGizmoRenderer __instance, ref uint color)
        {
            switch (color)
            {
                case 10:
                    color = 5;
                    break;

                case 5:
                    color = 2;
                    break;

                case 3:
                    color = 1;
                    break;
            }
        }

        [HarmonyPatch(typeof(ConnGizmoRenderer), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GizmoColorUpdate(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions)
                         .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                                       new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ConnGizmoRenderer), nameof(ConnGizmoRenderer.factory))),
                                       new CodeMatch(OpCodes.Ldloc_3)).Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 6))
                         .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0))
                         .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<int, ConnGizmoObj>>((int speed, ref ConnGizmoObj renderer) =>
                          {
                              switch (speed)
                              {
                                  case 5:
                                      renderer.color = 2;
                                      break;

                                  case 3:
                                      renderer.color = 1;
                                      break;
                              }
                          }));

            return matcher.InstructionEnumeration();
        }

        private delegate void RefAction<in T1, T2>(T1 arg1, ref T2 arg2);
    }
}
