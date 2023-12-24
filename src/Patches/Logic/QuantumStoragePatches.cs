using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class QuantumStoragePatches
    {
        private const int QuantumStorageSize = 90;

        private static StorageComponent _component = new StorageComponent(QuantumStorageSize);

        private static Dictionary<int, List<int>> _quantumStorageIds = new Dictionary<int, List<int>>();

        [HarmonyPatch(typeof(FactoryStorage), "NewStorageComponent")]
        [HarmonyPostfix]
        public static void FactoryStorage_NewStorageComponent(
            FactoryStorage __instance,
            int entityId,
            int size,
            ref StorageComponent __result)
        {
            if (size == QuantumStorageSize)
            {
                __instance.storagePool[__result.id] = _component;
                __result = _component;
            }
        }

        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.UpdateRaycast))]
        [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.DeterminePreviews))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Addon_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStorage))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                                                                 AccessTools.Method(typeof(QuantumStoragePatches),
                                                                                    nameof(BuildTool_Addon_PatchMethod))));
            
            return matcher.InstructionEnumeration();
        }

        public static bool BuildTool_Addon_PatchMethod(PrefabDesc desc) => desc.isStorage && desc.storageRow != 9;

        [HarmonyPatch(typeof(StorageComponent), "SetEmpty")]
        [HarmonyPrefix]
        public static bool StorageComponent_SetEmpty(StorageComponent __instance) => __instance.size != QuantumStorageSize;

        [HarmonyPatch(typeof(FactoryStorage), "TryTakeBackItems_Storage")]
        [HarmonyPostfix]
        public static void FactoryStorage_TryTakeBackItems_Storage(FactoryStorage __instance, int storageId, ref bool __result)
        {
            StorageComponent package = __instance.GetStorageComponent(storageId);
            if (package == null || package.size == QuantumStorageSize) __result = true;
        }

        [HarmonyPatch(typeof(UIStorageWindow), "_OnUpdate")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnSortClick")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnBansSliderValueChange")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterItemPickerReturn")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterButton2Click")]
        [HarmonyPatch(typeof(UIStorageWindow), "OnFilterButton3Click")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageWindow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))),
                                 new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(UIStorageWindow), nameof(UIStorageWindow.storageId))));

            object label = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool PatchMethod(StorageComponent component) => component.size == QuantumStorageSize;

        [HarmonyPatch(typeof(UIStorageWindow), "OnStorageIdChange")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStorageWindow_OnStorageIdChange_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);


            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EntityData), nameof(EntityData.protoId))));

            matcher.CreateLabelAt(matcher.Pos + 1, out Label label);

            matcher.Advance(-6).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, ProtoID.I量子储物仓), new CodeInstruction(OpCodes.Ldloc_0),
                                                 new CodeInstruction(OpCodes.Call,
                                                                     AccessTools.Method(typeof(QuantumStoragePatches), nameof(PatchMethod))),
                                                 new CodeInstruction(OpCodes.Brtrue_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactoryStorage), "Export")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactoryStorage_Export_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StorageComponent), nameof(StorageComponent.id))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).Advance(1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuantumStoragePatches), nameof(Export_PatchMethod))))
                   .SetOpcodeAndAdvance(OpCodes.Brtrue);

            return matcher.InstructionEnumeration();
        }

        public static bool Export_PatchMethod(StorageComponent component, FactoryStorage storage, int index)
        {
            bool b = component.size == QuantumStorageSize;
            if (b) _quantumStorageIds.TryAddOrInsert(storage.planet.id, index);
            return b;
        }

        private static void SetQuantumStorage()
        {
            GalaxyData galaxy = GameMain.data.galaxy;

            foreach (KeyValuePair<int, List<int>> pair in _quantumStorageIds)
            {
                foreach (int i in pair.Value)
                {
                    PlanetFactory planetFactory = galaxy.PlanetById(pair.Key)?.factory;
                    if (planetFactory == null) continue;

                    planetFactory.factoryStorage.storagePool[i] = _component;
                }
            }
        }

        internal static void Export(BinaryWriter w)
        {
            lock (_quantumStorageIds)
            {
                w.Write(_quantumStorageIds.Count);

                foreach (KeyValuePair<int, List<int>> pair in _quantumStorageIds)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Count);
                    foreach (int t in pair.Value) w.Write(t);
                }
            }

            lock (_component)
            {
                _component.Export(w);
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int storagecount = r.ReadInt32();

                for (int j = 0; j < storagecount; j++)
                {
                    int key = r.ReadInt32();
                    int length = r.ReadInt32();
                    var datas = new List<int>();
                    for (int i = 0; i < length; i++)
                    {
                        datas.Add(r.ReadInt32());
                    }

                    _quantumStorageIds.Add(key, datas);
                }

                _component.Import(r);

                SetQuantumStorage();
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll()
        {
            _quantumStorageIds = new Dictionary<int, List<int>>();
            _component = new StorageComponent(QuantumStorageSize);
        }
    }
}
