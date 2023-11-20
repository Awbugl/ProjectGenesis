using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static class SwapShaderPatches
    {
        private static readonly Dictionary<string, Shader> replaceShaderMap = new Dictionary<string, Shader>();
        private static readonly Dictionary<string, Dictionary<string, Color>> replaceShaderProps = new Dictionary<string, Dictionary<string, Color>>();
        
        
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
                foreach (var mat in matarray)
                {
                    if (mat == null) continue;
                    ReplaceShaderIfAvailable(mat);
                }
            }

            return true;
        }

        public static void AddSwapShaderMapping(string oriShaderName, Shader replacementShader)
        {
            replaceShaderMap.Add(oriShaderName, replacementShader);
        }
        
        public static void AddShaderPropMapping(string oriShaderName, Dictionary<string, Color> newProps)
        {
            replaceShaderProps.Add(oriShaderName, newProps);
        }
        
        private static void ReplaceShaderIfAvailable(Material mat)
        {
            string oriShaderName = mat.shader.name;
            if (replaceShaderMap.TryGetValue(oriShaderName, out var replacementShader)) mat.shader = replacementShader;
            if (replaceShaderProps.TryGetValue(oriShaderName, out var newProps))
            {
                foreach (var prop in newProps) mat.SetColor(prop.Key, prop.Value);
            }
        }
    }
}