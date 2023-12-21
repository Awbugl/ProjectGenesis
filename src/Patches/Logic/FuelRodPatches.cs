using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class FuelRodPatches
    {
        [HarmonyPatch(typeof(Mecha), "GenerateEnergy")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetCargoBytes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_I4_M1), 
                                 new CodeMatch(OpCodes.Stloc_S));
            
            
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), 
                                                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(FuelRodPatches), nameof(GenerateEnergy_Patch))));

            return matcher.InstructionEnumeration();
        }

        public static void GenerateEnergy_Patch(Mecha mecha)
        {
            int count = 0;

            switch (mecha.reactorItemId)
            {
                case ProtoIDUsedByPatches.I氢燃料棒:
                case ProtoIDUsedByPatches.I煤油燃料棒:
                case ProtoIDUsedByPatches.I四氢双环戊二烯燃料棒:
                case ProtoIDUsedByPatches.I铀燃料棒:
                case ProtoIDUsedByPatches.I钚燃料棒:
                case ProtoIDUsedByPatches.I氘核燃料棒:
                case ProtoIDUsedByPatches.I氦三燃料棒:
                case ProtoIDUsedByPatches.I反物质燃料棒:
                    count = 1;
                    break;

                case ProtoIDUsedByPatches.I氘氦混合聚变燃料棒:
                    count = 2;
                    break;
                
                case ProtoIDUsedByPatches.IMOX燃料棒:
                    count = 3;
                    break;

                case ProtoIDUsedByPatches.I奇异燃料棒:
                    count = 4;
                    break;
            }
            
            if (count == 0) return;
            
            mecha.player.TryAddItemToPackage(ProtoIDUsedByPatches.I空燃料棒, count, mecha.reactorItemInc, true);
            UIItemup.Up(ProtoIDUsedByPatches.I空燃料棒, count);
        }
    }
}
