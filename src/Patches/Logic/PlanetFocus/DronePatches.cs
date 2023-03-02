using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(StationComponent), "InternalTickLocal")]
        [HarmonyPrefix]
        public static void StationComponent_InternalTickLocal_PreFix(PlanetFactory factory, ref float droneSpeed)
        {
            if (ContainsFocus(factory.planetId, 6530)) droneSpeed *= 1.25f;
        }

        [HarmonyPatch(typeof(DispenserComponent), "InternalTick")]
        [HarmonyPrefix]
        public static void DispenserComponent_InternalTick_PreFix(PlanetFactory factory, ref float courierSpeed)
        {
            if (ContainsFocus(factory.planetId, 6530)) courierSpeed *= 1.25f;
        }
    }
}
