using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(MinerComponent), nameof(MinerComponent.InternalUpdate))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void MinerComponent_InternalUpdate_PreFix(PlanetFactory factory, ref float miningSpeed)
        {
            if (ContainsFocus(factory.planetId, 6528)) miningSpeed *= 1.25f;
        }

        [HarmonyPatch(typeof(UIMinerWindow), nameof(UIMinerWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIMinerWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.miningSpeedScale))));

                if (matcher.IsInvalid) break;

                matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIMinerWindow), nameof(UIMinerWindow.factory))));

                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetFocusPatches), nameof(MiningSpeedScale))));
            }

            return matcher.InstructionEnumeration();
        }

        public static float MiningSpeedScale(float scale, PlanetFactory factory)
        {
            bool exist = ContainsFocus(factory.planetId, 6528);

            return exist ? scale * 1.25f : scale;
        }

        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.UpdateCollection))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void StationComponent_UpdateCollection_Prefix(StationComponent __instance, PlanetFactory factory,
            ref float collectSpeedRate, int[] productRegister)
        {
            collectSpeedRate = GameMain.history.miningSpeedScale;

            if (ContainsFocus(factory.planetId, 6528)) collectSpeedRate *= 1.25f;
        }

        [HarmonyPatch(typeof(UIStationStorage), nameof(UIStationStorage.RefreshValues))]
        [HarmonyPostfix]
        public static void UIStationStorage_RefreshValues_Postfix(UIStationStorage __instance)
        {
            if (!__instance.station.isCollector) return;

            float miningSpeedScale = GameMain.history.miningSpeedScale;

            if (GameMain.localPlanet.type != EPlanetType.Gas && ContainsFocus(__instance.station.planetId, 6528)) miningSpeedScale *= 1.25f;

            __instance.speedText.text =
                $"{3600.0 * ((double)__instance.station.collectionPerTick[__instance.index] * miningSpeedScale):0.00}/min";
        }

        [HarmonyPatch(typeof(UIControlPanelStationStorage), nameof(UIControlPanelStationStorage.RefreshValues))]
        [HarmonyPostfix]
        public static void UIControlPanelStationStorage_RefreshValues_Postfix(UIControlPanelStationStorage __instance)
        {
            if (!__instance.station.isCollector) return;

            float miningSpeedScale = GameMain.history.miningSpeedScale;

            if (__instance.factory.planet.type != EPlanetType.Gas && ContainsFocus(__instance.station.planetId, 6528))
                miningSpeedScale *= 1.25f;

            __instance.speedText.text =
                $"{3600.0 * ((double)__instance.station.collectionPerTick[__instance.index] * miningSpeedScale):0.00}/min";
        }

        [HarmonyPatch(typeof(UIVeinCollectorPanel), nameof(UIVeinCollectorPanel._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIVeinCollectorPanel_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.miningSpeedScale))));

                if (matcher.IsInvalid) break;

                matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(UIVeinCollectorPanel), nameof(UIVeinCollectorPanel.factory))));

                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetFocusPatches), nameof(MiningSpeedScale))));
            }

            return matcher.InstructionEnumeration();
        }
    }
}
