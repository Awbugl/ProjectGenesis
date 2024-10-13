using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace ProjectGenesis.Compatibility
{
    internal static class LazyOutposting
    {
        internal const string GUID = "eirshy.dsp.LazyOutposting";

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo)) return;
            
            Assembly assembly = pluginInfo.Instance.GetType().Assembly;

            var type = assembly.GetType("Eirshy.DSP.LazyOutposting.Components.DwarvenContract");
            
            HarmonyPatch.Patch(AccessTools.Method(type, 
                    "SetUp"), null, null,
                new HarmonyMethod(typeof(LazyOutposting), nameof(SetUp_Transpiler)));
        }

        public static IEnumerable<CodeInstruction> SetUp_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_1));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(LazyOutposting), nameof(Patch_Method_allEVT))));
            
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_2));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(LazyOutposting), nameof(Patch_Method_validTargets))));

            return matcher.InstructionEnumeration();
        }
        
        public static EVeinType[] Patch_Method_allEVT(EVeinType[] allEVT)
        {
            var list = allEVT.ToList();
            
            list.Add(EVeinType.Aluminum);
            list.Add(EVeinType.Radioactive);
            list.Add(EVeinType.Tungsten);
            list.Add(EVeinType.Sulfur);

            return list.ToArray();
        }

        public static List<EVeinType> Patch_Method_validTargets(List<EVeinType> validTargets)
        {
            validTargets.Add(EVeinType.Aluminum);
            validTargets.Add(EVeinType.Radioactive);
            validTargets.Add(EVeinType.Tungsten);
            validTargets.Add(EVeinType.Sulfur);

            return validTargets;
        }
    }
}