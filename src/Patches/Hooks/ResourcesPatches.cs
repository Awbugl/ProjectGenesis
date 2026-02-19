using System;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class ResourcesPatches
    {
        [HarmonyPatch(typeof(Resources), nameof(Resources.Load), typeof(string), typeof(Type))]
        [HarmonyBefore(CommonAPI.CommonAPIPlugin.GUID)]
        [HarmonyPrefix]
        public static bool GetAssemblyManifestResource(ref string path, Type systemTypeInstance, ref UnityEngine.Object __result)
        {
            const string texpackPrefix = "Assets/texpack/";

            if (!path.StartsWith(texpackPrefix)) return true;

            string name = path.Substring(texpackPrefix.Length);
            
            if (systemTypeInstance == typeof(Texture2D))
            {
                __result = TextureHelper.GetTexture(name);
                return __result == null;
            }

            if (systemTypeInstance == typeof(Sprite))
            {
                __result = TextureHelper.GetSprite(name);
                return __result == null;
            }

            return true;
        }
    }
}
