using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.UI
{
    public static class LaserMk2Patches
    {
        [HarmonyPatch(typeof(SkillSystem), nameof(SkillSystem.Init))]
        [HarmonyPostfix]
        public static void SkillSystem_Init(SkillSystem __instance)
        {
            RenderableObjectDesc turretLaserContinuousDesc = Configs.combat.turretLaserContinuousDesc;
            Material material = turretLaserContinuousDesc.materials[0];

            var value = new Color(0.8471f, 0.6078f, 1.0000f);
            material.SetColor("_BeamColor1", value);
            material.SetColor("_MuzzleWaveColor", value);
            material.SetColor("_PulseColor", value);
            material.SetColor("_ShortWaveColor", value);

            value = new Color(0.6118f, 0.0000f, 1.0000f);
            material.SetColor("_BeamColor2", value);

            value = new Color(0.7922f, 0.4627f, 1.0000f);
            material.SetColor("_LongWaveColor", value);

            value = new Color(0.7569f, 0.4078f, 1.0000f);
            material.SetColor("_MuzzleFlareColor", value);
        }
    }
}
