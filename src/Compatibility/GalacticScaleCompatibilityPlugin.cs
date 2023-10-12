using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using GalacticScale;
using HarmonyLib;
using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.Logic.AddVein;
using static ProjectGenesis.Patches.Logic.AddVein.AdjustPlanetTheme;
using PluginInfo = BepInEx.PluginInfo;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(GalacticScaleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class GalacticScaleCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.GalacticScale";
        public const string MODNAME = "GenesisBook.Compatibility.GalacticScale";
        public const string VERSION = "1.0.0";

        private const string GalacticScaleGUID = "dsp.galactic-scale.2";

        internal static bool GalacticScaleInstalled;

        public void Awake()
        {
            GalacticScaleInstalled = Chainloader.PluginInfos.TryGetValue(GalacticScaleGUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;

            foreach (FieldInfo fi in AccessTools.GetDeclaredFields(assembly.GetType("GalacticScale.Themes")))
            {
                ref GSTheme theme = ref AccessTools.StaticFieldRefAccess<GSTheme>(fi)();

                GSThemeAdjust.AdjustTheme(ref theme);
            }

            GSVeinType.saneVeinTypes["Aluminum"] = (EVeinType)15;
            GSVeinType.saneVeinTypes["Radioactive"] = (EVeinType)16;
            GSVeinType.saneVeinTypes["Tungsten"] = (EVeinType)17;

            GSVeinType.insaneVeinTypes[(EVeinType)15] = "Aluminum";
            GSVeinType.insaneVeinTypes[(EVeinType)16] = "Radioactive";
            GSVeinType.insaneVeinTypes[(EVeinType)17] = "Tungsten";

            var harmony = new Harmony(MODGUID);

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.GS2"), "SetPlanetTheme"), null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(SetPlanetTheme_Postfix)),
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(SetPlanetTheme_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "DisableVeins"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(DisableVeins_Transpiler)));
            
            harmony.Patch(AccessTools.PropertyGetter(assembly.GetType("GalacticScale.GS2MainSettings"), "VeinTips"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(GS2MainSettings_VeinTips_Getter_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIPlanetDetail"), "OnPlanetDataSet7Prefix"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(OnPlanetDataSet_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIPlanetDetail"), "OnPlanetDataSet7Prefix"), null, null,
                          new HarmonyMethod(typeof(PlanetGasPatches), nameof(PlanetGasPatches.OnDataSet_ChangeWaterId_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIPlanetDetail"), "OnPlanetDataSet7Prefix"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(OnPlanetDataSet_ChangeVeinData_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIStarDetail"), "OnStarDataSet2"), null, null,
                          new HarmonyMethod(typeof(PlanetGasPatches), nameof(PlanetGasPatches.OnDataSet_ChangeWaterId_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIStarDetail"), "OnStarDataSet2"), null, null,
                          new HarmonyMethod(typeof(PlanetGasPatches), nameof(PlanetGasPatches.PlanetGen_SetPlanetTheme_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIStarDetail"), "OnStarDataSet2"), null, null,
                          new HarmonyMethod(typeof(PlanetGasPatches), nameof(PlanetGasPatches.OnStarDataSet_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.PatchOnUIStarDetail"), "OnStarDataSet2"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(OnStarDataSet_ChangeVeinData_Transpiler)));
        }

        public static IEnumerable<CodeInstruction> SetPlanetTheme_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher
                = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "type")),
                                                             new CodeMatch(OpCodes.Ldc_I4_5));
            matcher.Advance(-1);
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
            return matcher.InstructionEnumeration();
        }

        public static void SetPlanetTheme_Postfix(PlanetData planet)
        {
            if (planet.type != EPlanetType.Gas)
            {
                planet.gasHeatValues = new float[planet.gasItems.Length];
                planet.gasTotalHeat = 0;
            }
        }

        public static IEnumerable<CodeInstruction> DisableVeins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));

            matcher.SetOperandAndAdvance((sbyte)18);

            return matcher.InstructionEnumeration();
        }
        
        public static IEnumerable<CodeInstruction> GS2MainSettings_VeinTips_Getter_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16));

            matcher.SetOperandAndAdvance((sbyte)20);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16));

            matcher.SetOperandAndAdvance((sbyte)20);

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> OnPlanetDataSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher
                = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "type")),
                                                             new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(OpCodes.Ceq),
                                                             new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Ceq),
                                                             new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                                             new CodeMatch(OpCodes.Brfalse));

            object label = matcher.Operand;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "实际采集速度"));

            matcher.Advance(-14).SetOperandAndAdvance(label);
            matcher.Advance(39).SetOperandAndAdvance(label);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "collectionPerTick")));

            CodeInstruction stationComponent = matcher.Advance(-1).Instruction;

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(stationComponent), new CodeInstruction(OpCodes.Ldarg_0),
                                                 new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIPlanetDetail), "_planet")),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(PlanetGasPatches),
                                                                                        nameof(PlanetGasPatches.GetGasCollectionPerTick))));

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> OnPlanetDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_6), new CodeMatch(OpCodes.Clt));

            object index = matcher.Operand;
            object jump = matcher.Advance(5).Operand;
            object endlabel = matcher.Advance(2).Labels.First();

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index), new CodeInstruction(OpCodes.Ldc_I4, 15),
                                     new CodeInstruction(OpCodes.Beq, endlabel), new CodeInstruction(OpCodes.Ldc_I4, 14),
                                     new CodeInstruction(OpCodes.Stloc_S, index), new CodeInstruction(OpCodes.Br, jump));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Clt));

            index = matcher.Operand;

            object addLabel = matcher.Clone().MatchBack(false, new CodeMatch(OpCodes.Brfalse)).Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Beq, addLabel), new CodeInstruction(OpCodes.Ldloc_S, index),
                                                new CodeInstruction(OpCodes.Ldc_I4, 18));

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> OnStarDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7), new CodeMatch(OpCodes.Clt));

            matcher.Advance(1)
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(AddVeinPatches),
                                                                                    nameof(AddVeinPatches
                                                                                              .OnStarDataSet_ChangeVeinData_HighlightPatches))))
                   .SetOpcodeAndAdvance(OpCodes.Nop);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7), new CodeMatch(OpCodes.Clt));

            matcher.Advance(1)
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(AddVeinPatches),
                                                                                    nameof(AddVeinPatches
                                                                                              .OnStarDataSet_ChangeVeinData_HighlightPatches))))
                   .SetOpcodeAndAdvance(OpCodes.Nop);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Clt));

            object index = matcher.Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index),
                                     new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(AddVeinPatches),
                                                                            nameof(AddVeinPatches.OnStarDataSet_ChangeVeinData_IndexPatches))),
                                     new CodeInstruction(OpCodes.Stloc_S, index));

            matcher.Advance(1).SetOperandAndAdvance(18);

            return matcher.InstructionEnumeration();
        }
    }

    public static class GSThemeAdjust
    {
        internal static void AdjustTheme(ref GSTheme theme)
        {
            if (theme.PlanetType == EPlanetType.Gas)
            {
                GasGiantAdjust(ref theme);
            }
            else
            {
                AdjustGasItems(ref theme);
                AdjustVeins(ref theme);

                if (theme.WaterItemId == 1000) theme.WaterItemId = 7018;

                switch (theme.LDBThemeId)
                {
                    case 8:
                        theme.WaterItemId = 1000;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        break;

                    case 12:
                        theme.WaterItemId = 7017;
                        theme.WaterHeight = -0.1f;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        Themes.Savanna.InitMaterials();
                        theme.oceanMat = Themes.Savanna.oceanMat;
                        RemoveVein(ref theme, 0);
                        RemoveVein(ref theme, 1);
                        RemoveVein(ref theme, 2);
                        RemoveVein(ref theme, 3);
                        RemoveVein(ref theme, 14);
                        break;

                    case 13:
                        RemoveVein(ref theme, 0);
                        RemoveVein(ref theme, 14);
                        break;

                    case 17:
                        theme.WaterItemId = 7014;
                        theme.WaterHeight = -0.1f;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        theme.Algo = 3;
                        theme.oceanMat = Themes.OceanicJungle.oceanMat;
                        RemoveVein(ref theme, 0);
                        RemoveVein(ref theme, 3);
                        RemoveVein(ref theme, 14);
                        break;
                }
            }
        }

        private static void GasGiantAdjust(ref GSTheme theme)
        {
            if (theme.GasItems.Length != 2) return;

            if (theme.GasItems[0] == 1011 && theme.GasItems[1] == 1120)
            {
                theme.GasItems = new[] { 1011, 1120, 7002 };
                theme.GasSpeeds = new float[] { theme.GasSpeeds[0], theme.GasSpeeds[1], theme.GasSpeeds[1] * 0.7f };
            }
            else if (theme.GasItems[0] == 1120 && theme.GasItems[1] == 1121)
            {
                theme.GasItems = new[] { 1120, 1121, 6234 };
                theme.GasSpeeds = new float[] { theme.GasSpeeds[0], theme.GasSpeeds[1], theme.GasSpeeds[1] * 0.5f };
            }
        }

        private static void AdjustVeins(ref GSTheme theme)
        {
            Array.Resize(ref theme.VeinSpot, 15);
            Array.Resize(ref theme.VeinCount, 15);
            Array.Resize(ref theme.VeinOpacity, 15);

            theme.VeinSpot[14] = (theme.VeinSpot[0] + theme.VeinSpot[1]) / 2;
            theme.VeinCount[14] = (theme.VeinCount[0] + theme.VeinCount[1]) / 2;
            theme.VeinOpacity[14] = (theme.VeinOpacity[0] + theme.VeinOpacity[1]) / 2;

            if (!theme.GasItems.Contains(7019))
            {
                RemoveVein(ref theme, 5);
            }
            else
            {
                theme.VeinSpot[5] += 1;
                theme.VeinCount[5] *= 1.1f;
            }

            if (PlanetAddRareVeinData.TryGetValue(theme.LDBThemeId, out AddVeinData value))
            {
                theme.RareVeins = theme.RareVeins.Concat(value.RareVeins).ToArray();
                theme.RareSettings = theme.RareSettings.Concat(value.RareSettings).ToArray();
            }
        }

        private static void AdjustGasItems(ref GSTheme theme)
        {
            if (theme.Wind == 0)
            {
                theme.GasItems = Array.Empty<int>();
                theme.GasSpeeds = Array.Empty<float>();
            }
            else if (PlanetGasData.TryGetValue(theme.LDBThemeId, out int[] value))
            {
                theme.GasItems = value;
                theme.GasSpeeds = theme.GasItems.Length == 1
                                      ? new float[] { theme.Wind * 0.7f }
                                      : new float[] { theme.Wind * 0.7f, theme.Wind * 0.18f };
            }
            else if (theme.GasItems == null || theme.GasItems.Length == 0)
            {
                switch (theme.PlanetType)
                {
                    case EPlanetType.Ocean:
                        theme.GasItems = new[] { 6220, 7019 };
                        theme.GasSpeeds = new float[] { theme.Wind * 0.7f, theme.Wind * 0.18f };
                        break;

                    default:
                        theme.GasItems = new[] { 6206 };
                        theme.GasSpeeds = new float[] { theme.Wind * 0.7f };
                        break;
                }
            }
        }

        private static void RemoveVein(ref GSTheme theme, int id)
        {
            theme.VeinSpot[id] = 0;
            theme.VeinCount[id] = 0f;
            theme.VeinOpacity[id] = 0f;
        }
    }
}
