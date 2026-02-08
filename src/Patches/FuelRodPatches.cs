using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class FuelRodPatches
    {
        private static readonly int[] FuelCells =
        {
            ProtoID.I氢燃料棒, ProtoID.I煤油燃料棒, ProtoID.I四氢双环戊二烯燃料棒, //
            ProtoID.I能量碎片,
        };

        internal static readonly int[] FuelRods =
        {
            ProtoID.I氢燃料棒, ProtoID.I煤油燃料棒, ProtoID.I四氢双环戊二烯燃料棒, //
            ProtoID.I铀燃料棒, ProtoID.I钚燃料棒, ProtoID.IMOX燃料棒,      //
            ProtoID.I氘核燃料棒, ProtoID.I氦三燃料棒, ProtoID.I氘氦混合聚变燃料棒, //
            ProtoID.I反物质燃料棒, ProtoID.I奇异燃料棒,                    //
            ProtoID.I满蓄电器, ProtoID.I能量碎片,
        };

        [HarmonyPatch(typeof(Mecha), nameof(Mecha.GenerateEnergy))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetCargoBytes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_I4_M1), new CodeMatch(OpCodes.Stloc_S));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FuelRodPatches), nameof(GenerateEnergy_Patch))));

            return matcher.InstructionEnumeration();
        }

        public static void GenerateEnergy_Patch(Mecha mecha)
        {
            int count = GetEmptyRodCount(mecha.reactorItemId);

            if (count == 0) return;

            mecha.player.TryAddItemToPackage(ProtoID.I空燃料棒, count, mecha.reactorItemInc, true);
            UIItemup.Up(ProtoID.I空燃料棒, count);
        }

        private static int GetEmptyRodCount(int itemId)
        {
            var count = 0;

            switch (itemId)
            {
                case ProtoID.I氢燃料棒:
                case ProtoID.I煤油燃料棒:
                case ProtoID.I四氢双环戊二烯燃料棒:
                case ProtoID.I铀燃料棒:
                case ProtoID.I钚燃料棒:
                case ProtoID.I氘核燃料棒:
                case ProtoID.I氦三燃料棒:
                case ProtoID.I反物质燃料棒:
                    count = 1;

                    break;

                case ProtoID.I氘氦混合聚变燃料棒:
                    count = 2;

                    break;

                case ProtoID.IMOX燃料棒:
                    count = 3;

                    break;

                case ProtoID.I奇异燃料棒:
                    count = 4;

                    break;
            }

            return count;
        }

        [HarmonyPatch(typeof(UIStorageGrid), nameof(UIStorageGrid.GetOtherStorageGrid), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageGrid_GetOtherStorageGrid_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.HeatValue))),
                new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Conv_I8));

            object label = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FuelRodPatches), nameof(IsFuelRod))),
                new CodeInstruction(OpCodes.Brfalse_S, label));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.LoadStatic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StorageComponent_LoadStatic_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.HeatValue))));

            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(FuelRodPatches), nameof(IsFuelRod)))
               .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_I8));

            return matcher.InstructionEnumeration();
        }

        public static bool IsFuelRod(ItemProto proto) => FuelRods.Contains(proto.ID) || proto.FuelType == 31;

        [HarmonyPatch(typeof(UIStorageGrid), nameof(UIStorageGrid.HandPut))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageGrid_HandPut_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "只能放入燃料"),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Localization), nameof(Localization.Translate))));

            matcher.SetAndAdvance(OpCodes.Ldstr, "只能放入燃料棒").SetAndAdvance(OpCodes.Call,
                AccessTools.Method(typeof(TranslateUtils), nameof(TranslateUtils.TranslateFromJson)));

            matcher.Advance(1);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "只能放入燃料"),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Localization), nameof(Localization.Translate))));

            matcher.SetAndAdvance(OpCodes.Ldstr, "只能放入燃料棒").SetAndAdvance(OpCodes.Call,
                AccessTools.Method(typeof(TranslateUtils), nameof(TranslateUtils.TranslateFromJson)));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.EntityFastFillIn))]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertInto), new[]
        {
            typeof(int), typeof(int), typeof(int), typeof(byte),
            typeof(byte), typeof(byte),
        }, new[]
        {
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Out,
        })]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertInto), new[]
        {
            typeof(uint), typeof(int), typeof(int), typeof(byte),
            typeof(byte), typeof(byte),
        }, new[]
        {
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Out,
        })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_OxygenMask_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
                int[] fuelNeed = ItemProto.fuelNeeds[(int) local5.fuelMask];

                IL_07e6: ldsfld       int32[][] ItemProto::fuelNeeds

                ... // powerGeneratorComponent

                IL_07ed: ldfld        int16 PowerGeneratorComponent::fuelMask
                IL_07f2: ldelem.ref
                IL_07f3: stloc.s      // fuelNeed
            */

            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))));
            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.SetAndAdvance(OpCodes.Ldarg_0, null);
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(FuelRodPatches), nameof(ThermalPowerGen_InsertMethod)));

            return matcher.InstructionEnumeration();
        }

        public static int[] ThermalPowerGen_InsertMethod(ref PowerGeneratorComponent component, PlanetFactory factory)
        {
            short componentFuelMask = component.fuelMask;

            if (componentFuelMask != 1) return ItemProto.fuelNeeds[componentFuelMask];

            return factory.planet.gasItems.Contains(ProtoID.I氧) ? ItemProto.fuelNeeds[1] : FuelCells;
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow.OnFuelButtonClick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSpawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Ldarg_0, null);
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(FuelRodPatches), nameof(UIPowerGenerator_InsertMethod)));

            return matcher.InstructionEnumeration();
        }

        public static int[] UIPowerGenerator_InsertMethod(UIPowerGeneratorWindow window)
        {
            PowerGeneratorComponent component = window.powerSystem.genPool[window.generatorId];

            short componentFuelMask = component.fuelMask;

            if (componentFuelMask != 1) return ItemProto.fuelNeeds[componentFuelMask];

            if (window.factory.planet.gasItems.Contains(ProtoID.I氧)) return ItemProto.fuelNeeds[1];

            int playerInhandItemId = window.player.inhandItemId;

            if (playerInhandItemId > 0 && window.player.inhandItemCount > 0)
            {
                if (playerInhandItemId == 1000) return null;

                if (FuelCells.Contains(playerInhandItemId)) return FuelCells;

                if (ItemProto.fuelNeeds[1].Contains(playerInhandItemId))
                {
                    UIRealtimeTip.Popup("需要氧气".TranslateFromJson());

                    return null;
                }
            }

            return FuelCells;
        }
    }
}
