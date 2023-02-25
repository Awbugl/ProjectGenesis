using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class PlanetGasPatches
    {
        private static readonly FieldInfo StationComponent_IsCollector_Field = AccessTools.Field(typeof(StationComponent), "isCollector"),
                                          PlanetData_GasItems_Field = AccessTools.Field(typeof(PlanetData), "gasItems"),
                                          StationComponent_isStellar_Field = AccessTools.Field(typeof(StationComponent), "isStellar"),
                                          PrefabDesc_isStellarStation_Field = AccessTools.Field(typeof(PrefabDesc), "isStellarStation");

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
                                                                     new CodeMatch(OpCodes.Ldc_I4_5));
            matcher.Advance(-1);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnPlanetDataSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false,
                                                                     new CodeMatch(OpCodes.Ldfld,
                                                                                   AccessTools.Field(typeof(StationComponent), "collectionPerTick")));

            var stationComponent = matcher.Advance(-1).Instruction;

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(stationComponent), new CodeInstruction(OpCodes.Ldarg_0),
                                                 new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIPlanetDetail), "_planet")),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(PlanetGasPatches), nameof(GetGasCollectionPerTick))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnStarDataSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "gasTotalHeat")));
            var planet = matcher.Advance(-1).Instruction;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "collectionPerTick")));
            var stationComponent = matcher.Advance(-1).Instruction;

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(stationComponent), new CodeInstruction(planet),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(PlanetGasPatches), nameof(GetGasCollectionPerTick))));

            return matcher.InstructionEnumeration();
        }

        public static double GetGasCollectionPerTick(double original, StationComponent stationComponent, PlanetData planet)
            => planet.type == EPlanetType.Gas ? original : GameMain.history.miningSpeedScale * stationComponent.collectSpeed;

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), "DetermineActive")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "DetermineActive")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditionsPrestage")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Blueprint_DetermineActive_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (BlueprintTweaksCompatibilityPlugin.BlueprintTweaksInstalled) return instructions;

            var matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                     new CodeMatch(OpCodes.Ldfld,
                                                                                   AccessTools.Field(typeof(BuildTool), nameof(BuildTool.planet))),
                                                                     new CodeMatch(OpCodes.Ldfld, PlanetData_GasItems_Field));

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
            if (BlueprintTweaksCompatibilityPlugin.BlueprintTweaksInstalled) return instructions;

            var matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                     new CodeMatch(OpCodes.Ldfld,
                                                                                   AccessTools.Field(typeof(PlayerController), "gameData")),
                                                                     new CodeMatch(OpCodes.Callvirt,
                                                                                   AccessTools.PropertyGetter(typeof(GameData), "localPlanet")),
                                                                     new CodeMatch(OpCodes.Ldfld, PlanetData_GasItems_Field));

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

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 14297f));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool), "planet")),
                                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(GetDistance))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelem_Ref),
                                 new CodeMatch(OpCodes.Ldfld, StationComponent_isStellar_Field));

            // StationComponent comp = stationPool[i];
            List<CodeInstruction> ins = matcher.InstructionsWithOffsets(0, 2);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = buildPreview1.desc;
            List<CodeInstruction> ins2 = matcher.InstructionsWithOffsets(-2, -1);

            // set actual distance
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(ins).InsertAndAdvance(ins2)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(SetDistance))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = itemProto.desc;
            List<CodeInstruction> ins3 = matcher.InstructionsWithOffsets(-2, -1);

            matcher.Advance(1).MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_isStellarStation_Field));

            // PrefabDesc desc = buildPreview1.desc;
            List<CodeInstruction> ins4 = matcher.InstructionsWithOffsets(-2, -1);

            // set actual distance
            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(ins3).InsertAndAdvance(ins4)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetGasPatches), nameof(SetPreBuildDistance))));


            return matcher.InstructionEnumeration();
        }

        public static bool IsSuit(PlanetData planet, BuildPreview preview)
            => preview.item.ID == (planet.type == EPlanetType.Gas ? ProtoIDUsedByPatches.I轨道采集器 : ProtoIDUsedByPatches.I大气采集器);

        public static float GetDistance(PlanetData planet) => planet.type == EPlanetType.Gas ? 14297f : 3845f;

        public static float SetDistance(float origin, StationComponent stationComponent, PrefabDesc prebuildDesc)
            => stationComponent.isCollector ^ prebuildDesc.isCollectStation ? 0 : origin;

        public static float SetPreBuildDistance(float origin, PrefabDesc prebuildDesc, PrefabDesc prebuildDesc2)
            => prebuildDesc.isCollectStation ^ prebuildDesc2.isCollectStation ? 0 : origin;

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

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(comp), new CodeInstruction(OpCodes.Ldfld, StationComponent_IsCollector_Field),
                                                new CodeInstruction(is_S ? OpCodes.Brtrue_S : OpCodes.Brtrue, label));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(StationComponent), "Init")]
        [HarmonyPostfix]
        public static void StationComponent_Init_Postfix(StationComponent __instance)
        {
            if (__instance.isCollector) __instance.warperMaxCount = 0;
        }

        [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
        [HarmonyPostfix]
        public static void StationComponent_UpdateCollection_Postfix(StationComponent __instance)
        {
            for (var i = 0; i < __instance.collectionIds.Length; i++)
            {
                lock (__instance.storage)
                {
                    if (__instance.storage[i].count < __instance.storage[i].max)
                    {
                        __instance.energy = 1;
                        return;
                    }
                }

                __instance.energy = 0;
            }
        }
    }
}
