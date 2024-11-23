using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

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

            var typeDwarvenContract = assembly.GetType("Eirshy.DSP.LazyOutposting.Components.DwarvenContract");
            var typeDwarfMission = typeDwarvenContract.GetNestedType("DwarfMission", BindingFlags.NonPublic | BindingFlags.Instance);
            var typeEGear = typeDwarfMission.GetNestedType("EGear", BindingFlags.NonPublic | BindingFlags.Instance);

            HarmonyPatch.Patch(AccessTools.Constructor(typeDwarfMission, new[] { typeEGear, }), null, null,
                new HarmonyMethod(typeof(LazyOutposting), nameof(DwarfMission_Constructor_Transpiler)));

            HarmonyPatch.CreateReversePatcher(AccessTools.Method(typeDwarvenContract, "SetUp"),
                new HarmonyMethod(typeof(LazyOutposting), nameof(SetUp_ReversePatch))).Patch();

            SetUp_ReversePatch();
        }

        public static IEnumerable<CodeInstruction> DwarfMission_Constructor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_2));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(LazyOutposting), nameof(Patch_Method_validTargets))));

            return matcher.InstructionEnumeration();
        }

        public static List<EVeinType> Patch_Method_validTargets(List<EVeinType> validTargets)
        {
            validTargets.Add(EVeinType.Aluminum);

            return validTargets;
        }

        public static void SetUp_ReversePatch()
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions);

                matcher.MatchForward(false, new CodeMatch(OpCodes.Call), new CodeMatch(OpCodes.Ldtoken));

                matcher.RemoveInstructions(matcher.Remaining - 1);

                return matcher.InstructionEnumeration();
            }

            _ = Transpiler(null);

            return;
        }
    }
}
