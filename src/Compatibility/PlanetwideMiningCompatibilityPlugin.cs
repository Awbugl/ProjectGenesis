using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Patches.Logic;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable once RedundantAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(PlanetwideMiningGUID)]
    public class PlanetwideMiningCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.PlanetwideMining";
        public const string MODNAME = "GenesisBook.Compatibility.PlanetwideMining";
        public const string VERSION = "1.0.0";

        private const string PlanetwideMiningGUID = "930f5bae-66d2-4917-988b-162fe2456643";

        public void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(PlanetwideMiningGUID, out var pluginInfo);

            if (pluginInfo == null) return;

            var assembly = pluginInfo.Instance.GetType().Assembly;
            var harmony = new Harmony(MODGUID);

            harmony.Patch(AccessTools.Method(assembly.GetType("PlanetwideMining.PatchMiners"), "CheckBuildConditions"), null, null,
                          new HarmonyMethod(typeof(PlanetwideMiningCompatibilityPlugin), nameof(BuildTool_Click_CheckBuildConditions_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("PlanetwideMining.PatchMiners"), "CheckBuildConditions"), null, null,
                          new HarmonyMethod(typeof(MegaPumpPatches), nameof(BuildTool_Click_CheckBuildConditions_Transpiler)));
        }

        private static readonly FieldInfo PrefabDesc_isStellarStation_Field = AccessTools.Field(typeof(PrefabDesc), "isStellarStation");

        public static IEnumerable<CodeInstruction> BuildTool_Click_CheckBuildConditions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), "desc")),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), "isCollectStation")));

            var preview = matcher.Operand;

            matcher.Advance(12);
            matcher.SetAndAdvance(OpCodes.Ldloc_S, preview);
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(PlanetGasPatches.IsSuit)));

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)36));
            matcher.Advance(-3).SetAndAdvance(OpCodes.Nop,null);
            matcher.SetOpcodeAndAdvance(matcher.Opcode == OpCodes.Brfalse_S ? OpCodes.Br_S : OpCodes.Br);

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 14297f));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool), "planet")),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(PlanetGasPatches),
                                                                                       nameof(PlanetGasPatches.GetDistance))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelem_Ref),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "isStellar")));

            // StationComponent comp = stationPool[i];
            List<CodeInstruction> ins = matcher.InstructionsWithOffsets(0, 2);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = buildPreview1.desc;
            List<CodeInstruction> ins2 = matcher.InstructionsWithOffsets(-2, -1);

            // set actual distance
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(ins).InsertAndAdvance(ins2)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(PlanetGasPatches), nameof(PlanetGasPatches.SetDistance))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = itemProto.desc;
            List<CodeInstruction> ins3 = matcher.InstructionsWithOffsets(-2, -1);

            matcher.Advance(1).MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = buildPreview1.desc;
            List<CodeInstruction> ins4 = matcher.InstructionsWithOffsets(-2, -1);

            // set actual distance
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(ins3).InsertAndAdvance(ins4)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(PlanetGasPatches), nameof(PlanetGasPatches.SetPreBuildDistance))));

            return matcher.InstructionEnumeration();
        }
    }
}
