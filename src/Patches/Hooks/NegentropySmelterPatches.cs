using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    public static class NegentropySmelterPatches
    {
        public static void GameTick_AssemblerComponent_InternalUpdate_Patch(PlanetFactory factory, ref AssemblerComponent component,
            float power)
        {
            if (factory.entityPool[component.entityId].protoId != ProtoID.I负熵熔炉 || !component.replicating) return;

            RecipeExecuteData data = component.recipeExecuteData;

            component.extraTime += (int)(power * component.extraSpeed)
                                 + (int)(power * component.speedOverride * data.extraTimeSpend / data.timeSpend);
        }
    }
}
