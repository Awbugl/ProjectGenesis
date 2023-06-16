using HarmonyLib;
using ERecipeType_1 = ERecipeType;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.LithographyAssembler
{
    internal static partial class LithographyAssemblerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "TakeBackItems_Assembler")]
        public static void FactorySystem_TakeBackItems_Assembler(ref FactorySystem __instance, Player player, int asmId)
            => SetEmpty(__instance.planet.id, asmId);
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "EntityFastFillIn")]
        public static void PlanetFactory_EntityFastFillIn_Postfix(
            PlanetFactory __instance,
            int entityId,
            bool fromPackage,
            ItemBundle itemBundle)
        {
            if (__instance.factorySystem == null) return;
            var entityData = __instance.entityPool[entityId];
            if (entityData.id != entityId) return;
            ref var assemblerComponent = ref __instance.factorySystem.assemblerPool[entityData.assemblerId];
            if (assemblerComponent.id != entityData.assemblerId) return;
            if (assemblerComponent.recipeType != (ERecipeType_1)Utils_ERecipeType.电路蚀刻) return;
            var data = GetLithographyData(__instance.factorySystem.planet.id, entityData.assemblerId);

            var itemId = GetLithographyLenId(assemblerComponent.recipeId);

            if (itemId == 0) return;

            var mainPlayer = GameMain.mainPlayer;

            if (itemId != data.ItemId)
            {
                var upCount = mainPlayer.TryAddItemToPackage(data.ItemId, data.ItemCount, data.ItemInc, true);
                data.ItemCount = 0;
                UIItemup.Up(data.ItemId, upCount);
            }

            var itemCount = 1 - data.ItemCount;
            var itemInc = 0;
            if (itemCount > 0) mainPlayer.TakeItemFromPlayer(ref itemId, ref itemCount, out itemInc, fromPackage, itemBundle);
            if (itemCount > 0)
            {
                data.ItemId = itemId;
                data.ItemCount = itemCount;
                data.ItemInc = itemInc;
                SetLithographyData(__instance.factorySystem.planet.id, entityData.assemblerId, data);
            }
        }
    }
}
