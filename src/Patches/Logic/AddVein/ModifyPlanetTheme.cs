﻿using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.Logic.AddVein
{
    internal static class ModifyPlanetTheme
    {
        internal static readonly Dictionary<int, int[]> PlanetGasData = new Dictionary<int, int[]>
                                                                        {
                                                                            { 1, new[] { 6220, 7019 } },
                                                                            { 6, new[] { 6206 } },
                                                                            { 7, new[] { 6220 } },
                                                                            { 8, new[] { 6220, 7019 } },
                                                                            { 9, new[] { 6206, 6220 } },
                                                                            { 10, new[] { 6220 } },
                                                                            { 12, new[] { 6206 } },
                                                                            { 13, new[] { 6206 } },
                                                                            { 14, new[] { 6220, 7019 } },
                                                                            { 15, new[] { 6220, 7019 } },
                                                                            { 16, new[] { 6220, 7019 } },
                                                                            { 17, new[] { 6220 } },
                                                                            { 18, new[] { 6220, 7019 } },
                                                                            { 19, new[] { 6220 } },
                                                                            { 20, new[] { 6220 } },
                                                                            { 22, new[] { 6220, 7019 } },
                                                                            { 23, new[] { 6206 } },
                                                                            { 24, new[] { 6220 } },
                                                                            { 25, new[] { 6220, 7019 } }
                                                                        };

        internal static readonly Dictionary<int, AddVeinData> PlanetAddRareVeinData = new Dictionary<int, AddVeinData>
                                                                                      {
                                                                                          {
                                                                                              1,
                                                                                              new AddVeinData(new[] { 8, 16, 18 },
                                                                                                              new[]
                                                                                                              {
                                                                                                                  1.0f, 1.0f, 0.0f, 0.4f, 1.0f, 1.0f,
                                                                                                                  0.0f, 0.4f, 1.0f, 1.0f, 1.0f, 0.3f
                                                                                                              })
                                                                                          },
                                                                                          {
                                                                                              6,
                                                                                              new AddVeinData(new[] { 16, 18 },
                                                                                                              new[]
                                                                                                              {
                                                                                                                  0.2f, 0.7f, 0.2f, 0.8f, 1.0f, 1.0f,
                                                                                                                  0.3f, 0.5f
                                                                                                              })
                                                                                          },
                                                                                          {
                                                                                              9,
                                                                                              new AddVeinData(new[] { 16, 17, 18 },
                                                                                                              new[]
                                                                                                              {
                                                                                                                  0.2f, 0.5f, 0.3f, 0.7f, 0.2f, 0.6f,
                                                                                                                  0.3f, 0.8f, 0.6f, 0.8f, 0.4f, 0.8f
                                                                                                              })
                                                                                          },
                                                                                          {
                                                                                              12,
                                                                                              new AddVeinData(new[] { 17 },
                                                                                                              new[] { 0.2f, 0.6f, 0.3f, 0.8f })
                                                                                          },
                                                                                          {
                                                                                              13,
                                                                                              new AddVeinData(new[] { 17, 18 },
                                                                                                              new[]
                                                                                                              {
                                                                                                                  0.2f, 0.6f, 0.3f, 0.8f, 1.0f, 1.0f,
                                                                                                                  0.4f, 0.8f
                                                                                                              })
                                                                                          },
                                                                                          {
                                                                                              16,
                                                                                              new AddVeinData(new[] { 18 },
                                                                                                              new[] { 0.2f, 0.6f, 0.2f, 0.3f })
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

        internal static void ModifyPlanetThemeDataVanilla()
        {
            foreach (ThemeProto theme in LDB.themes.dataArray) ModifyThemeVanilla(theme);
        }

        internal static void ModifyThemeVanilla(ThemeProto theme)
        {
            if (theme.PlanetType == EPlanetType.Gas)
            {
                GasGiantModify(theme);
            }
            else
            {
                ModifyGasItems(theme);
                ModifyVeins(theme);

                if (theme.WaterItemId == 1000) theme.WaterItemId = 7018;

                switch (theme.ID)
                {
                    case 8:
                        theme.WaterItemId = 1000;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        break;

                    case 12:
                        theme.WaterItemId = 7017;
                        theme.WaterHeight = -0.1f;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        theme.oceanMat = LDB.themes.Select(22).oceanMat;
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 1);
                        RemoveVein(theme, 2);
                        RemoveVein(theme, 3);
                        RemoveVein(theme, 14);
                        break;

                    case 13:
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 14);
                        break;

                    case 17:
                        theme.WaterItemId = 7014;
                        theme.WaterHeight = -0.1f;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        theme.Algos = new[] { 3 };
                        theme.oceanMat = LDB.themes.Select(8).oceanMat;
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 3);
                        RemoveVein(theme, 14);
                        break;

                    case 19:
                    case 25:
                        theme.Distribute = EThemeDistribute.Default;
                        break;
                }
            }
        }

        private static void GasGiantModify(ThemeProto theme)
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

        private static void ModifyVeins(ThemeProto theme)
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

        private static void ModifyGasItems(ThemeProto theme)
        {
            var rand = new DotNet35Random();

            float themeWind = theme.Wind;

            if (theme.ID == 12) themeWind = 1;

            if (themeWind == 0)
            {
                theme.GasItems = Array.Empty<int>();
                theme.GasSpeeds = Array.Empty<float>();
            }
            else if (PlanetGasData.TryGetValue(theme.ID, out int[] value))
            {
                theme.GasItems = value;
                theme.GasSpeeds = theme.GasItems.Length == 1 ? GasSpeedsOneItem() : GasSpeedsTwoItems();
            }
            else if (theme.GasItems == null || theme.GasItems.Length == 0)
            {
                switch (theme.PlanetType)
                {
                    case EPlanetType.Ocean:
                        theme.GasItems = new[] { 6220, 7019 };
                        theme.GasSpeeds = GasSpeedsTwoItems();
                        break;

                    default:
                        theme.GasItems = new[] { 6206 };
                        theme.GasSpeeds = GasSpeedsOneItem();
                        break;
                }
            }

            float[] GasSpeedsTwoItems()
                => new float[] { (float)(themeWind * (0.65f + rand.NextDouble() * 0.1f)), (float)(themeWind * (0.16f + rand.NextDouble() * 0.04f)) };

            float[] GasSpeedsOneItem() => new float[] { (float)(themeWind * (0.65f + rand.NextDouble() * 0.1f)) };
        }

        private static void RemoveVein(ThemeProto theme, int id)
        {
            theme.VeinSpot[id] = 0;
            theme.VeinCount[id] = 0f;
            theme.VeinOpacity[id] = 0f;
        }

        public struct AddVeinData
        {
            public readonly int[] RareVeins;
            public readonly float[] RareSettings;

            public AddVeinData(int[] rareVeins, float[] rareSettings)
            {
                RareVeins = rareVeins;
                RareSettings = rareSettings;
            }
        }
    }
}