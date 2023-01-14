using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Utils
{
    internal static class PlanetThemeUtils
    {
        private static readonly Dictionary<int, int[]> PlanetGasData = new Dictionary<int, int[]>()
                                                                       {
                                                                           { 1, new[] { 6220, 7019 } },
                                                                           { 6, new[] { 6206 } },
                                                                           { 7, new[] { 6220 } },
                                                                           { 8, new[] { 6220, 7019 } },
                                                                           { 9, new[] { 6206, 6220 } },
                                                                           { 10, new[] { 6220, 7019 } },
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
                                                                           { 25, new[] { 6220, 7019 } },
                                                                       };

        internal static void AdjustPlanetThemeData()
        {
            LDB.themes.Select(8).WaterItemId = 7018;

            foreach (var theme in LDB.themes.dataArray)
            {
                if (theme.PlanetType == EPlanetType.Gas) continue;

                if (theme.Distribute == EThemeDistribute.Birth)
                {
                    theme.RareVeins = new[] { 8 };
                    theme.RareSettings = new float[] { 1.0f, 0.5f, 0.0f, 0.4f };
                }

                if (PlanetGasData.ContainsKey(theme.ID))
                {
                    theme.GasItems = PlanetGasData[theme.ID];
                    theme.GasSpeeds = theme.GasItems.Length == 1
                                          ? new float[] { theme.Wind * 0.7f }
                                          : new float[] { theme.Wind * 0.7f, theme.Wind * 0.18f };
                }
                else if (theme.Wind == 0 || theme.GasItems == null)
                {
                    theme.GasItems = Array.Empty<int>();
                    theme.GasSpeeds = Array.Empty<float>();
                }

                AdjustVeins(theme);
            }
        }

        private static void AdjustVeins(ThemeProto theme)
        {
            if (theme.VeinSpot.Length > 2)
            {
                ref var silicon = ref theme.VeinSpot[2];

                if (silicon > 0)
                {
                    silicon = 1 + silicon / 4;
                    theme.VeinCount[2] = 0.5f;
                    theme.VeinOpacity[2] = 0.5f;
                }
            }

            if (theme.VeinSpot.Length > 5)
            {
                ref var coal = ref theme.VeinSpot[5];

                if (!theme.GasItems.Contains(7019))
                {
                    coal = 0;
                    theme.VeinCount[5] = 0f;
                    theme.VeinOpacity[5] = 0f;
                }
                else
                {
                    coal += 1;
                    theme.VeinCount[5] *= 1.25f;
                    theme.VeinOpacity[5] *= 1.25f;
                }
            }
        }
    }
}
