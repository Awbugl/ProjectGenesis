using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Patches.Logic.AddVein;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Compatibility
{
    internal static class PlanetVeinUtilization
    {
        internal const string GUID = "testpostpleaseignore.dsp.planet_vein_utilization";

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo)) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            Type type = assembly.GetType("PlanetVeinUtilization.PlanetVeinUtilization");
            Type veinTypeInfoType = type.GetNestedType("VeinTypeInfo");

            HarmonyPatch.Patch(AccessTools.Method(type, "UIPlanetDetail_RefreshDynamicProperties_Postfix"), null, null,
                new HarmonyMethod(typeof(PlanetVeinUtilization), nameof(RefreshDynamicProperties_Postfix_Transpiler)));

            HarmonyPatch.Patch(AccessTools.Method(type, "UIStarDetail_RefreshDynamicProperties_Postfix"), null, null,
                new HarmonyMethod(typeof(PlanetVeinUtilization), nameof(RefreshDynamicProperties_Postfix_Transpiler)));

            ref Array local1 = ref AccessTools.StaticFieldRefAccess<Array>(type, "planetVeinCount");
            ref Array local2 = ref AccessTools.StaticFieldRefAccess<Array>(type, "starVeinCount");

            sbyte veinTypeCount = AddVeinPatches.VeinTypeCount;

            local1 = Array.CreateInstance(veinTypeInfoType, veinTypeCount);

            for (var i = 0; i < veinTypeCount; i++) local1.SetValue(Activator.CreateInstance(veinTypeInfoType, null), i);

            local2 = Array.CreateInstance(veinTypeInfoType, veinTypeCount);

            for (var i = 0; i < veinTypeCount; i++) local2.SetValue(Activator.CreateInstance(veinTypeInfoType, null), i);
        }

        public static IEnumerable<CodeInstruction> RefreshDynamicProperties_Postfix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));

            matcher.SetOperandAndAdvance(AddVeinPatches.VeinTypeCount);

            return matcher.InstructionEnumeration();
        }
    }
}
