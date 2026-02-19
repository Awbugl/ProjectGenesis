using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable Unity.UnknownResource

namespace ProjectGenesis.Patches
{
    public static partial class ChemicalRecipeFcolPatches
    {
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(GameLogic), nameof(GameLogic._assembler_parallel))]
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
            Texture texture = TextureHelper.GetTexture("化工厂渲染索引");
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
