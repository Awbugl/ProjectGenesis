using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class FastTravelPatches
    {
        [HarmonyPatch(typeof(UIStarmap), nameof(UIStarmap.UpdateCursorView))]
        [HarmonyPatch(typeof(UIStarmap), nameof(UIStarmap.StartFastTravelToPlanet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarmap_UpdateCursorView_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (FastTravelEnabler.Installed) return instructions;

            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameMain), nameof(GameMain.sandboxToolsEnabled))));
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(FastTravelPatches), nameof(IsFastTravelTechUnlocked))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarmap), nameof(UIStarmap.OnFastTravelButtonClick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarmap_FastTravel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (FastTravelEnabler.Installed) return instructions;

            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameMain), nameof(GameMain.sandboxToolsEnabled))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(FastTravelPatches), nameof(IsFastTravelEnabled))));

            return matcher.InstructionEnumeration();
        }

        public static bool IsFastTravelTechUnlocked(bool sandboxToolsEnabled) =>
            sandboxToolsEnabled || GameMain.history.TechUnlocked(ProtoID.T量子折跃);

        public static bool IsFastTravelEnabled(bool sandboxToolsEnabled)
        {
            if (sandboxToolsEnabled) return true;

            if (!GameMain.history.TechUnlocked(ProtoID.T量子折跃))
            {
                UIRealtimeTip.Popup("未解锁量子折跃".TranslateFromJson());

                return false;
            }

            Player player = GameMain.mainPlayer;

            Mecha mecha = player.mecha;

            var energyWant = 12000000000;

            mecha.QueryEnergy(energyWant, out double energyGet, out _);

            if (energyGet < energyWant)
            {
                UIRealtimeTip.Popup("折跃能量不足".TranslateFromJson());

                return false;
            }

            if (!UseNegentropySingularity(player))
            {
                UIRealtimeTip.Popup("负熵奇点不足".TranslateFromJson());

                return false;
            }

            mecha.coreEnergy -= energyGet;
            mecha.MarkEnergyChange(9, -energyWant);

            return true;
        }

        private static bool UseNegentropySingularity(Player player)
        {
            int itemId = ProtoID.I负熵奇点;
            var itemCount = 1;

            player.TakeItemFromPlayer(ref itemId, ref itemCount, out _, true, null);

            if (itemId != ProtoID.I负熵奇点) return false;

            if (itemCount != 1) return false;

            player.mecha.AddConsumptionStat(itemId, itemCount, player.nearestFactory);

            return true;
        }
    }
}
