using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class PlanetGasPatches
    {
        private static readonly FieldInfo StationComponent_IsCollector_Field = AccessTools.Field(typeof(StationComponent), "isCollector");
        private static readonly FieldInfo PlanetData_GasInfo_Field = AccessTools.Field(typeof(PlanetData), "gasItems");

        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIPlanetDetail_OnPlanetDataSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(true,
                                                                     new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "type")),
                                                                     new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(OpCodes.Beq));

            var label = matcher.Operand;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "实际采集速度"));

            matcher.Advance(-10).SetOperandAndAdvance(label);
            matcher.Advance(28).SetOperandAndAdvance(label);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        [HarmonyPatch(typeof(PlanetGen), "SetPlanetTheme")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetGen_SetPlanetTheme_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false,
                                                                     new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "type")),
                                                                     new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(OpCodes.Bne_Un));
            matcher.Advance(-1);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "DetermineActive")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DetermineActive")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditionsPrestage")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Blueprint_DetermineActive_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                     new CodeMatch(OpCodes.Ldfld,
                                                                                   AccessTools.Field(typeof(BuildTool), nameof(BuildTool.planet))),
                                                                     new CodeMatch(OpCodes.Ldfld, PlanetData_GasInfo_Field));

            matcher.SetOperandAndAdvance(AccessTools.Field(typeof(PlanetData), "type"));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_5));
            matcher.SetOpcodeAndAdvance(matcher.Opcode == OpCodes.Brfalse_S ? OpCodes.Bne_Un_S : OpCodes.Bne_Un);

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintCopyMode")]
        [HarmonyPatch(typeof(PlayerController), "OpenBlueprintPasteMode")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlayerController_OpenBlueprintMode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                     new CodeMatch(OpCodes.Ldfld,
                                                                                   AccessTools.Field(typeof(PlayerController), "gameData")),
                                                                     new CodeMatch(OpCodes.Callvirt,
                                                                                   AccessTools.PropertyGetter(typeof(GameData), "localPlanet")),
                                                                     new CodeMatch(OpCodes.Ldfld, PlanetData_GasInfo_Field));

            matcher.SetOperandAndAdvance(AccessTools.Field(typeof(PlanetData), "type"));

            var label = matcher.Operand;
            var isS = matcher.Opcode == OpCodes.Brfalse_S;

            matcher.SetAndAdvance(OpCodes.Ldc_I4_5, null);
            matcher.SetAndAdvance(isS ? OpCodes.Bne_Un_S : OpCodes.Bne_Un, label);

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_Click), "CheckBuildConditions")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Click_CheckBuildConditions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), "desc")),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), "isCollectStation")),
                                 new CodeMatch(OpCodes.Brfalse), new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool), "planet")), new CodeMatch(OpCodes.Brfalse));

            var preview = matcher.Operand;

            matcher.Advance(9);
            matcher.SetAndAdvance(OpCodes.Ldloc_S, preview);
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(IsSuit)));

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)36));
            matcher.Advance(-2).SetOpcodeAndAdvance(matcher.Opcode == OpCodes.Bgt_Un_S ? OpCodes.Br_S : OpCodes.Br);

            matcher.Advance(-3);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.Advance(1);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            return matcher.InstructionEnumeration();
        }

        public static bool IsSuit(PlanetData planet, BuildPreview preview)
            => preview.item.ID == (planet.type == EPlanetType.Gas ? ProtoIDUsedByPatches.I轨道采集器 : ProtoIDUsedByPatches.I大气采集器);

        [HarmonyPatch(typeof(PlanetTransport), "NewStationComponent")]
        [HarmonyPostfix]
        public static void PlanetTransport_NewStationComponent_Postfix(PlanetTransport __instance, PrefabDesc _desc, StationComponent __result)
        {
            if (__instance.planet.type != EPlanetType.Gas && __result.isCollector)
                for (var index = 0; index < Math.Min(__result.collectionIds.Length, _desc.stationMaxItemKinds); ++index)
                    __result.storage[index].localLogic = ELogisticStorage.Supply;
        }

        [HarmonyPatch(typeof(PlanetGen), "SetPlanetTheme")]
        [HarmonyPostfix]
        public static void PlanetGen_SetPlanetTheme_Postfix(PlanetData planet)
        {
            if (planet.type != EPlanetType.Gas)
            {
                planet.gasHeatValues = new float[planet.gasItems.Length];
                planet.gasTotalHeat = 0;
            }
        }

        [HarmonyPatch(typeof(PlanetTransport), "GameTick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetTransport_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, StationComponent_IsCollector_Field));

            var label = matcher.Advance(1).Operand;

            matcher.Advance(7).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetTransport), "planet")),
                                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(IsGas))),
                                                new CodeInstruction(OpCodes.Brfalse, label));

            return matcher.InstructionEnumeration();
        }

        public static bool IsGas(PlanetData planet) => planet.type == EPlanetType.Gas;

        [HarmonyPatch(typeof(UIBeltBuildTip), "SetOutputEntity")]
        [HarmonyPatch(typeof(UISlotPicker), "SetOutputEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetOutputEntity_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "isStellar")));

            var comp = matcher.Advance(-1).Instruction;
            var label = matcher.Advance(2).Operand;
            var is_S = matcher.Opcode == OpCodes.Brfalse_S;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(comp),
                                                new CodeInstruction(OpCodes.Ldfld, StationComponent_IsCollector_Field),
                                                new CodeInstruction(is_S ? OpCodes.Brtrue_S : OpCodes.Brtrue, label));

            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(StationComponent), "Init")]
        [HarmonyPostfix]
        public static void StationComponent_Init_Postfix(StationComponent __instance)
        {
            if (__instance.isCollector) __instance.warperMaxCount = 0;
        }
    }
}
