using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        internal static void ModifyVeinData()
        {
            AddVeinProtos(NewVein(15, "铝矿脉", "I铝矿", "Assets/texpack/铝矿脉", 6202, 25),
                          NewVein(16, "放射性矿脉", "I放射性矿物", "Assets/texpack/放射晶体矿脉", 6222, 31, 4),
                          NewVein(17, "钨矿脉", "I钨矿", "Assets/texpack/钨矿脉", 6201, 34));

            VeinProto NewVein(
                int id,
                string name,
                string description,
                string iconPath,
                int miningItem,
                int miningEffect,
                int modelIndex = 1,
                int miningTime = 60)
                => new VeinProto()
                   {
                       ID = id,
                       Name = name,
                       Description = description,
                       IconPath = iconPath,
                       MiningItem = miningItem,
                       MiningEffect = miningEffect,
                       ModelIndex = modelIndex,
                       MiningTime = miningTime,
                       CircleRadius = 1,
                       MinerBaseModelIndex = 58,
                       MinerCircleModelIndex = 59,
                       MiningAudio = 122,
                       ModelCount = 1
                   };
        }

        private static void AddVeinProtos(params VeinProto[] protos)
        {
            VeinProtoSet veins = LDB.veins;

            int dataArrayLength = veins.dataArray.Length;

            Array.Resize(ref veins.dataArray, dataArrayLength + protos.Length);

            for (int index = 0; index < protos.Length; ++index)
            {
                veins.dataArray[dataArrayLength + index] = protos[index];
            }

            veins.OnAfterDeserialize();
        }

        [HarmonyPatch(typeof(UISandboxMenu), "StaticLoad")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISandboxMenu_StaticLoad_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(LDB), nameof(LDB.veins))),
                                 new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldlen));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)15))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetAlgorithm), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm7), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm11), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm12), "GenerateVeins")]
        [HarmonyPatch(typeof(PlanetAlgorithm13), "GenerateVeins")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15));
            matcher.SetOperandAndAdvance(18);

            return matcher.InstructionEnumeration();
        }
    }
}
