using System;
using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Utils;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.Logic.AddVein
{
    internal static class ModifyPlanetTheme
    {
        internal static readonly Dictionary<int, int[]> PlanetGasData = new Dictionary<int, int[]>
                                                                        {
                                                                            { 1, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 6, new[] { ProtoID.I二氧化碳 } },
                                                                            { 7, new[] { ProtoID.I氮 } },
                                                                            { 8, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 9, new[] { ProtoID.I二氧化碳, ProtoID.I氮 } },
                                                                            { 10, new[] { ProtoID.I氮 } },
                                                                            { 12, new[] { ProtoID.I二氧化碳 } },
                                                                            { 13, new[] { ProtoID.I二氧化碳 } },
                                                                            { 14, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 15, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 16, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 17, new[] { ProtoID.I氮 } },
                                                                            { 18, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 19, new[] { ProtoID.I氮 } },
                                                                            { 20, new[] { ProtoID.I氮 } },
                                                                            { 22, new[] { ProtoID.I氮, ProtoID.I氧 } },
                                                                            { 23, new[] { ProtoID.I二氧化碳 } },
                                                                            { 24, new[] { ProtoID.I氮 } },
                                                                            { 25, new[] { ProtoID.I氮, ProtoID.I氧 } }
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

                if (theme.WaterItemId == ProtoID.I水) theme.WaterItemId = ProtoID.I海水;

                switch (theme.ID)
                {
                    case 8:
                        theme.WaterItemId = ProtoID.I水;
                        theme.Distribute = EThemeDistribute.Interstellar;
                        break;

                    case 12:
                        theme.WaterItemId = ProtoID.I硝酸;
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
                        theme.WaterItemId = ProtoID.I盐酸;
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

            if (theme.GasItems[0] == ProtoID.I可燃冰 && theme.GasItems[1] == ProtoID.I氢)
            {
                theme.GasItems = new[] { ProtoID.I可燃冰, ProtoID.I氢, ProtoID.I氨 };
                theme.GasSpeeds = new float[] { theme.GasSpeeds[0], theme.GasSpeeds[1], theme.GasSpeeds[1] * 0.7f };
            }
            else if (theme.GasItems[0] == ProtoID.I氢 && theme.GasItems[1] == ProtoID.I重氢)
            {
                theme.GasItems = new[] { ProtoID.I氢, ProtoID.I重氢, ProtoID.I氦 };
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

            if (!theme.GasItems.Contains(ProtoID.I氧))
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
                        theme.GasItems = new[] { ProtoID.I氮, ProtoID.I氧 };
                        theme.GasSpeeds = GasSpeedsTwoItems();
                        break;

                    default:
                        theme.GasItems = new[] { ProtoID.I二氧化碳 };
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
