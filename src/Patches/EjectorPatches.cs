using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class EjectorPatches
    {
        [HarmonyPatch(typeof(EjectorComponent), nameof(EjectorComponent.InternalUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EjectorComponent_InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(DysonSwarm), nameof(DysonSwarm.AddBullet))));

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, (sbyte)6))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(EjectorPatches), nameof(Ejector_PatchMethod))))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            var matcher2 = matcher.Clone();

            matcher2.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EjectorComponent), nameof(EjectorComponent.coldSpend))),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(EjectorComponent), nameof(EjectorComponent.time))));

            matcher.SetJumpTo(OpCodes.Br, matcher2.Pos, out _);

            return matcher.InstructionEnumeration();
        }

        public static void Ejector_PatchMethod(DysonSwarm swarm, SailBullet sailBullet, ref EjectorComponent component,
            int[] consumeRegister)
        {
            int stationPilerLevel = GameMain.history.stationPilerLevel;
            ref int bulletCount = ref component.bulletCount;

            var count = stationPilerLevel > bulletCount ? bulletCount : stationPilerLevel;

            for (int i = 0; i < count; i++) swarm.AddBullet(sailBullet, component.runtimeOrbitId);

            int bulletInc = component.bulletInc / bulletCount;
            if (!component.incUsed) component.incUsed = bulletInc > 0;
            component.bulletInc -= bulletInc * count;
            bulletCount -= count;
            if (bulletCount == 0) component.bulletInc = 0;
            lock (consumeRegister) consumeRegister[component.bulletId] += count;
        }
    }
}
