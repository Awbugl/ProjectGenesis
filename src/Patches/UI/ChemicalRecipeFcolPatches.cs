using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable Unity.UnknownResource

namespace ProjectGenesis.Patches.UI
{
    public static class ChemicalRecipeFcolPatches
    {
        private static readonly Dictionary<int, int> RecipeIdPos = new Dictionary<int, int>
        {
            { ProtoID.R二氧化碳, 431 },
            { ProtoID.R催化重整, 432 },
            { ProtoID.R四氢双环戊二烯, 433 },
            { ProtoID.R有机晶体重组, 434 },
            { ProtoID.R水电解, 435 },
            { ProtoID.R盐水电解, 436 },
            { ProtoID.R合成氨, 437 },
            { ProtoID.R三氯化铁, 438 },
            { ProtoID.R氨氧化, 439 },
            { ProtoID.R羰基合成, 440 },
            { ProtoID.R聚苯硫醚, 441 },
            { ProtoID.R聚酰亚胺, 442 },
            { ProtoID.R钨矿筛选, 443 },
            { ProtoID.R海水淡化, 444 },
            { ProtoID.R有机晶体活化, 445 },
            { ProtoID.R二氧化硫还原, 446 },
            { ProtoID.R增产剂, 447 },
            { ProtoID.R氦原子提取, 448 },
            { ProtoID.R硅石筛选, 449 },
        };

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool), typeof(int), typeof(int),
            typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeType))));

            matcher.Advance(1)
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ChemicalRecipeFcolPatches), nameof(ChemicalRecipeTypePatch))))
               .SetOpcodeAndAdvance(OpCodes.Brfalse_S);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeId))),
                new CodeMatch(OpCodes.Conv_R4),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.working_length))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ChemicalRecipeFcolPatches), nameof(ChemicalRecipeFcolPatch))));

            return matcher.InstructionEnumeration();
        }

        private static bool ChemicalRecipeTypePatch(int recipeType) => recipeType == (int)ERecipeType.Chemical || recipeType == 16;

        private static int ChemicalRecipeFcolPatch(int recipeId) => RecipeIdPos.GetValueOrDefault(recipeId, recipeId);

        internal static void SetChemicalRecipeFcol()
        {
            Texture texture = Resources.Load<Texture>("Assets/texpack/chemical-plant-recipe-fcol");
            int fluidTex = Shader.PropertyToID("_FluidTex");

            ref PrefabDesc prefabDesc = ref LDB.models.Select(64).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture(fluidTex, texture);
            prefabDesc.lodMaterials[1][1].SetTexture(fluidTex, texture);

            prefabDesc = ref LDB.models.Select(376).prefabDesc;
            prefabDesc.lodMaterials[0][1].SetTexture(fluidTex, texture);
            prefabDesc.lodMaterials[1][1].SetTexture(fluidTex, texture);
        }
    }
}
