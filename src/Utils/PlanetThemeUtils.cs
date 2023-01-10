using System;

namespace ProjectGenesis.Utils
{
    internal static class PlanetThemeUtils
    {
        internal static void AdjustPlanetThemeData()
        {
            LDB.themes.Select(8).WaterItemId = 7018;
            LDB.themes.Select(12).WaterItemId = 7017;
            LDB.themes.Select(12).WaterHeight = LDB.themes.Select(22).WaterHeight + 0.6f;

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var theme in LDB.themes.dataArray)
            {
                if (theme.ID == 1)
                {
                    theme.RareVeins = new[] { 8 };
                    theme.RareSettings = new float[] { 1.0f, 0.5f, 0.0f, 0.4f };
                    theme.GasItems = new[] { 7019 };
                    theme.GasSpeeds = new[] { 1f };
                }
                
                if (theme.GasItems == null) theme.GasItems = Array.Empty<int>();
                if (theme.GasSpeeds == null) theme.GasSpeeds = Array.Empty<float>();

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
            }
        }
    }
}
