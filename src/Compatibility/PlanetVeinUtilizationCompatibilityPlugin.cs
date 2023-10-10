using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(PlanetVeinUtilizationGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class PlanetVeinUtilizationCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.PlanetVeinUtilization";
        public const string MODNAME = "GenesisBook.Compatibility.PlanetVeinUtilization";
        public const string VERSION = "1.0.0";

        private const string PlanetVeinUtilizationGUID = "testpostpleaseignore.dsp.planet_vein_utilization";

        public void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(PlanetVeinUtilizationGUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            Type type = assembly.GetType("PlanetVeinUtilization.PlanetVeinUtilization");
            Type veinTypeInfoType = type.GetNestedType("VeinTypeInfo");

            var harmony = new Harmony(MODGUID);

            harmony.Patch(AccessTools.Method(type, "UIPlanetDetail_RefreshDynamicProperties_Postfix"), null, null,
                          new HarmonyMethod(typeof(PlanetVeinUtilizationCompatibilityPlugin), nameof(RefreshDynamicProperties_Postfix_Transpiler)));

            harmony.Patch(AccessTools.Method(type, "UIStarDetail_RefreshDynamicProperties_Postfix"), null, null,
                          new HarmonyMethod(typeof(PlanetVeinUtilizationCompatibilityPlugin), nameof(RefreshDynamicProperties_Postfix_Transpiler)));

            ref Array local1 = ref AccessTools.StaticFieldRefAccess<Array>(type, "planetVeinCount");
            ref Array local2 = ref AccessTools.StaticFieldRefAccess<Array>(type, "starVeinCount");

            local1 = Array.CreateInstance(veinTypeInfoType, 18);

            for (int i = 0; i < 18; i++)
            {
                local1.SetValue(Activator.CreateInstance(veinTypeInfoType, null), i);
            }

            local2 = Array.CreateInstance(veinTypeInfoType, 18);

            for (int i = 0; i < 18; i++)
            {
                local2.SetValue(Activator.CreateInstance(veinTypeInfoType, null), i);
            }
        }

        public static IEnumerable<CodeInstruction> RefreshDynamicProperties_Postfix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher
                = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));

            matcher.SetOperandAndAdvance((sbyte)18);

            return matcher.InstructionEnumeration();
        }
    }
}
