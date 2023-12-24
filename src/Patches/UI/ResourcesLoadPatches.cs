using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class ResourcesLoadPatches
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        [HarmonyPatch(typeof(Resources), nameof(Resources.Load), typeof(string), typeof(Type))]
        [HarmonyBefore(CommonAPI.CommonAPIPlugin.GUID)]
        [HarmonyPrefix]
        public static bool ResourcesLoad_Prefix(string path, Type systemTypeInstance, ref Object __result)
        {
            if (systemTypeInstance != typeof(Sprite)) return true;

            if (!Sprites.TryGetValue(path, out Sprite sprite)) return true;
            
            __result = sprite;
            return false;
        }

        [HarmonyPatch(typeof(Resources), nameof(Resources.Load), typeof(string), typeof(Type))]
        [HarmonyPostfix]
        public static void ResourcesLoad_Postfix(string path, Type systemTypeInstance, ref Object __result)
        {
            if (systemTypeInstance != typeof(Sprite)) return;

            if (Sprites.ContainsKey(path)) return;

            Sprites[path] = (Sprite)__result;
        }

        [HarmonyPatch(typeof(IconSet), nameof(IconSet.Create))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IconSet_Create_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(TechProto), nameof(TechProto.iconSprite))));

            object label = matcher.InstructionAt(5).operand;

            object index_V_23 = matcher.Advance(-2).Operand;
            object dataArray3 = matcher.Advance(-1).Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, dataArray3), new CodeInstruction(OpCodes.Ldloc_S, index_V_23),
                                     new CodeInstruction(OpCodes.Ldelem_Ref),
                                     new CodeInstruction(
                                         OpCodes.Call, AccessTools.Method(typeof(ResourcesLoadPatches), nameof(IconSet_Create_Patch))),
                                     new CodeInstruction(OpCodes.Brtrue_S, label));

            return matcher.InstructionEnumeration();
        }

        public static bool IconSet_Create_Patch(TechProto proto) => proto.ID < 2000;
    }
}
