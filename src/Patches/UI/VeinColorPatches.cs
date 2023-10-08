using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
using UnityEngine;
using xiaoye97;

namespace ProjectGenesis.Patches.UI
{
    public static class VeinColorPatches
    {
        [HarmonyPatch(typeof(PlanetFactory), "AddVeinData")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_AddVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.type))),
                                 new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(VeinColorPatches), nameof(EVeinTypeToAnimDataState))));

            return matcher.InstructionEnumeration();
        }

        // update with preloader

        public static uint EVeinTypeToAnimDataState(EVeinType type)
        {
            if (type > EVeinType.Max)
            {
                switch ((byte)type)
                {
                    case 16:
                        return 14;

                    case 17:
                        return 6;

                    case 18:
                        return 11;
                }
            }

            return (uint)type;
        }

        internal static void ModifyVeinData()
        {
            ref ModelProto emptyModel = ref LDB.models.modelArray[7];

            if (emptyModel.ID == 8)
            {
                ModelProto stoneModel = LDB.models.Select(2);
                emptyModel = stoneModel.Copy();

                emptyModel.ID = 8;
                emptyModel.Name = emptyModel.ID.ToString();
                PrefabDesc desc = stoneModel.prefabDesc;
                emptyModel.prefabDesc = new PrefabDesc(8, desc.prefab, desc.colliderPrefab);
            }

            RegisterNewVein(16, "铝矿", "I铝矿", "Assets/texpack/铝矿", 1001, 22);

            RegisterNewVein(17, "钨矿", "I钨矿", "Assets/texpack/钨矿", 1016, 22);

            RegisterNewVein(18, "放射性矿物", "I放射性矿物碎块", "Assets/texpack/放射性矿物碎块", 6222, 22, 4);

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
