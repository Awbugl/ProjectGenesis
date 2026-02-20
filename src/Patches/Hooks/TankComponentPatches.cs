using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 修改储液罐的游戏逻辑，流体堆叠输出基于堆叠科技等级
    /// </summary>
    internal static class TankComponentPatches
    {
        [HarmonyPatch(typeof(TankComponent), nameof(TankComponent.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TankComponent_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            /*
                if (cargoTraffic.TryInsertItemAtHead(this.belt0, this.fluidId, (byte) 1, (byte) inc2))
                {
                    --this.fluidCount;
                    this.fluidInc -= inc2;
                }

                to

                if (cargoTraffic.TryInsertItemAtHead(this.belt0, this.fluidId, (byte) currentOutputStack, (byte) inc2 * currentOutputStack))
                {
                    this.fluidCount -= currentOutputStack;
                    this.fluidInc -= inc2;
                }
            */

            LocalBuilder local = ilGenerator.DeclareLocal(typeof(int));
            local.SetLocalSymInfo("currentOutputStack");
            var localIndex = local.LocalIndex;

            matcher.Start().InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(TankComponentPatches), nameof(TankComponent_GameTick_Insert_Method))),
                new CodeInstruction(OpCodes.Stloc_S, localIndex));

            while (true)
            {
                /*

                    IL_01e4: ldarg.0      // this
                    IL_01e5: ldfld        int32 TankComponent::fluidInc
                    IL_01ea: ldarg.0      // this
                    IL_01eb: ldfld        int32 TankComponent::fluidCount
                    IL_01f0: div
                    IL_01f1: br.s         IL_01f4
                    IL_01f3: ldc.i4.0
                    IL_01f4: stloc.s      inc3
                 */

                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TankComponent), nameof(TankComponent.fluidCount))),
                    new CodeMatch(OpCodes.Div));

                if (matcher.IsInvalid) break;

                // tankComponent.fluidInc *= currentOutputStack;
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, localIndex), new CodeInstruction(OpCodes.Mul));
            }

            matcher.Start();

            // replace all Ldc_I4_1 to currentOutputStack
            while (true)
            {
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1));

                if (matcher.IsInvalid) break;

                matcher.SetAndAdvance(OpCodes.Ldloc_S, localIndex);
            }

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
