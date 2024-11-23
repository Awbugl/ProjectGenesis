using System;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    internal static partial class ModifyPlanetTheme
    {
        internal static void ModifyPlanetThemeDataVanilla()
        {
            foreach (ThemeProto theme in LDB.themes.dataArray) ModifyThemeVanilla(theme);
        }

        private static void ModifyThemeVanilla(ThemeProto theme)
        {
            if (theme.PlanetType == EPlanetType.Gas) { GasGiantModify(theme); }
            else
            {
                ModifyThemeData(theme);

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
                        theme.Algos = new[] { 3, };
                        theme.oceanMat = LDB.themes.Select(8).oceanMat;
                        RemoveVein(theme, 0);
                        RemoveVein(theme, 14);

                        break;
                }
            }
        }

        private static void GasGiantModify(ThemeProto theme)
        {
            if (theme.GasItems.Length != 2) return;

            if (theme.GasItems[0] == ProtoID.I可燃冰 && theme.GasItems[1] == ProtoID.I氢)
            {
                theme.GasItems = new[] { ProtoID.I可燃冰, ProtoID.I氢, ProtoID.I氨, };
                theme.GasSpeeds = new float[] { theme.GasSpeeds[0], theme.GasSpeeds[1], theme.GasSpeeds[1] * 0.7f, };
            }
            else if (theme.GasItems[0] == ProtoID.I氢 && theme.GasItems[1] == ProtoID.I重氢)
            {
                theme.GasItems = new[] { ProtoID.I氢, ProtoID.I重氢, ProtoID.I氦, };
                theme.GasSpeeds = new float[] { theme.GasSpeeds[0], theme.GasSpeeds[1], theme.GasSpeeds[1] * 0.5f, };
            }
        }

        private static void ModifyThemeData(ThemeProto theme)
        {
            float themeWind = theme.Wind;

            if (ThemeDatas.TryGetValue(theme.ID, out ThemeData value))
            {
                if (themeWind == 0)
                {
                    theme.GasItems = Array.Empty<int>();
                    theme.GasSpeeds = Array.Empty<float>();
                }
                else
                {
                    theme.GasItems = value.GasItems;
                    theme.GasSpeeds = value.GasSpeedFactors.Select(factor => themeWind * factor).ToArray();
                }

                theme.RareVeins = theme.RareVeins.Concat(value.RareVeins).ToArray();
                theme.RareSettings = theme.RareSettings.Concat(value.RareSettings).ToArray();
            }
            else if (theme.GasItems == null || theme.GasItems.Length == 0)
            {
                switch (theme.PlanetType)
                {
                    case EPlanetType.Ocean:
                        theme.GasItems = new[] { ProtoID.I氮, ProtoID.I氧, };
                        theme.GasSpeeds = new float[] { themeWind * 0.7f, themeWind * 0.18f, };

                        break;

                    default:
                        theme.GasItems = new[] { ProtoID.I二氧化碳, };
                        theme.GasSpeeds = new float[] { themeWind * 0.8f, };

                        break;
                }
            }

            Array.Resize(ref theme.VeinSpot, 15);
            Array.Resize(ref theme.VeinCount, 15);
            Array.Resize(ref theme.VeinOpacity, 15);

            // Aluminum
            theme.VeinSpot[14] = (theme.VeinSpot[0] + theme.VeinSpot[1]) / 2;
            theme.VeinCount[14] = (theme.VeinCount[0] + theme.VeinCount[1]) / 2;
            theme.VeinOpacity[14] = (theme.VeinOpacity[0] + theme.VeinOpacity[1]) / 2;

            // Coal
            if (!theme.GasItems.Contains(ProtoID.I氧)) { RemoveVein(theme, 5); }
            else
            {
                theme.VeinSpot[5] += 1;
                theme.VeinCount[5] *= 1.1f;
            }
        }

        private static void RemoveVein(ThemeProto theme, int id)
        {
            theme.VeinSpot[id] = 0;
            theme.VeinCount[id] = 0f;
            theme.VeinOpacity[id] = 0f;
        }
    }
}
