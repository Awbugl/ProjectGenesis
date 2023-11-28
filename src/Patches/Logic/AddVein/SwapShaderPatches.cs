using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static class SwapShaderPatches
    {
        private static readonly Dictionary<string, Shader> ReplaceShaderMap = new Dictionary<string, Shader>();

        private static readonly Dictionary<string, Dictionary<string, Color>>
            ReplaceShaderProps = new Dictionary<string, Dictionary<string, Color>>();

        [HarmonyPatch(typeof(VFPreload), "SaveMaterial")]
        [HarmonyPrefix]
        public static bool VFPreload_SaveMaterial_Prefix(Material mat)
        {
            if (mat == null) return false;
            ReplaceShaderIfAvailable(mat);
            return true;
        }

        [HarmonyPatch(typeof(VFPreload), "SaveMaterials", typeof(Material[]))]
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

        [HarmonyPatch(typeof(VFPreload), "SaveMaterials", typeof(Material[][]))]
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

        internal static void AddSwapShaderMapping(string oriShaderName, Shader replacementShader)
            => ReplaceShaderMap.Add(oriShaderName, replacementShader);

        internal static void AddShaderPropMapping(string oriShaderName, Dictionary<string, Color> newProps)
            => ReplaceShaderProps.Add(oriShaderName, newProps);

        internal static bool AddShaderPropMapping(string oriShaderName, string colorName, Color newColor)
        {
            if (!ReplaceShaderProps.TryGetValue(oriShaderName, out _)) ReplaceShaderProps[oriShaderName] = new Dictionary<string, Color>();

            return ReplaceShaderProps[oriShaderName].TryAdd(colorName, newColor);
        }

        private static void ReplaceShaderIfAvailable(Material mat)
        {
            string oriShaderName = mat.shader.name;
            if (ReplaceShaderMap.TryGetValue(oriShaderName, out Shader replacementShader)) mat.shader = replacementShader;
            if (ReplaceShaderProps.TryGetValue(oriShaderName, out Dictionary<string, Color> newProps))
                foreach (KeyValuePair<string, Color> prop in newProps)
                    mat.SetColor(prop.Key, prop.Value);
        }
    }
}
