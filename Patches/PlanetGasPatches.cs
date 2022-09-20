using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static class PlanetGasPatches
    {
        private static readonly KeyValuePair<int, int> PlanetGasMap = new KeyValuePair<int, int>(1121, 7019);

        private static void ReplacePlanetGas(GalaxyData galaxy)
        {
            //遍历星系 替换气态成分
            foreach (var planetData in galaxy.stars.SelectMany(star => star.planets.Where(planetData => planetData.type == EPlanetType.Gas)))
            {
                for (var k = 0; k < planetData.gasItems.Length; ++k)
                    if (PlanetGasMap.Key == planetData.gasItems[k]) planetData.gasItems[k] = PlanetGasMap.Value;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "Import")]
        public static void GameData_Import(ref GameData __instance, BinaryReader r) => ReplacePlanetGas(__instance.galaxy);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "NewGame")]
        public static void GameData_NewGame(ref GameData __instance, GameDesc _gameDesc) => ReplacePlanetGas(__instance.galaxy);
    }
}
