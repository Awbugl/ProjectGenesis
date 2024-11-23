using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    internal static class TankComponentPatches
    {
        [HarmonyPatch(typeof(TankComponent), nameof(TankComponent.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TankComponent_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            LocalBuilder local = ilGenerator.DeclareLocal(typeof(int));
            local.SetLocalSymInfo("currentOutputStack");

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CargoTraffic), nameof(CargoTraffic.TryInsertItemAtHead))));

            do
            {
                matcher.Advance(-19).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(TankComponentPatches), nameof(TankComponent_GameTick_Insert_Method))),
                    new CodeInstruction(OpCodes.Stloc_S, local.LocalIndex));

                matcher.Advance(5).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                    new CodeInstruction(OpCodes.Mul));

                matcher.Advance(11).SetInstruction(new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex));

                matcher.Advance(8).SetInstruction(new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex));

                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CargoTraffic), nameof(CargoTraffic.TryInsertItemAtHead))));
            }
            while (matcher.IsValid);

            return matcher.InstructionEnumeration();
        }

        public static int TankComponent_GameTick_Insert_Method(ref TankComponent component)
        {
            int componentFluidCount = component.fluidCount;
            int historyStationPilerLevel = GameMain.history.stationPilerLevel;

            return componentFluidCount < historyStationPilerLevel ? componentFluidCount : historyStationPilerLevel;
        }
    }
}
