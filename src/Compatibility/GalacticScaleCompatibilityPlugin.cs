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
using UnityEngine;
using static ProjectGenesis.Patches.Logic.AddVein.AdjustPlanetTheme;
using PluginInfo = BepInEx.PluginInfo;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(GalacticScaleGUID)]
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

            if (!GalacticScaleInstalled || pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;

            foreach (FieldInfo fi in AccessTools.GetDeclaredFields(assembly.GetType("GalacticScale.Themes")))
            {
                ref GSTheme theme = ref AccessTools.StaticFieldRefAccess<GSTheme>(fi)();

                GSThemeAdjust.AdjustTheme(ref theme);
            }

            GSVeinType.saneVeinTypes["Aluminum"] = (EVeinType)15;
            GSVeinType.saneVeinTypes["Radioactive"] = (EVeinType)16;
            GSVeinType.saneVeinTypes["Tungsten"] = (EVeinType)17;
            GSVeinType.saneVeinTypes["Sulfur"] = (EVeinType)18;

            GSVeinType.insaneVeinTypes[(EVeinType)15] = "Aluminum";
            GSVeinType.insaneVeinTypes[(EVeinType)16] = "Radioactive";
            GSVeinType.insaneVeinTypes[(EVeinType)17] = "Tungsten";
            GSVeinType.insaneVeinTypes[(EVeinType)18] = "Sulfur";

            var harmony = new Harmony(MODGUID);

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.GS2"), "SetPlanetTheme"), null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(SetPlanetTheme_Postfix)),
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(SetPlanetTheme_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.GSTheme"), "ToProto"), null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(GSTheme_ToProto_Postfix)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "DisableVeins"), null, null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(DisableVeins_Transpiler)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "DistributeVeinTypes"),
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(DistributeVeinTypes_Prefix)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "GenBirthPoints"), null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(GenBirthPoints_Postfix)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "CalculateVectorsGS2"), null,
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(CalculateVectorsGS2_Postfix)));

            harmony.Patch(AccessTools.Method(assembly.GetType("GalacticScale.VeinAlgorithms"), "InitBirthVeinVectors"),
                          new HarmonyMethod(typeof(GalacticScaleCompatibilityPlugin), nameof(InitBirthVeinVectors_Postfix)));

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

        public static void DistributeVeinTypes_Prefix(GSPlanet gsPlanet)
        {
            if (GSSettings.BirthPlanet != gsPlanet) return;

            var vt1 = GSVeinType.Generate((EVeinType)16, 1, 1, 0.6f, 0.6f, 6, 6, false);
            var vt2 = GSVeinType.Generate(EVeinType.Fireice, 1, 1, 0.6f, 0.6f, 6, 6, false);

            List<EVeinType> eveinTypeList = gsPlanet.veinSettings.VeinTypes.Select(veinType => veinType.type).ToList();

            if (!eveinTypeList.Contains((EVeinType)16)) gsPlanet.veinSettings.VeinTypes.Add(vt1);

            if (!eveinTypeList.Contains(EVeinType.Fireice)) gsPlanet.veinSettings.VeinTypes.Add(vt2);

            for (int index = 0; index < gsPlanet.veinSettings.VeinTypes.Count; index++)
            {
                GSVeinType veinType = gsPlanet.veinSettings.VeinTypes[index];

                switch (veinType.type)
                {
                    case (EVeinType)16:
                        gsPlanet.veinSettings.VeinTypes[index] = vt1;
                        break;

                    case EVeinType.Fireice:
                        gsPlanet.veinSettings.VeinTypes[index] = vt2;
                        break;
                }
            }
        }

        public static void CalculateVectorsGS2_Postfix(GSPlanet gsPlanet, List<GSVeinDescriptor> __result)
        {
            if (gsPlanet.planetData.id == GSSettings.BirthPlanetId)
                __result.Add(new GSVeinDescriptor
                             {
                                 count = 6,
                                 position = gsPlanet.planetData.birthResourcePoint2,
                                 rare = false,
                                 type = (EVeinType)15,
                                 richness = 0.1f
                             });
        }

        public static void GenBirthPoints_Postfix(GSPlanet gsPlanet)
        {
            VeinAlgorithms.random = new GS2.Random(GSSettings.Seed);
            PlanetData planetData = gsPlanet.planetData;
            double num1 = 85.0 / planetData.orbitalPeriod + planetData.orbitPhase / 360.0;
            int num2 = (int)(num1 + 0.1);
            double num3 = (num1 - num2) * (2.0 * Math.PI);
            double num4 = 85.0 / planetData.rotationPeriod + planetData.rotationPhase / 360.0;
            int num5 = (int)(num4 + 0.1);
            double angle = (num4 - num5) * 360.0;
            Vector3 v = new Vector3((float)Math.Cos(num3) * planetData.orbitRadius, 0.0f, (float)Math.Sin(num3) * planetData.orbitRadius);
            Vector3 position = Maths.QRotate(planetData.runtimeOrbitRotation, v);
            Pose pose;
            if (planetData.orbitAroundPlanet != null)
            {
                pose = planetData.orbitAroundPlanet.PredictPose(85.0);
                position.x += pose.position.x;
                position.y += pose.position.y;
                position.z += pose.position.z;
            }

            pose = new Pose(position, planetData.runtimeSystemRotation * Quaternion.AngleAxis((float)angle, Vector3.down));
            Vector3 vector3_1 = Maths.QInvRotateLF(pose.rotation, planetData.star.uPosition - (VectorLF3)pose.position * 40000.0);
            vector3_1.Normalize();
            Vector3 vector3_2 = Vector3.Cross(vector3_1, Vector3.up);
            Vector3 normalized1 = vector3_2.normalized;
            vector3_2 = Vector3.Cross(normalized1, vector3_1);
            Vector3 normalized2 = vector3_2.normalized;
            int num6 = 0;
            while (num6++ < 256)
            {
                float num7 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.5f;
                float num8 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.5f;
                Vector3 vector3_3 = vector3_1 + num7 * normalized1 + num8 * normalized2;
                vector3_3.Normalize();
                planetData.birthPoint = vector3_3 * (float)(planetData.realRadius + 0.20000000298023224 + 1.5800000429153442);
                vector3_2 = Vector3.Cross(vector3_3, Vector3.up);
                normalized1 = vector3_2.normalized;
                vector3_2 = Vector3.Cross(normalized1, vector3_3);
                normalized2 = vector3_2.normalized;
                bool flag = false;
                for (int index = 0; index < 10; ++index)
                {
                    Vector2 vector2_1 = new Vector2((float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0),
                                                    (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0)).normalized *
                                        0.1f;
                    Vector2 vector2_2 = AddVeinPatches.Rotate(vector2_1, 120);
                    float num9 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    float num10 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    vector2_2.x += num9;
                    vector2_2.y += num10;
                    Vector2 vector2_3 = AddVeinPatches.Rotate(vector2_2, 120);
                    float num51 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    float num61 = (float)(VeinAlgorithms.random.NextDouble() * 2.0 - 1.0) * 0.06f;
                    vector2_3.x += num51;
                    vector2_3.y += num61;
                    vector3_2 = vector3_3 + vector2_1.x * normalized1 + vector2_1.y * normalized2;
                    Vector3 normalized3 = vector3_2.normalized;
                    vector3_2 = vector3_3 + vector2_2.x * normalized1 + vector2_2.y * normalized2;
                    Vector3 normalized4 = vector3_2.normalized;
                    vector3_2 = vector3_3 + vector2_3.x * normalized1 + vector2_3.y * normalized2;
                    Vector3 normalized5 = vector3_2.normalized;
                    planetData.birthResourcePoint0 = normalized3.normalized;
                    planetData.birthResourcePoint1 = normalized4.normalized;
                    planetData.birthResourcePoint2 = normalized5.normalized;
                    float num11 = planetData.realRadius + 0.2f;
                    if (planetData.data.QueryHeight(vector3_3) > (double)num11 &&
                        planetData.data.QueryHeight(normalized3) > (double)num11 &&
                        planetData.data.QueryHeight(normalized4) > (double)num11)
                    {
                        Vector3 vpos1 = normalized3 + normalized1 * 0.03f;
                        Vector3 vpos2 = normalized3 - normalized1 * 0.03f;
                        Vector3 vpos3 = normalized3 + normalized2 * 0.03f;
                        Vector3 vpos4 = normalized3 - normalized2 * 0.03f;
                        Vector3 vpos5 = normalized4 + normalized1 * 0.03f;
                        Vector3 vpos6 = normalized4 - normalized1 * 0.03f;
                        Vector3 vpos7 = normalized4 + normalized2 * 0.03f;
                        Vector3 vpos8 = normalized4 - normalized2 * 0.03f;
                        Vector3 vpos9 = normalized5 + normalized1 * 0.03f;
                        Vector3 vpos10 = normalized5 - normalized1 * 0.03f;
                        Vector3 vpos11 = normalized5 + normalized2 * 0.03f;
                        Vector3 vpos12 = normalized5 - normalized2 * 0.03f;
                        if (planetData.data.QueryHeight(vpos1) > (double)num11 &&
                            planetData.data.QueryHeight(vpos2) > (double)num11 &&
                            planetData.data.QueryHeight(vpos3) > (double)num11 &&
                            planetData.data.QueryHeight(vpos4) > (double)num11 &&
                            planetData.data.QueryHeight(vpos5) > (double)num11 &&
                            planetData.data.QueryHeight(vpos6) > (double)num11 &&
                            planetData.data.QueryHeight(vpos7) > (double)num11 &&
                            planetData.data.QueryHeight(vpos8) > (double)num11 &&
                            planetData.data.QueryHeight(vpos9) > (double)num11 &&
                            planetData.data.QueryHeight(vpos10) > (double)num11 &&
                            planetData.data.QueryHeight(vpos11) > (double)num11 &&
                            planetData.data.QueryHeight(vpos12) > (double)num11)
                        {
                            flag = true;
                            break;
                        }
                    }
                }

                if (flag) break;
            }
        }

        public static void InitBirthVeinVectors_Postfix(GSPlanet gsPlanet)
        {
            PlanetData planetData = gsPlanet.planetData;
            gsPlanet.veinData.types[2] = (EVeinType)15;
            gsPlanet.veinData.vectors[2] = planetData.birthResourcePoint2;
            gsPlanet.veinData.count = 3;
        }

        public static void SetPlanetTheme_Postfix(PlanetData planet)
        {
            if (planet.type != EPlanetType.Gas)
            {
                planet.gasHeatValues = new float[planet.gasItems.Length];
                planet.gasTotalHeat = 0;
            }
        }

        public static void GSTheme_ToProto_Postfix(ThemeProto __result)
        {
            switch (__result.name)
            {
                case "SaltLake":
                    __result.oceanMat = LDB.themes.Select(8).oceanMat;
                    break;

                case "Gobi":
                    __result.oceanMat = LDB.themes.Select(22).oceanMat;
                    break;
            }
        }

        public static IEnumerable<CodeInstruction> GS2MainSettings_VeinTips_Getter_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));

            matcher.SetOperandAndAdvance(AddVeinPatches.VeinTypeCount);

            return matcher.InstructionEnumeration();
        }

        public static IEnumerable<CodeInstruction> DisableVeins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16));

            int veinTypeArrayLength = AddVeinPatches.VeinTypeCount + 1;

            matcher.SetOperandAndAdvance(veinTypeArrayLength);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16));

            matcher.SetOperandAndAdvance(veinTypeArrayLength);

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
                                                new CodeInstruction(OpCodes.Ldc_I4_S, AddVeinPatches.VeinTypeCount));

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

            matcher.Advance(1).SetOperandAndAdvance(AddVeinPatches.VeinTypeCount);

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
                        Themes.OceanicJungle.InitMaterials();
                        theme.oceanMat = Themes.OceanicJungle.oceanMat;
                        RemoveVein(ref theme, 0);
                        RemoveVein(ref theme, 3);
                        RemoveVein(ref theme, 14);
                        break;

                    case 19:
                    case 25:
                        theme.Distribute = EThemeDistribute.Default;
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
