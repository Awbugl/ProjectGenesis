using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Patches.UI
{
    public static class VeinColorPatches
    {
        [HarmonyPatch(typeof(PlanetFactory), "AddVeinData")]
        [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_AddVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.type))),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(VeinColorPatches), nameof(EVeinTypeToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnPlanetDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_6), new CodeMatch(OpCodes.Blt));

            object index = matcher.Operand;
            object jump = matcher.Advance(2).Operand;
            object endlabel = matcher.Advance(1).Labels.First();

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index), new CodeInstruction(OpCodes.Ldc_I4, 15),
                                     new CodeInstruction(OpCodes.Beq, endlabel), new CodeInstruction(OpCodes.Ldc_I4, 14),
                                     new CodeInstruction(OpCodes.Stloc_S, index), new CodeInstruction(OpCodes.Br, jump));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Blt));

            index = matcher.Operand;

            object addLabel = matcher.Clone().MatchBack(false, new CodeMatch(OpCodes.Bne_Un)).Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Beq, addLabel), new CodeInstruction(OpCodes.Ldloc_S, index),
                                                new CodeInstruction(OpCodes.Ldc_I4, 18));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnStarDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Blt));

            matcher.Advance(1)
                //  .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                 //                                        AccessTools.Method(typeof(VeinColorPatches),
                 //                                                           nameof(OnStarDataSet_ChangeVeinData_IndexPatches))));

        //    matcher
               .SetOperandAndAdvance(18);

            
            return matcher.InstructionEnumeration();
        }

        public static int OnStarDataSet_ChangeVeinData_IndexPatches(int index)
        {
            if (index == 7) return 15;

            if (index == 18) return 7;

            if (index == 15) return 18;

            return index;
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

        [HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        [HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RefreshDynamicProperties_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "未知珍奇信号"));

            CodeMatcher codeMatcher = matcher.Clone();
            object jmp = codeMatcher.Advance(-2).Operand;
            codeMatcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Dup), new CodeInstruction(OpCodes.Ldc_I4, 15),
                                                     new CodeInstruction(OpCodes.Beq, jmp));

            jmp = matcher.Advance(6).Operand;
            codeMatcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Dup), new CodeInstruction(OpCodes.Ldc_I4, 15),
                                                     new CodeInstruction(OpCodes.Beq, jmp));
            return matcher.InstructionEnumeration();
        }

        // update with preloader

        public static uint EVeinTypeToAnimDataState(EVeinType type)
        {
            if (type > EVeinType.Max)
                switch ((byte)type)
                {
                    case 15:
                        return 12;

                    case 16:
                        return 11;

                    case 17:
                        return 6;
                }

            return (uint)type;
        }

        internal static void ModifyVeinData()
        {
            RegisterNewVein(15, "铝矿", "I铝矿", "Assets/texpack/铝矿脉", 6202, 32);

            RegisterNewVein(16, "放射性矿物", "I放射性矿物碎块", "Assets/texpack/放射晶体矿脉", 6222, 31, 4);

            RegisterNewVein(17, "钨矿", "I钨矿", "Assets/texpack/钨矿脉", 6201, 34);

            void RegisterNewVein(
                int id,
                string name,
                string description,
                string iconPath,
                int miningItem,
                int miningEffect,
                int modelIndex = 1,
                int miningTime = 60)
                => LDBTool.PreAddProto(new VeinProto()
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
                                       });
        }
    }
}
