using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

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
                    this.fluidInc -= inc2 * currentOutputStack;
                }
            */

            LocalBuilder local = ilGenerator.DeclareLocal(typeof(int));
            local.SetLocalSymInfo("currentOutputStack");
            var localIndex = local.LocalIndex;

            while (true)
            {
                /*
                    IL_04ae: ldarg.0      // this
                    IL_04af: ldfld        int32 TankComponent::fluidInc
                    IL_04b4: brfalse.s    IL_04c5

                    IL_04b6: ldarg.0      // this
                    IL_04b7: ldfld        int32 TankComponent::fluidInc
                    IL_04bc: ldarg.0      // this
                    IL_04bd: ldfld        int32 TankComponent::fluidCount
                    IL_04c2: div
                    IL_04c3: br.s         IL_04c6
                    IL_04c5: ldc.i4.0
                    IL_04c6: stloc.s      inc5

                    // [302 7 - 302 94]
                    IL_04c8: ldloc.2      // cargoTraffic
                    IL_04c9: ldarg.0      // this
                    IL_04ca: ldfld        int32 TankComponent::belt3
                    IL_04cf: ldarg.0      // this
                    IL_04d0: ldfld        int32 TankComponent::fluidId
                    IL_04d5: ldc.i4.1
                    IL_04d6: ldloc.s      inc5
                    IL_04d8: conv.u1
                    IL_04d9: callvirt     instance bool CargoTraffic::TryInsertItemAtHead(int32, int32, unsigned int8, unsigned int8)
                    IL_04de: brfalse      IL_05df

                    // [304 7 - 304 24]
                    IL_04e3: ldarg.0      // this
                    IL_04e4: ldarg.0      // this
                    IL_04e5: ldfld        int32 TankComponent::fluidCount
                    IL_04ea: ldc.i4.1
                    IL_04eb: sub
                    IL_04ec: stfld        int32 TankComponent::fluidCount
                 */

                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TankComponent), nameof(TankComponent.fluidInc))),
                    CodeMatchUtils.BrFalse);

                if (matcher.IsInvalid) break;

                // calc currentOutputStack
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TankComponentPatches), nameof(CalcCurrentOutputStack))),
                    new CodeInstruction(OpCodes.Stloc_S, localIndex));

                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TankComponent), nameof(TankComponent.fluidCount))),
                    new CodeMatch(OpCodes.Div));

                // tankComponent.fluidInc * currentOutputStack;
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, localIndex), new CodeInstruction(OpCodes.Mul));

                matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TankComponent), nameof(TankComponent.fluidId))),
                    new CodeMatch(OpCodes.Ldc_I4_1));

                // stack = currentOutputStack;
                matcher.SetAndAdvance(OpCodes.Ldloc_S, localIndex);

                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Sub));

                // this.fluidCount -= currentOutputStack;
                matcher.SetAndAdvance(OpCodes.Ldloc_S, localIndex);
            }

            return matcher.InstructionEnumeration();
        }

        public static int CalcCurrentOutputStack(ref TankComponent component)
        {
            int componentFluidCount = component.fluidCount;
            int historyStationPilerLevel = GameMain.history.stationPilerLevel;

            return componentFluidCount < historyStationPilerLevel ? componentFluidCount : historyStationPilerLevel;
        }
    }
}
