using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class IconSetPatches
    {
        [HarmonyPatch(typeof(IconSet), nameof(IconSet.Create))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IconSet_Create_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // 找到 iconSprite != null 的检查位置
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality")));

            // 获取跳转标签 (IL_06ff)
            object label = matcher.Advance(1).Operand;
            
            // 获取 techProto 局部变量
            object techProtoLocal = matcher.Advance(-7).Operand;

            // 移动到 brfalse IL_06ff 之后
            matcher.Advance(8);

            // 插入 ID 检查: if (techProto.ID >= 2000) goto IL_06ff
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_S, techProtoLocal),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TechProto), nameof(TechProto.ID))),
                new CodeInstruction(OpCodes.Ldc_I4, 2000),
                new CodeInstruction(OpCodes.Bge, label));

            return matcher.InstructionEnumeration();
        }
    }
}
