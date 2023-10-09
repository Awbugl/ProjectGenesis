using System;
using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Compatibility;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Utils
{
    internal static class PlanetThemeUtils
    {
        private static readonly Dictionary<int, int[]> PlanetGasData = new Dictionary<int, int[]>
                                                                       {
                                                                           { 1, new[] { 6220, 7019 } },
                                                                           { 6, new[] { 6206 } },
                                                                           { 7, new[] { 6220 } },
                                                                           { 8, new[] { 6220, 7019 } },
                                                                           { 9, new[] { 6206, 6220 } },
                                                                           { 10, new[] { 6220 } },
                                                                           { 12, new[] { 6206 } },
                                                                           { 13, new[] { 6206, 6220 } },
                                                                           { 14, new[] { 6220, 7019 } },
                                                                           { 15, new[] { 6220, 7019 } },
                                                                           { 16, new[] { 6220, 7019 } },
                                                                           { 17, new[] { 6220 } },
                                                                           { 18, new[] { 6220, 7019 } },
                                                                           { 19, new[] { 6220 } },
                                                                           { 20, new[] { 6220, 7002 } },
                                                                           { 22, new[] { 6220, 7019 } },
                                                                           { 23, new[] { 6206 } },
                                                                           { 24, new[] { 6220, 6205 } },
                                                                           { 25, new[] { 6220, 7019 } }
                                                                       };

        private static readonly Dictionary<int, AddVeinData> PlanetAddRareVeinData = new Dictionary<int, AddVeinData>
                                                                                     {
                                                                                         {
                                                                                             1,
                                                                                             new AddVeinData(new[] { 8, 16 },
                                                                                                             new[]
                                                                                                             {
                                                                                                                 1.0f, 0.0f, 0.0f, 0.4f, 1.0f, 0.0f,
                                                                                                                 0.0f, 0.4f
                                                                                                             })
                                                                                         },
                                                                                         {
                                                                                             6,
                                                                                             new AddVeinData(new[] { 16 },
                                                                                                             new[] { 0.2f, 0.7f, 0.2f, 0.8f })
                                                                                         },
                                                                                         {
                                                                                             9,
                                                                                             new AddVeinData(new[] { 16, 17 },
                                                                                                             new[]
                                                                                                             {
                                                                                                                 0.2f, 0.5f, 0.3f, 0.7f,
                                                                                                                 0.2f, 0.6f, 0.3f, 0.8f
                                                                                                             })
                                                                                         },
                                                                                         {
                                                                                             12,
                                                                                             new AddVeinData(new[] { 17 },
                                                                                                             new[] { 0.2f, 0.6f, 0.3f, 0.8f })
                                                                                         },
                                                                                         {
                                                                                             17,
                                                                                             new AddVeinData(new[] { 16 },
                                                                                                             new[] { 0.2f, 0.6f, 0.2f, 0.8f })
                                                                                         },
                                                                                         {
                                                                                             23,
                                                                                             new AddVeinData(new[] { 17 },
                                                                                                             new[] { 0.2f, 0.6f, 0.5f, 0.8f })
                                                                                         },
                                                                                     };

        internal static void AdjustPlanetThemeDataVanilla()
        {
            foreach (ThemeProto theme in LDB.themes.dataArray) AdjustTheme(theme);

            ThemeProto gobi = LDB.themes.Select(12);
            gobi.WaterItemId = 7017;
            gobi.WaterHeight = -0.1f;
            gobi.Distribute = EThemeDistribute.Interstellar;
            gobi.oceanMat = LDB.themes.Select(22).oceanMat;
            RemoveVein(gobi, 0);
            RemoveVein(gobi, 1);
            RemoveVein(gobi, 2);
            RemoveVein(gobi, 3);
            RemoveVein(gobi, 14);

            ThemeProto volcanic = LDB.themes.Select(13);
            RemoveVein(volcanic, 0);
            RemoveVein(volcanic, 14);

            ThemeProto oceanicJungle = LDB.themes.Select(8);
            oceanicJungle.WaterItemId = 1000;
            oceanicJungle.Distribute = EThemeDistribute.Interstellar;

            ThemeProto rockySalt = LDB.themes.Select(17);
            rockySalt.WaterItemId = 7014;
            rockySalt.WaterHeight = -0.1f;
            rockySalt.Distribute = EThemeDistribute.Interstellar;
            rockySalt.Algos = new[] { 3 };
            rockySalt.oceanMat = LDB.themes.Select(8).oceanMat;
            RemoveVein(rockySalt, 0);
            RemoveVein(rockySalt, 3);
            RemoveVein(rockySalt, 14);

            ThemeProto ice = LDB.themes.Select(10);
            ice.WaterItemId = 7002;
            ice.Distribute = EThemeDistribute.Interstellar;
        }

        internal static void AdjustTheme(ThemeProto theme)
        {
            void TerrestrialAdjust()
            {
                AdjustGasItems(theme);
                AdjustVeins(theme);

                if (theme.WaterItemId == 1000) theme.WaterItemId = 7018;

                // for GalacticScale mod
                if (GalacticScaleCompatibilityPlugin.GalacticScaleInstalled)
                {
                    if (theme.name == "OceanicJungle") theme.WaterItemId = 1000;

                    if (theme.name.StartsWith("IceGelisol")) theme.WaterItemId = 7002;

                    if (theme.name == "SaltLake")
                    {
                        theme.WaterItemId = 7014;
                        theme.WaterHeight = -0.1f;
                        theme.Algos = new[] { 3 };
                        theme.oceanMat = LDB.themes.Select(8).oceanMat;
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 3);
                        RemoveVein(theme, 14);
                    }

                    if (theme.name == "Gobi")
                    {
                        theme.WaterItemId = 7017;
                        theme.WaterHeight = -0.1f;
                        theme.oceanMat = LDB.themes.Select(22).oceanMat;
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 1);
                        RemoveVein(theme, 2);
                        RemoveVein(theme, 3);
                        RemoveVein(theme, 14);
                    }
                }
            }

            void GasGiantAdjust()
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

            if (theme.PlanetType == EPlanetType.Gas)
                GasGiantAdjust();
            else
                TerrestrialAdjust();
        }

        private static void AdjustVeins(ThemeProto theme)
        {
            Array.Resize(ref theme.VeinSpot, 15);
            Array.Resize(ref theme.VeinCount, 15);
            Array.Resize(ref theme.VeinOpacity, 15);

            theme.VeinSpot[14] = (theme.VeinSpot[0] + theme.VeinSpot[1]) / 2;
            theme.VeinCount[14] = (theme.VeinCount[0] + theme.VeinCount[1]) / 2;
            theme.VeinOpacity[14] = (theme.VeinOpacity[0] + theme.VeinOpacity[1]) / 2;

            if (!theme.GasItems.Contains(7019))
            {
                RemoveVein(theme, 5);
            }
            else
            {
                theme.VeinSpot[5] += 1;
                theme.VeinCount[5] *= 1.1f;
            }

            if (PlanetAddRareVeinData.TryGetValue(theme.ID, out AddVeinData value))
            {
                theme.RareVeins = theme.RareVeins.Concat(value.RareVeins).ToArray();
                theme.RareSettings = theme.RareSettings.Concat(value.RareSettings).ToArray();
            }
        }

        private static void AdjustGasItems(ThemeProto theme)
        {
            if (theme.Wind == 0)
            {
                theme.GasItems = Array.Empty<int>();
                theme.GasSpeeds = Array.Empty<float>();
            }
            else if (PlanetGasData.TryGetValue(theme.ID, out int[] value))
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

        private static void RemoveVein(ThemeProto theme, int id)
        {
            theme.VeinSpot[id] = 0;
            theme.VeinCount[id] = 0f;
            theme.VeinOpacity[id] = 0f;
        }

        public struct AddVeinData
        {
            public int[] RareVeins;
            public float[] RareSettings;

            public AddVeinData(int[] rareVeins, float[] rareSettings)
            {
                RareVeins = rareVeins;
                RareSettings = rareSettings;
            }
        }
    }
}
