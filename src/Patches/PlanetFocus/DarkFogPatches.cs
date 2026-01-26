using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static partial class PlanetFocusPatches
    {
        private static readonly FieldInfo EvolveData_threatshr_Field = AccessTools.Field(typeof(EvolveData), nameof(EvolveData.threatshr));

        [HarmonyPatch(typeof(DFGBaseComponent), nameof(DFGBaseComponent.UpdateFactoryThreat))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void DFGBaseComponent_UpdateFactoryThreat_PreFix(DFGBaseComponent __instance, ref float power_threat_factor)
        {
            if (ContainsFocus(__instance.groundSystem.factory.planetId, 6531)) power_threat_factor *= 0.8f;
        }

        [HarmonyPatch(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.replicateSpeed), MethodType.Getter)]
        [HarmonyPostfix]
        public static void EnemyDFGroundSystem_replicateSpeed_Postfix(EnemyDFGroundSystem __instance, ref double __result)
        {
            if (ContainsFocus(__instance.factory.planetId, 6533)) __result *= 0.8f;
        }

        [HarmonyPatch(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.buildSpeed), MethodType.Getter)]
        [HarmonyPostfix]
        public static void EnemyDFGroundSystem_buildSpeed_Postfix(EnemyDFGroundSystem __instance, ref double __result)
        {
            if (ContainsFocus(__instance.factory.planetId, 6534)) __result *= 0.8f;
        }

        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.AddGroundEnemyHatred),
            new[] {
                typeof(DFGBaseComponent), typeof(EnemyData), typeof(ETargetType), typeof(int),
            }, new[] {
                ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal,
            })]
        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.AddGroundEnemyHatred),
            new[] {
                typeof(DFGBaseComponent), typeof(EnemyData), typeof(ETargetType), typeof(int), typeof(int),
            }, new[] {
                ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SkillSystem_AddGroundEnemyHatred_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldflda, EvolveData_threatshr_Field));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Add), new CodeMatch(OpCodes.Conv_I4), new CodeMatch(OpCodes.Call));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(threatshr_Method))));

            return matcher.InstructionEnumeration();
        }

        public static double threatshr_Method(double value, DFGBaseComponent component)
        {
            if (ContainsFocus(component.groundSystem.factory.planetId, 6532)) value *= 0.8f;

            return value;
        }
    }
}
