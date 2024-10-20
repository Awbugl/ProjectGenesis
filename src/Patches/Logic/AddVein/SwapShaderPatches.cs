using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static class SwapShaderPatches
    {
        private static readonly Dictionary<string, Shader> ReplaceShaderMap = new Dictionary<string, Shader>();

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.SaveMaterial))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool VFPreload_SaveMaterial_Prefix(Material mat)
        {
            if (mat == null) return false;

            ReplaceShaderIfAvailable(mat);

            return true;
        }

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.SaveMaterials), typeof(Material[]))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool VFPreload_SaveMaterials_Prefix(Material[] mats)
        {
            if (mats == null) return false;

            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                ReplaceShaderIfAvailable(mat);
            }

            return true;
        }

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.SaveMaterials), typeof(Material[][]))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool VFPreload_SaveMaterials_Prefix(Material[][] mats)
        {
            if (mats == null) return false;

            foreach (Material[] matarray in mats)
            {
                if (matarray == null) continue;

                foreach (Material mat in matarray)
                {
                    if (mat == null) continue;

                    ReplaceShaderIfAvailable(mat);
                }
            }

            return true;
        }

        internal static void AddSwapShaderMapping(string oriShaderName, Shader replacementShader) =>
            ReplaceShaderMap.Add(oriShaderName, replacementShader);

        private static void ReplaceShaderIfAvailable(Material mat)
        {
            string oriShaderName = mat.shader.name;

            if (ReplaceShaderMap.TryGetValue(oriShaderName, out Shader replacementShader)) mat.shader = replacementShader;
        }
    }
}
